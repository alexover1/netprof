using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Netprof;

/// <summary>
/// A synchronous profiling scope.
///
/// The zone must be disposed on the same stack frame that it was called from,
/// otherwise profiling information will not be accurate. It is not recommended
/// to create a zone in a method that is <c>async</c> or that returns a <see cref="Task"/>,
/// as the duration of the zone will not correspond to the duration of the task.
/// Instead, you should use <see cref="AsyncZone"/> for this purpose, because it
/// tracks the total elapsed time of an async task.
/// </summary>
public ref struct Zone
{
    private EventWriter? _writer;
    private ref Event _activeEvent;
    private readonly int _eventIndex;

    internal Zone(EventWriter writer, ref Event activeEvent, int eventIndex)
    {
        _writer = writer;
        _activeEvent = ref activeEvent;
        _eventIndex = eventIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_writer is not null)
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            Profiler.ExitZone(_writer, ref _activeEvent, _eventIndex, endTimestamp);
            _writer = null;
        }
    }
}
