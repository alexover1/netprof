namespace Netprof;

/// <summary>
/// Immutable event data for one physical thread in a recording.
/// </summary>
public sealed class ProfileThread
{
    internal ProfileThread(int threadIndex, int managedThreadId, string? name, ProfileEvent[] events, AsyncProfileEvent[] asyncEvents)
    {
        ThreadIndex = threadIndex;
        ManagedThreadId = managedThreadId;
        Name = name;
        Events = events;
        AsyncEvents = asyncEvents;
    }

    public int ThreadIndex { get; }
    public int ManagedThreadId { get; }
    public string? Name { get; }
    public IReadOnlyList<ProfileEvent> Events { get; }
    public IReadOnlyList<AsyncProfileEvent> AsyncEvents { get; }
}

/// <summary>
/// Immutable snapshot of one completed profiling recording.
/// </summary>
public sealed class ProfileRecording
{
    internal ProfileRecording(long timestampFrequency, long startTimestamp, long endTimestamp, ProfileThread[] threads)
    {
        TimestampFrequency = timestampFrequency;
        StartTimestamp = startTimestamp;
        EndTimestamp = endTimestamp;
        Threads = threads;
    }

    public long TimestampFrequency { get; }
    public long StartTimestamp { get; }
    public long EndTimestamp { get; }
    public IReadOnlyList<ProfileThread> Threads { get; }
    public long DurationTicks => EndTimestamp - StartTimestamp;
    public TimeSpan Duration => TimeSpan.FromSeconds((double)DurationTicks / TimestampFrequency);
}
