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
