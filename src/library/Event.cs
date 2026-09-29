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
