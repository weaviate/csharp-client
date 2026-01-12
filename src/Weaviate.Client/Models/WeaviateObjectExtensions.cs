using System.Dynamic;

namespace Weaviate.Client.Models;

/// <summary>
/// The weaviate object extensions class
/// </summary>
public static class WeaviateObjectExtensions
{
    /// <summary>
    /// Converts the obj
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="obj">The obj</param>
    /// <returns>The</returns>
    public static T? As<T>(this WeaviateObject obj)
        where T : new()
    {
        return Internal.ObjectHelper.UnmarshallProperties<T>(obj.Properties);
    }

    /// <summary>
    /// Does the obj
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="obj">The obj</param>
    /// <param name="action">The action</param>
    public static void Do<T>(this WeaviateObject obj, Action<T> action)
        where T : new()
    {
        var data = obj.As<T>();
        if (data is not null)
        {
            action(data);
        }
    }

    /// <summary>
    /// Does the obj
    /// </summary>
    /// <param name="obj">The obj</param>
    /// <param name="action">The action</param>
    public static void Do(this WeaviateObject obj, Action<dynamic> action)
    {
        obj.Do<ExpandoObject>(action);
    }

    /// <summary>
    /// Gets the obj
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="obj">The obj</param>
    /// <param name="func">The func</param>
    /// <returns>The result</returns>
    public static TResult? Get<TSource, TResult>(
        this WeaviateObject obj,
        Func<TSource, TResult> func
    )
        where TSource : new()
    {
        var data = obj.As<TSource>();
        if (data is not null)
        {
            return func(data);
        }
        return default;
    }

    /// <summary>
    /// Gets the obj
    /// </summary>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="obj">The obj</param>
    /// <param name="func">The func</param>
    /// <returns>The result</returns>
    public static TResult? Get<TResult>(this WeaviateObject obj, Func<dynamic, TResult> func)
    {
        return obj.Get<ExpandoObject, TResult>(func);
    }
}
