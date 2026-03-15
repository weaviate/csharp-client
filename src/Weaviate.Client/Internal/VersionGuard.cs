using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Weaviate.Client.Internal;

/// <summary>
/// Checks that the connected Weaviate server version satisfies the minimum version declared
/// by <see cref="RequiresWeaviateVersionAttribute"/> on the calling method.
/// Results are cached per (type, method name) pair after the first lookup.
/// </summary>
internal static class VersionGuard
{
    private static readonly ConcurrentDictionary<(Type, string, Type[]?), Version?> _cache = new();

    /// <summary>
    /// Throws <see cref="WeaviateVersionMismatchException"/> if <paramref name="serverVersion"/>
    /// is lower than the version declared on the method in <typeparamref name="TCallerType"/>
    /// identified by <paramref name="operationName"/>.
    /// Does nothing when <paramref name="serverVersion"/> is <c>null</c> (version unknown) or
    /// when the method carries no <see cref="RequiresWeaviateVersionAttribute"/>.
    /// </summary>
    /// <typeparam name="TCallerType">The class that declares the annotated method.</typeparam>
    /// <param name="serverVersion">The connected server version, or <c>null</c> if unknown.</param>
    /// <param name="operationName">
    /// Automatically supplied by <see cref="CallerMemberNameAttribute"/>. Do not pass explicitly.
    /// </param>
    internal static void Check<TCallerType>(
        Version? serverVersion,
        [CallerMemberName] string operationName = ""
    )
    {
        if (serverVersion is null)
            return;

        // Disambiguate known ambiguous overloads by parameter types
        Type[]? paramTypes = null;
        if (typeof(TCallerType).Name == "BatchManager" && operationName == "InsertMany")
        {
            // Try to match the most common overloads used in tests and client
            // Note: This can be extended if more overloads need version checks
            // IEnumerable<BatchInsertRequest>, BatchOptions, CancellationToken
            paramTypes = new[]
            {
                typeof(System.Collections.Generic.IEnumerable<>).MakeGenericType(
                    typeof(Weaviate.Client.Models.BatchInsertRequest)
                ),
                typeof(Weaviate.Client.Batch.BatchOptions),
                typeof(System.Threading.CancellationToken),
            };
        }

        var required = _cache.GetOrAdd(
            (typeof(TCallerType), operationName, paramTypes),
            static key =>
            {
                MethodInfo? method;
                if (key.Item3 != null)
                {
                    // Use parameter types to disambiguate
                    method = key.Item1.GetMethod(key.Item2, key.Item3);
                }
                else
                {
                    method = key.Item1.GetMethod(key.Item2);
                }
                return method
                    ?.GetCustomAttribute<RequiresWeaviateVersionAttribute>()
                    ?.MinimumVersion;
            }
        );

        if (required is not null && serverVersion < required)
        {
            throw new WeaviateVersionMismatchException(operationName, required, serverVersion);
        }
    }
}
