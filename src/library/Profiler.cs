using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Netprof;

/// <summary>
///
/// </summary>
public class Profiler
{
    private static int _generatedZoneCount; // NOTE(alex): Populated when the Netprof.Generators assembly is included.

    [ThreadStatic]
    private static int _parentIndex;

    internal Counter[] Counters { get; }

    internal static int ParentIndex
    {
        get => _parentIndex;
        set => _parentIndex = value;
    }

    public Profiler()
        : this(_generatedZoneCount)
    {
    }

    public Profiler(int zoneCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(zoneCount); // NOTE(alex): Zero is allowed.
        Counters = new Counter[zoneCount + 1];
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterGeneratedZoneCount(int zoneCount)
    {
        _generatedZoneCount = zoneCount;
    }

    public int ZoneCount => Counters.Length;

    /// <summary>
    /// Returns a copy of a counter at the specified index.
    /// This method can return a partially written struct if the specified zone is in use when this method is called.
    /// </summary>
    public Counter GetCounter(int index)
    {
        return Counters[index + 1];
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
        throw new InvalidOperationException("Method Profiler.EnterZone() was not intercepted. Reference Netprof.Generators as an analyzer and enable the Netprof interceptor namespace.");
    }

    /// <summary>
    /// Enters the zone represented by <paramref name="index"/>.
    /// While this method can be manually called, it is generally prefer to use <see cref="EnterZone()"/> instead.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Zone EnterZone(int index)
    {
        return new Zone(this, index + 1);
    }
}
