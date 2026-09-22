using System.Diagnostics;

namespace Netprof;

/// <summary>
/// A disposable profile zone associated with a reference to a particular <see cref="Counter"/> instance.
///
/// Constructing a new <see cref="Zone"/> corresponds to opening the referenced counter.
/// Disposing the zone corresponds to closing the referenced counter.
public ref struct Zone : IDisposable
{
    private readonly Profiler _profiler;
    private readonly int _index;
    private readonly int _parentIndex;
    private readonly long _oldInclusiveTicks;
    private readonly long _startTimestamp;

    internal Zone(Profiler profiler, int index)
    {
        _profiler = profiler;
        _index = index;
        _parentIndex = Profiler.ParentIndex;
        _oldInclusiveTicks = _profiler.Counters[index].InclusiveTicks;

        Profiler.ParentIndex = index;

        _startTimestamp = Stopwatch.GetTimestamp();
    }

    public void Dispose()
    {
        var elapsedTicks = Stopwatch.GetTimestamp() - _startTimestamp;
        Profiler.ParentIndex = _parentIndex;

        ref Counter parent = ref _profiler.Counters[_parentIndex];
        ref Counter counter = ref _profiler.Counters[_index];

        parent.ExclusiveTicks -= elapsedTicks;
        counter.ExclusiveTicks += elapsedTicks;
        counter.InclusiveTicks = _oldInclusiveTicks + elapsedTicks;
        counter.HitCount += 1;
    }
}
