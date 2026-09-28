namespace Netprof;

/// <summary>
/// A completed synchronous profiling event on one physical thread.
///
/// When a scope is closed, any zones defined in that scope will be disposed,
/// and an event will be written to the event stream indicating the values of
/// <c>Stopwatch.GetTimestamp()</c> at the start and end of the scope.
///
/// Events form a hierarchical tree structure. A single event may contain zero
/// or more child zones which account for a subsection of the event's total
/// duration.
/// </summary>
public readonly struct ProfileEvent
{
    internal ProfileEvent(string name, long startTimestamp, long endTimestamp, int parentIndex)
    {
        Name = name;
        StartTimestamp = startTimestamp;
        EndTimestamp = endTimestamp;
        ParentIndex = parentIndex;
    }

    public string Name { get; }
    public long StartTimestamp { get; }
    public long EndTimestamp { get; }
    public int ParentIndex { get; }
    public long DurationTicks => EndTimestamp - StartTimestamp;
}

/// <summary>
/// A completed async profiling event that may cross physical threads.
///
/// While awaiting a Task usually immediately runs on the same thread,
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
