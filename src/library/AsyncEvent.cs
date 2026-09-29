namespace Netprof;

/// <summary>
/// Asynchronous counterpart to <see cref="Event"/>, for use within async methods
/// or methods that return a <see cref="Task"/>.
/// </summary>
internal readonly struct AsyncEvent
{
    internal AsyncEvent(
        string name,
        long spanId,
        long parentSpanId,
        long startTimestamp,
        long endTimestamp,
        int startThreadIndex,
        int endThreadIndex
    )
    {
        Name = name;
        SpanId = spanId;
        ParentSpanId = parentSpanId;
        StartTimestamp = startTimestamp;
        EndTimestamp = endTimestamp;
        StartThreadIndex = startThreadIndex;
        EndThreadIndex = endThreadIndex;
    }

    internal readonly string Name;
    internal readonly long SpanId;
    internal readonly long ParentSpanId;
    internal readonly long StartTimestamp;
    internal readonly long EndTimestamp;
    internal readonly int StartThreadIndex;
    internal readonly int EndThreadIndex;
}
