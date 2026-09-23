using System.Diagnostics;

namespace Netprof;

/// <summary>
/// Accumulated timing information for a <see cref="Zone"/>.
/// Tick values are measured in <see cref="Stopwatch"/> ticks, not <see cref="Timespan.Ticks"/>.
/// </summary>
public struct Counter
{
    public string? Name { get; internal set; }
    public long InclusiveTicks { get; internal set; }
    public long ExclusiveTicks { get; internal set; }
    public long HitCount { get; internal set; }

    public readonly TimeSpan InclusiveTime => TimeSpan.FromSeconds((double)InclusiveTicks / Stopwatch.Frequency);

    public readonly TimeSpan ExclusiveTime => TimeSpan.FromSeconds((double)ExclusiveTicks / Stopwatch.Frequency);
}
