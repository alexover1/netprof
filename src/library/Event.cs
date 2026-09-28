namespace Netprof;

/// <summary>
/// Internal representation of a profiling event.
///
/// This type is never constructed manually. A linked list of fixed-size event
/// chunks stores events for performant allocation and insertion. When an event
/// is started, its value of <see cref="EndTimestamp"/> will be zero. Whenever
/// the event is finished, its value of <see cref="EndTimestamp"/> will be set
/// to the current timestamp.
///
/// From the internal, per-thread stream of events, an array of <see cref="ProfileEvent"/>
/// will be constructed whenever <see cref="EventSession.Capture"/> is called.
/// </summary>
internal struct Event
{
    internal string? Name;
    internal long StartTimestamp;
    internal long EndTimestamp;
    internal int ParentIndex;
}

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
