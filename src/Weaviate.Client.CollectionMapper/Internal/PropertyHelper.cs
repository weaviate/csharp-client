using System.Linq.Expressions;
using Humanizer;

namespace Weaviate.Client.CollectionMapper.Internal;

/// <summary>
/// Internal helper utilities for property name manipulation and expression parsing.
/// </summary>
internal static class PropertyHelper
{
    /// <summary>
    /// Converts a C# property name to Weaviate property name (camelCase).
    /// Uses Humanizer for consistent transformation.
    /// </summary>
    /// <param name="propertyName">The C# property name (e.g., "MyProperty").</param>
    /// <returns>The Weaviate property name (e.g., "myProperty").</returns>
    public static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return propertyName.Camelize();
    }

    /// <summary>
    /// Extracts the property name from a lambda expression: x => x.Property
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <typeparam name="TProp">The property type.</typeparam>
    /// <param name="selector">The property selector expression.</param>
    /// <returns>The property name in PascalCase.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a simple property access.</exception>
    public static string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> selector)
    {
        return selector.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression unaryMember } => unaryMember.Member.Name,
            _ => throw new ArgumentException(
                "Expression must be a property access (e.g., x => x.Property)",
                nameof(selector)
            ),
        };
    }

    /// <summary>
    /// Extracts nested property path from a member expression.
    /// Example: x.Category.Name -> "category.name"
    /// </summary>
    /// <param name="member">The member expression.</param>
    /// <returns>The property path in camelCase, dot-separated.</returns>
    public static string GetNestedPropertyPath(MemberExpression member)
    {
        var parts = new Stack<string>();
        var current = member;

        while (current != null)
        {
            parts.Push(ToCamelCase(current.Member.Name));

            if (current.Expression is MemberExpression parent)
                current = parent;
            else
                break;
        }

        return string.Join(".", parts);
    }

    /// <summary>
    /// Extracts property names from a new expression: x => new { x.Prop1, x.Prop2 }
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <param name="selector">The selector expression.</param>
    /// <returns>List of property names in PascalCase.</returns>
    public static List<string> GetPropertyNames<T>(Expression<Func<T, object>> selector)
    {
        var properties = new List<string>();

        switch (selector.Body)
        {
            case MemberExpression member:
                properties.Add(member.Member.Name);
                break;

            case NewExpression newExpr:
                properties.AddRange(newExpr.Members?.Select(m => m.Name) ?? []);
                break;

            case UnaryExpression { Operand: MemberExpression unaryMember }:
                properties.Add(unaryMember.Member.Name);
                break;

            default:
                throw new ArgumentException(
                    "Expression must be a property access or anonymous type initializer",
                    nameof(selector)
                );
        }

        return properties;
    }

    /// <summary>
    /// Gets the property names in camelCase from a selector expression.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <param name="selector">The selector expression.</param>
    /// <returns>List of property names in camelCase.</returns>
    public static List<string> GetCamelCasePropertyNames<T>(Expression<Func<T, object>> selector)
    {
        return GetPropertyNames(selector).Select(ToCamelCase).ToList();
    }
}
