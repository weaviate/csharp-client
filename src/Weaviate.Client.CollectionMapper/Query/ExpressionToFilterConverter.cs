using System.Linq.Expressions;
using Weaviate.Client.CollectionMapper.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Query;

/// <summary>
/// Converts C# lambda expressions to Weaviate Filter objects.
/// Supports binary operations, method calls, and nested property access.
/// </summary>
public static class ExpressionToFilterConverter
{
    /// <summary>
    /// Converts a lambda expression predicate to a Weaviate Filter.
    /// </summary>
    /// <typeparam name="T">The type being filtered.</typeparam>
    /// <param name="predicate">The predicate expression (e.g., x => x.Age > 18).</param>
    /// <returns>A Weaviate Filter object.</returns>
    /// <example>
    /// <code>
    /// var filter = ExpressionToFilterConverter.Convert&lt;Person&gt;(p => p.Age > 18);
    /// var filter2 = ExpressionToFilterConverter.Convert&lt;Article&gt;(a => a.Title.Contains("hello"));
    /// </code>
    /// </example>
    public static Filter Convert<T>(Expression<Func<T, bool>> predicate)
    {
        return ConvertExpression(predicate.Body);
    }

    /// <summary>
    /// Recursively converts an expression to a Filter.
    /// </summary>
    private static Filter ConvertExpression(Expression expression)
    {
        return expression switch
        {
            // Binary expressions: a.Size > 100, a.Name == "test", etc.
            BinaryExpression binary => ConvertBinaryExpression(binary),

            // Method calls: a.Name.Contains("test"), a.Tags.ContainsAny([...])
            MethodCallExpression method => ConvertMethodCall(method),

            // Member access: a.IsActive (bool property)
            MemberExpression member => ConvertMemberExpression(member),

            // Unary: !a.IsActive
            UnaryExpression unary => ConvertUnaryExpression(unary),

            _ => throw new NotSupportedException(
                $"Expression type '{expression.NodeType}' is not supported in filters. "
                    + $"Supported: ==, !=, >, <, >=, <=, &&, ||, !, Contains, ContainsAny, ContainsAll"
            ),
        };
    }

    /// <summary>
    /// Converts binary expressions (comparisons and logical operators).
    /// </summary>
    private static Filter ConvertBinaryExpression(BinaryExpression binary)
    {
        // Handle logical operators (&&, ||)
        if (binary.NodeType == ExpressionType.AndAlso)
        {
            return Filter.And(ConvertExpression(binary.Left), ConvertExpression(binary.Right));
        }

        if (binary.NodeType == ExpressionType.OrElse)
        {
            return Filter.Or(ConvertExpression(binary.Left), ConvertExpression(binary.Right));
        }

        // Handle comparison operators
        // Left side must be a property access
        if (binary.Left is not MemberExpression memberExpr)
        {
            throw new NotSupportedException(
                $"Left side of comparison must be a property access. Got: {binary.Left.NodeType}"
            );
        }

        var propertyPath = PropertyHelper.GetNestedPropertyPath(memberExpr);
        var value = EvaluateExpression(binary.Right);

        return binary.NodeType switch
        {
            ExpressionType.Equal => Filter.Property(propertyPath).Equal(value),
            ExpressionType.NotEqual => Filter.Property(propertyPath).NotEqual(value),
            ExpressionType.GreaterThan => Filter.Property(propertyPath).GreaterThan(value),
            ExpressionType.GreaterThanOrEqual => Filter
                .Property(propertyPath)
                .GreaterThanEqual(value),
            ExpressionType.LessThan => Filter.Property(propertyPath).LessThan(value),
            ExpressionType.LessThanOrEqual => Filter.Property(propertyPath).LessThanEqual(value),
            _ => throw new NotSupportedException(
                $"Binary operator '{binary.NodeType}' is not supported"
            ),
        };
    }

