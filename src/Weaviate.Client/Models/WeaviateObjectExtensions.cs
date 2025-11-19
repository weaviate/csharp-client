using System.Dynamic;

namespace Weaviate.Client.Models;

public static class WeaviateObjectExtensions
{
    public static T? As<T>(this WeaviateObject obj)
        where T : new()
    {
        return ObjectHelper.UnmarshallProperties<T>(obj.Properties);
    }

    public static void Do<T>(this WeaviateObject obj, Action<T> action)
        where T : new()
    {
        var data = obj.As<T>();
        if (data is not null)
        {
            action(data);
        }
    }

    public static void Do(this WeaviateObject obj, Action<dynamic> action)
    {
        obj.Do<ExpandoObject>(action);
    }

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

    public static TResult? Get<TResult>(this WeaviateObject obj, Func<dynamic, TResult> func)
    {
        return obj.Get<ExpandoObject, TResult>(func);
    }
}
