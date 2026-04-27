namespace Weaviate.Client.Models;

/// <summary>
/// Represents a running export operation with status tracking and cancellation.
/// </summary>
public class ExportOperation : ExportOperationBase
{
    internal ExportOperation(
        Export initial,
        Func<CancellationToken, Task<Export>> statusFetcher,
        Func<CancellationToken, Task<bool>> operationCancel
    )
        : base(initial, statusFetcher, operationCancel) { }
}