    /// <summary>
    /// Converts method call expressions.
    /// </summary>
    private static Filter ConvertMethodCall(MethodCallExpression method)
    {
        var methodName = method.Method.Name;

        // String.Contains
        if (methodName == "Contains" && method.Object is MemberExpression member)
        {
            var propertyPath = PropertyHelper.GetNestedPropertyPath(member);
            var value = EvaluateExpression(method.Arguments[0]);
            return Filter.Property(propertyPath).Like($"%{value}%");
        }

        // List/Array Contains (for checking if a collection contains a value)
        if (methodName == "Contains" && method.Arguments.Count == 2)
        {
            var propertyPath = PropertyHelper.GetNestedPropertyPath(
                (MemberExpression)method.Arguments[0]
            );
            var value = EvaluateExpression(method.Arguments[1]);
            return Filter.Property(propertyPath).ContainsAny(new[] { value });
        }

        // Extension methods or custom methods
        // ContainsAny, ContainsAll, ContainsNone
        if (methodName is "ContainsAny" or "ContainsAll" or "ContainsNone")
        {
            if (method.Arguments.Count < 2)
                throw new NotSupportedException(
                    $"Method '{methodName}' requires at least 2 arguments"
                );

            var propertyPath = PropertyHelper.GetNestedPropertyPath(
                (MemberExpression)method.Arguments[0]
            );
            var values =
                EvaluateExpression(method.Arguments[1]) as System.Collections.IEnumerable
                ?? throw new NotSupportedException(
                    $"Second argument to '{methodName}' must be enumerable"
                );

            var valueList = values.Cast<object>().ToList();

            return methodName switch
            {
                "ContainsAny" => Filter.Property(propertyPath).ContainsAny(valueList),
                "ContainsAll" => Filter.Property(propertyPath).ContainsAll(valueList),
                "ContainsNone" => Filter.Property(propertyPath).ContainsNone(valueList),
                _ => throw new NotSupportedException(),
            };
        }

        throw new NotSupportedException(
            $"Method '{methodName}' is not supported in filters. "
                + $"Supported methods: Contains, ContainsAny, ContainsAll, ContainsNone"
        );
    }

    /// <summary>
    /// Converts member expressions (bool properties).
    /// </summary>
    private static Filter ConvertMemberExpression(MemberExpression member)
    {
        // For boolean properties: a.IsActive => a.IsActive == true
        var propertyPath = PropertyHelper.GetNestedPropertyPath(member);
        return Filter.Property(propertyPath).Equal(true);
    }

    /// <summary>
    /// Converts unary expressions (negation).
    /// </summary>
    private static Filter ConvertUnaryExpression(UnaryExpression unary)
    {
        if (unary.NodeType == ExpressionType.Not)
        {
            return Filter.Not(ConvertExpression(unary.Operand));
        }

        throw new NotSupportedException($"Unary operator '{unary.NodeType}' is not supported");
    }

    /// <summary>
    /// Evaluates an expression to get its constant value.
    /// </summary>
    private static object EvaluateExpression(Expression expression)
    {
        // Handle constants directly
        if (expression is ConstantExpression constant)
        {
            return constant.Value
                ?? throw new InvalidOperationException("Filter value cannot be null");
        }

        // Handle member access (variables, fields, properties)
        if (expression is MemberExpression memberExpr)
        {
            // Compile and evaluate
            var lambda = Expression.Lambda(memberExpr);
            var value = lambda.Compile().DynamicInvoke();
            return value ?? throw new InvalidOperationException("Filter value cannot be null");
        }

        // For more complex expressions, compile and evaluate
        try
        {
            var lambda = Expression.Lambda(expression);
            var value = lambda.Compile().DynamicInvoke();
            return value ?? throw new InvalidOperationException("Filter value cannot be null");
        }
        catch (Exception ex)
        {
            throw new NotSupportedException(
                $"Unable to evaluate expression: {expression}. "
                    + $"Filters must use constant values or variables, not computed values. "
                    + $"Error: {ex.Message}",
                ex
            );
        }
    }
}
