using System.Runtime.CompilerServices;

namespace Netprof;

/// <summary>
///
/// </summary>
public class Profiler
{
    [ThreadStatic]
    private static int _currentCounterIndex;

    internal Counter[] Counters { get; }

    internal static int CurrentCounterIndex
    {
        get => _currentCounterIndex;
        set => _currentCounterIndex = value;
    }

    public Profiler(int zoneCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(zoneCount); // NOTE(alex): Zero is allowed.
        Counters = new Counter[zoneCount];
    }

    public int ZoneCount => Counters.Length;

    /// <summary>
    /// Returns a copy of a counter at the specified index.
    /// This method can return a partially written struct if the specified zone is in use when this method is called.
    /// </summary>
    public Counter GetCounter(int index)
    {
        return Counters[index];
    }

    /// <summary>
    /// Returns copies of all counters.
    /// </summary>
    public Counter[] GetCounters()
    {
        return (Counter[])Counters.Clone();
    }

    /// <summary>
    /// Enters the zone based on the source location this method is called from.
    /// A unique <see cref="Counter"/> object will be generated for each invocation of this method,
    /// and its index will be substituted automatically at compile-time by the source generator.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public Zone EnterZone()
    {
        throw new InvalidOperationException("Method Profiler.EnterZone() was not intercepted. Reference Netprof.Generators as an analyzer and enable the Netprof.Generated interceptor namespace.");
    }

    /// <summary>
    /// Enters the zone represented by <paramref name="counterIndex"/>.
    /// While this method can be manually called, it is generally prefer to use <see cref="EnterZone()"/> instead.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Zone EnterZone(int counterIndex)
    {
        return new Zone(this, counterIndex);
    }
}
