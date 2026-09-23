using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Netprof;

/// <summary>
/// Collects timing information for profiling zones.
/// </summary>
public class Profiler
{
    private static string[] _generatedZoneNames = []; // NOTE(alex): Populated when the Netprof.Generators assembly is included.

    [ThreadStatic]
    private static int _currentCounterIndex;

    private readonly Counter[] _counters;

    internal static int CurrentCounterIndex
    {
        get => _currentCounterIndex;
        set => _currentCounterIndex = value;
    }

    internal ref Counter this[int internalIndex]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _counters[internalIndex];
    }

    /// <summary>
    /// Creates a profiler containing the zones discovered by the source generator.
    /// </summary>
    public Profiler()
        : this(_generatedZoneNames.Length)
    {
        for (var i = 0; i < _generatedZoneNames.Length; i += 1)
        {
            _counters[i + 1].Name = _generatedZoneNames[i];
        }
    }

    /// <summary>
    /// Creates a profiler with the specified number of manually indexed zones.
    /// </summary>
    public Profiler(int zoneCount)
    {
        _counters = new Counter[zoneCount + 1];
    }

    /// <summary>
    /// Gets the number of user-visible profiling zones.
    /// </summary>
    public int ZoneCount => _counters.Length - 1;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void RegisterGeneratedZones(string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);

        // NOTE(alex): Don't retain an externally mutable array (just in case).
        _generatedZoneNames = (string[])names.Clone();
    }

    /// <summary>
    /// Returns a copy of all counters.
    /// The returned value may contain partially updated data if any zones are
    /// being updated concurrently.
    /// </summary>
    public Counter[] GetCounters()
    {
        var result = new Counter[ZoneCount];
        Array.Copy(_counters, 1, result, 0, result.Length);
        return result;
    }

    /// <summary>
    /// Returns a copy of a counter at the specified zone index.
    /// The returned value may contain partially updated data if the zone is
    /// being updated concurrently.
    /// </summary>
    public Counter GetCounter(int index)
    {
        return _counters[index + 1];
    }

    /// <summary>
    /// Replaces the name of the profiling zone represented by <paramref name="index"/> with <paramref name="name"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetZoneName(int index, string? name)
    {
        _counters[index + 1].Name = name;
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
    /// Enters the profiling zone represented by <paramref name="index"/>.
    /// </summary>
    /// <remarks>
    /// Normally <see cref="EnterZone()"/> should be used so that the source generator assigns the index automatically.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Zone EnterZone(int index)
    {
        return new Zone(this, index + 1);
    }
}
