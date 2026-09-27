namespace Netprof;

/// <summary>
/// A completed synchronous profiling event on one physical thread.
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
/// A completed logical async profiling event that may cross physical threads.
/// </summary>
public readonly struct AsyncProfileEvent
{
    internal AsyncProfileEvent(string name, long spanId, long parentSpanId, long startTimestamp, long endTimestamp,
        int startThreadIndex, int endThreadIndex)
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

internal struct EventRecord
{
    internal string? Name;
    internal long StartTimestamp;
    internal long EndTimestamp;
    internal int ParentIndex;
}

internal readonly struct AsyncEventRecord
{
    internal AsyncEventRecord(string name, long spanId, long parentSpanId, long startTimestamp, long endTimestamp,
        int startThreadIndex, int endThreadIndex)
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
