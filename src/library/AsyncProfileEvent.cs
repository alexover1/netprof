namespace Netprof;

/// <summary>
/// A completed async profiling event that may cross physical threads.
///
/// While awaiting a Task usually immediately continues on the same thread,
/// async methods do not always run on the same thread they are awaited from,
/// in the case that they must wait for IO, network operations, or are wrapped
/// in <see cref="Task.Run"/> or <c>Task.ConfigureAwait(false)</c>.
/// </summary>
public readonly struct AsyncProfileEvent
{
    internal AsyncProfileEvent(
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

    public string Name { get; }
    public long SpanId { get; }
    public long ParentSpanId { get; }
    public long StartTimestamp { get; }
    public long EndTimestamp { get; }
    public int StartThreadIndex { get; }
    public int EndThreadIndex { get; }
    public long DurationTicks => EndTimestamp - StartTimestamp;
}
