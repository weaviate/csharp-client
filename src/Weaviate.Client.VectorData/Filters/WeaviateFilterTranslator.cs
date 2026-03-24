using System.Linq.Expressions;
using Weaviate.Client.Models;
using Weaviate.Client.VectorData.Mapping;

namespace Weaviate.Client.VectorData.Filters;

/// <summary>
/// Translates LINQ <see cref="Expression{TDelegate}"/> filter expressions to Weaviate <see cref="Filter"/>.
/// </summary>
internal static class WeaviateFilterTranslator
{
    /// <summary>
    /// Translates a LINQ filter expression to a Weaviate <see cref="Filter"/>,
    /// or returns null if no filter is provided.
    /// </summary>
    public static Filter? Translate<TRecord>(Expression<Func<TRecord, bool>>? filter)
    {
        if (filter == null)
            return null;

        return TranslateExpression(filter.Body);
    }

    private static Filter TranslateExpression(Expression expression)
    {
        return expression switch
        {
            BinaryExpression binary => TranslateBinary(binary),
            UnaryExpression { NodeType: ExpressionType.Not } unary => TranslateNot(unary),
            MethodCallExpression method => TranslateMethodCall(method),
            _ => throw new NotSupportedException(
                $"Expression type '{expression.NodeType}' is not supported by the Weaviate VectorData filter translator."
            ),
        };
    }

    private static Filter TranslateBinary(BinaryExpression binary)
    {
        // Logical operators: && and ||
        if (binary.NodeType == ExpressionType.AndAlso)
        {
            return Filter.AllOf(
                TranslateExpression(binary.Left),
                TranslateExpression(binary.Right)
            );
        }

        if (binary.NodeType == ExpressionType.OrElse)
        {
            return Filter.AnyOf(
                TranslateExpression(binary.Left),
                TranslateExpression(binary.Right)
            );
        }

        // Comparison operators
        var (propertyName, value, reversed) = ExtractPropertyAndValue(binary);
        var prop = Filter.Property(propertyName);
        // When the expression is reversed (e.g. 30 > x.Age), we must invert the operator
        var nodeType = reversed ? InvertComparison(binary.NodeType) : binary.NodeType;

        return nodeType switch
        {
            ExpressionType.Equal when value == null => prop.IsNull(true),
            ExpressionType.Equal => prop.IsEqual(value),
            ExpressionType.NotEqual when value == null => prop.IsNull(false),
            ExpressionType.NotEqual => prop.IsNotEqual(value),
            ExpressionType.GreaterThan => prop.IsGreaterThan(value),
            ExpressionType.GreaterThanOrEqual => prop.IsGreaterThanEqual(value),
            ExpressionType.LessThan => prop.IsLessThan(value),
            ExpressionType.LessThanOrEqual => prop.IsLessThanEqual(value),
            _ => throw new NotSupportedException(
                $"Binary operator '{binary.NodeType}' is not supported by the Weaviate VectorData filter translator."
            ),
        };
    }

    private static Filter TranslateNot(UnaryExpression unary)
    {
        var inner = TranslateExpression(unary.Operand);
        return Filter.Not(inner);
    }

    private static Filter TranslateMethodCall(MethodCallExpression method)
    {
        // Handle Enumerable.Contains / List.Contains for tag filtering
        if (method.Method.Name == "Contains")
        {
            // instance.Contains(value) pattern — e.g., x.Tags.Contains("foo")
            if (method.Object != null)
            {
                var propertyName = ExtractMemberName(method.Object);
                var value = EvaluateExpression(method.Arguments[0]);
                return BuildContainsAny(propertyName, value);
            }

            // Enumerable.Contains(collection, value) static pattern
            if (method.Arguments.Count == 2)
            {
                var propertyName = ExtractMemberName(method.Arguments[0]);
                var value = EvaluateExpression(method.Arguments[1]);
                return BuildContainsAny(propertyName, value);
            }
        }

        throw new NotSupportedException(
            $"Method '{method.Method.Name}' is not supported by the Weaviate VectorData filter translator. "
                + "Supported: ==, !=, >, >=, <, <=, &&, ||, !, .Contains()"
        );
    }

    private static (string PropertyName, object? Value, bool Reversed) ExtractPropertyAndValue(
        BinaryExpression binary
    )
    {
        // Try left=property, right=value
        if (TryExtractMemberName(binary.Left, out var leftName))
        {
            var value = EvaluateExpression(binary.Right);
            return (leftName, value, false);
        }

        // Try right=property, left=value (reversed comparison — operator must be inverted)
        if (TryExtractMemberName(binary.Right, out var rightName))
        {
            var value = EvaluateExpression(binary.Left);
            return (rightName, value, true);
        }

        throw new NotSupportedException(
            "Filter expression must compare a property to a constant value."
        );
    }

    private static Filter BuildContainsAny(string propertyName, object? value)
    {
        var prop = Filter.Property(propertyName);
        return value switch
        {
            string s => prop.ContainsAny<string>([s]),
            int i => prop.ContainsAny<int>([i]),
            long l => prop.ContainsAny<long>([l]),
            float f => prop.ContainsAny<float>([f]),
            double d => prop.ContainsAny<double>([d]),
            bool b => prop.ContainsAny<bool>([b]),
            _ => prop.ContainsAny<string>([value?.ToString() ?? ""]),
        };
    }

    private static ExpressionType InvertComparison(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.GreaterThan => ExpressionType.LessThan,
            ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
            ExpressionType.LessThan => ExpressionType.GreaterThan,
            ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
            // Equal and NotEqual are symmetric — no inversion needed
            _ => nodeType,
        };
    }

    private static string ExtractMemberName(Expression expression)
    {
        if (TryExtractMemberName(expression, out var name))
            return name;

        throw new NotSupportedException(
            $"Cannot extract property name from expression of type '{expression.NodeType}'."
        );
    }

    private static bool TryExtractMemberName(Expression expression, out string name)
    {
        // Unwrap conversions (e.g., boxing for value types)
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            expression = unary.Operand;

        if (expression is MemberExpression member && member.Expression is ParameterExpression)
        {
            name = RecordPropertyModel.Decapitalize(member.Member.Name);
            return true;
        }

        name = "";
        return false;
    }

    private static object? EvaluateExpression(Expression expression)
    {
        // Unwrap conversions
        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            expression = unary.Operand;

        // Constant value
        if (expression is ConstantExpression constant)
            return constant.Value;

        // Captured variable (closure) — compile and evaluate
        try
        {
            var lambda = Expression.Lambda(expression);
            var compiled = lambda.Compile();
            return compiled.DynamicInvoke();
        }
        catch (Exception ex)
        {
            throw new NotSupportedException(
                $"Cannot evaluate expression of type '{expression.NodeType}' as a constant value.",
                ex
            );
        }
    }
}
