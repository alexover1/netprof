using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Netprof;

internal class EventWriter
{
    private const int EventsPerChunk = 1024;

    [InlineArray(EventsPerChunk)]
    private struct EventArray
    {
        private EventRecord _element0;
    }

    [InlineArray(EventsPerChunk)]
    private struct AsyncEventArray
    {
        private AsyncEventRecord _element0;
    }

    private sealed class EventChunk
    {
        internal EventArray Events;
        internal int Count;
        internal EventChunk? Next;
    }

    private sealed class AsyncEventChunk
    {
        internal AsyncEventArray Events;
        internal int Count;
        internal AsyncEventChunk? Next;
    }

    private readonly EventSession _session;
    private EventChunk? _head;
    private EventChunk? _tail;
    private AsyncEventChunk? _asyncHead;
    private AsyncEventChunk? _asyncTail;
    private int _eventCount;
    private int _asyncEventCount;
    private int _currentEventIndex = -1;
    private int _openZoneCount;

    internal EventWriter? Next;

    internal EventWriter(EventSession session, int threadIndex, int managedThreadId, string? threadName)
    {
        _session = session;
        ThreadIndex = threadIndex;
        ManagedThreadId = managedThreadId;
        ThreadName = threadName;
    }

    internal EventSession Session => _session;
    internal int ThreadIndex { get; }
    internal int ManagedThreadId { get; }
    internal string? ThreadName { get; }
    internal int OpenZoneCount => _openZoneCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Zone Enter(string name)
    {
        var chunk = _tail;
        var slot = chunk?.Count ?? EventsPerChunk;
        if ((uint)slot >= EventsPerChunk)
        {
            chunk = Grow();
            slot = 0;
        }

        var eventIndex = _eventCount++;
        ref var record = ref chunk!.Events[slot];
        chunk.Count = slot + 1;
        record.Name = name;
        record.ParentIndex = _currentEventIndex;
        record.EndTimestamp = 0;
        _currentEventIndex = eventIndex;
        _openZoneCount++;
        record.StartTimestamp = Stopwatch.GetTimestamp();
        return new Zone(this, ref record, eventIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool Exit(ref EventRecord record, int eventIndex, long endTimestamp)
    {
#if DEBUG
        Debug.Assert(_currentEventIndex == eventIndex, "Profiling zones must be disposed in stack order.");
#endif
        record.EndTimestamp = endTimestamp;
        _currentEventIndex = record.ParentIndex;
        _openZoneCount--;
        return record.ParentIndex < 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteAsync(AsyncEventRecord record)
    {
        var chunk = _asyncTail;
        var slot = chunk?.Count ?? EventsPerChunk;
        if ((uint)slot >= EventsPerChunk)
        {
            chunk = GrowAsync();
            slot = 0;
        }

        chunk!.Events[slot] = record;
        chunk.Count = slot + 1;
        _asyncEventCount++;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private EventChunk Grow()
    {
        var chunk = new EventChunk();
        if (_tail is null) _head = chunk;
        else _tail.Next = chunk;
        _tail = chunk;
        return chunk;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private AsyncEventChunk GrowAsync()
    {
        var chunk = new AsyncEventChunk();
        if (_asyncTail is null) _asyncHead = chunk;
        else _asyncTail.Next = chunk;
        _asyncTail = chunk;
        return chunk;
    }

    internal ProfileThread Capture()
    {
        if (_openZoneCount != 0)
        {
            throw new InvalidOperationException($"Thread {ManagedThreadId} still has {_openZoneCount} open profiling zones.");
        }

        var events = new ProfileEvent[_eventCount];
        var destination = 0;
        for (var chunk = _head; chunk is not null; chunk = chunk.Next)
        {
            for (var i = 0; i < chunk.Count; i++)
            {
                ref var source = ref chunk.Events[i];
                if (source.EndTimestamp == 0 || source.Name is null)
                {
                    throw new InvalidOperationException("Encountered an unfinished profiling event.");
                }
                events[destination++] = new ProfileEvent(source.Name, source.StartTimestamp, source.EndTimestamp, source.ParentIndex);
            }
        }

        var asyncEvents = new AsyncProfileEvent[_asyncEventCount];
        destination = 0;
        for (var chunk = _asyncHead; chunk is not null; chunk = chunk.Next)
        {
            for (var i = 0; i < chunk.Count; i++)
            {
                var source = chunk.Events[i];
                asyncEvents[destination++] = new AsyncProfileEvent(source.Name, source.SpanId, source.ParentSpanId,
                    source.StartTimestamp, source.EndTimestamp, source.StartThreadIndex, source.EndThreadIndex);
            }
        }

        return new ProfileThread(ThreadIndex, ManagedThreadId, ThreadName, events, asyncEvents);
    }
}
