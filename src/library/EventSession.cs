using System.Diagnostics;

namespace Netprof;

/// <summary>
/// A session corresponds to one profiling recording and the per-thread event streams collected during it.
/// </summary>
public sealed class EventSession : IDisposable
{
    private int _syncRootState;
    private int _asyncSpanState;
    private long _nextSpanId;
    private int _nextThreadIndex;
    private int _stopped;
    private EventWriter? _writers;

    internal EventSession(long startTimestamp) => StartTimestamp = startTimestamp;

    public long StartTimestamp { get; }
    public long StopTimestamp { get; private set; }
    public bool IsStopped => Volatile.Read(ref _stopped) != 0;

    internal bool TryEnterSyncRoot() => TryEnter(ref _syncRootState);

    internal bool TryEnterAsyncSpan() => TryEnter(ref _asyncSpanState);

    internal void ExitSyncRoot() => Interlocked.Decrement(ref _syncRootState);

    internal void ExitAsyncSpan() => Interlocked.Decrement(ref _asyncSpanState);

    internal long NextSpanId() => Interlocked.Increment(ref _nextSpanId);

    internal bool IsQuiescent =>
        (Volatile.Read(ref _syncRootState) & int.MaxValue) == 0
        && (Volatile.Read(ref _asyncSpanState) & int.MaxValue) == 0;

    internal EventWriter CreateWriterForCurrentThread()
    {
        var writer = new EventWriter(
            this,
            Interlocked.Increment(ref _nextThreadIndex) - 1,
            Environment.CurrentManagedThreadId,
            Thread.CurrentThread.Name
        );

        // NOTE(alex): Writer registration is rare, so a simple lock-free intrusive stack is sufficient.
        EventWriter? head;
        do
        {
            head = Volatile.Read(ref _writers);
            writer.Next = head;
        } while (Interlocked.CompareExchange(ref _writers, writer, head) != head);
        return writer;
    }

    internal void MarkStopped(long stopTimestamp)
    {
        StopTimestamp = stopTimestamp;
        Interlocked.Or(ref _syncRootState, int.MinValue);
        Interlocked.Or(ref _asyncSpanState, int.MinValue);
        Volatile.Write(ref _stopped, 1);
    }

    /// <summary>
    /// Stops accepting new profiling events.
    ///
    /// If there are any open events, they are allowed to complete.
    /// This method is safe to call multiple times on the same <see cref="EventSession"/> instance.
    /// </summary>
    public void Stop() => Profiler.StopRecording(this);

    /// <summary>
    /// Copies the completed event streams into an immutable snapshot for reporting.
    /// </summary>
    public ProfileRecording Capture()
    {
        if (!IsStopped)
        {
            throw new InvalidOperationException("Stop the recording before capturing it.");
        }

        if (!IsQuiescent)
        {
            throw new InvalidOperationException(
                "Profiling scopes are still open. Capture after they have completed."
            );
        }

        var writers = new List<EventWriter>();
        for (var writer = Volatile.Read(ref _writers); writer is not null; writer = writer.Next)
        {
            writers.Add(writer);
        }

        writers.Sort(static (a, b) => a.ThreadIndex.CompareTo(b.ThreadIndex));

        var threads = new ProfileThread[writers.Count];
        var endTimestamp = StopTimestamp;
        for (var i = 0; i < writers.Count; i++)
        {
            var thread = writers[i].Capture();
            threads[i] = thread;

            foreach (var e in thread.Events)
            {
                endTimestamp = Math.Max(endTimestamp, e.EndTimestamp);
            }

            foreach (var e in thread.AsyncEvents)
            {
                endTimestamp = Math.Max(endTimestamp, e.EndTimestamp);
            }
        }

        return new ProfileRecording(Stopwatch.Frequency, StartTimestamp, endTimestamp, threads);
    }

    /// <summary>
    /// Stops the session if it is still active.
    /// </summary>
    public void Dispose() => Stop();

    private static bool TryEnter(ref int state)
    {
        while (true)
        {
            var observed = Volatile.Read(ref state);
            if (observed < 0)
            {
                return false;
            }

            if (observed == int.MaxValue)
            {
                throw new InvalidOperationException("Too many active profiler scopes.");
            }

            if (Interlocked.CompareExchange(ref state, observed + 1, observed) == observed)
            {
                return true;
            }
        }
    }
}
