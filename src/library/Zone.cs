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
    private readonly int _counterIndex;
    private readonly int _parentCounterIndex;
    private readonly long _oldInclusiveTicks;
    private readonly long _startTimestamp;

    internal Zone(Profiler profiler, int counterIndex)
    {
        _profiler = profiler;
        _counterIndex = counterIndex;
        _parentCounterIndex = Profiler.CurrentCounterIndex;
        _oldInclusiveTicks = _profiler.Counters[counterIndex].InclusiveTicks;

        Profiler.CurrentCounterIndex = counterIndex;

        _startTimestamp = Stopwatch.GetTimestamp();
    }

    public void Dispose()
    {
        var elapsedTicks = Stopwatch.GetTimestamp() - _startTimestamp;
        Profiler.CurrentCounterIndex = _parentCounterIndex;

        ref Counter parent = ref _profiler.Counters[_parentCounterIndex];
        ref Counter counter = ref _profiler.Counters[_counterIndex];

        parent.ExclusiveTicks -= elapsedTicks;
        counter.ExclusiveTicks += elapsedTicks;
        counter.InclusiveTicks = _oldInclusiveTicks + elapsedTicks;
        counter.HitCount += 1;
    }
}
