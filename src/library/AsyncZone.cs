using System.Diagnostics;

namespace Netprof;

/// <summary>
/// A profiling scope that may cross <c>await</c> boundaries and physical threads.
/// </summary>
public readonly struct AsyncZone : IDisposable
{
    private readonly AsyncSpanState? _state;

    internal AsyncZone(AsyncSpanState state) => _state = state;

    public void Dispose()
    {
        var state = _state;
        if (state is not null && Interlocked.Exchange(ref state.Disposed, 1) == 0)
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            Profiler.ExitAsyncZone(state, endTimestamp);
        }
    }
}

internal sealed class AsyncSpanContext
{
    internal AsyncSpanContext(EventSession session, long spanId)
    {
        Session = session;
        SpanId = spanId;
    }

    internal EventSession Session { get; }
    internal long SpanId { get; }
}

internal sealed class AsyncSpanState
{
    internal AsyncSpanState(
        EventSession session,
        string name,
        AsyncSpanContext? previousContext,
        AsyncSpanContext context,
        long parentSpanId,
        long startTimestamp,
        int startThreadIndex
    )
    {
        Session = session;
        Name = name;
        PreviousContext = previousContext;
        Context = context;
        ParentSpanId = parentSpanId;
        StartTimestamp = startTimestamp;
        StartThreadIndex = startThreadIndex;
    }

    internal readonly EventSession Session;
    internal readonly string Name;
    internal readonly AsyncSpanContext? PreviousContext;
    internal readonly AsyncSpanContext Context;
    internal readonly long ParentSpanId;
    internal readonly long StartTimestamp;
    internal readonly int StartThreadIndex;
    internal int Disposed;
}
