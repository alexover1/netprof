using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Netprof;

/// <summary>
/// A synchronous profiling scope.
///
/// The zone must be disposed on the same stack frame that it was called from,
/// otherwise profiling information will not be accurate.
/// </summary>
public ref struct Zone
{
    private EventWriter? _writer;
    private ref EventRecord _event;
    private readonly int _eventIndex;

    internal Zone(EventWriter writer, ref EventRecord profileEvent, int eventIndex)
    {
        _writer = writer;
        _event = ref profileEvent;
        _eventIndex = eventIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_writer is not null)
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            Profiler.ExitZone(_writer, ref _event, _eventIndex, endTimestamp);
            _writer = null;
        }
    }
}
