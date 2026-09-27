using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Netprof;

/// <summary>
/// Records low-overhead synchronous and logical async profiling events.
/// </summary>
public static class Profiler
{
    private static readonly AsyncLocal<AsyncSpanContext?> _asyncContext = new();
    private static EventSession? _currentSession;

    [ThreadStatic]
    private static EventSession? _cachedSession;

    [ThreadStatic]
    private static EventWriter? _cachedWriter;

    [ThreadStatic]
    private static EventSession? _activeSyncSession;

    [ThreadStatic]
    private static EventWriter? _activeSyncWriter;

    /// <summary>
    /// Gets whether a recording session is currently accepting new events.
    /// </summary>
    public static bool IsRecording => Volatile.Read(ref _currentSession) is not null;

    /// <summary>
    /// Starts a new recording. Only one global recording can be active at a time.
    /// </summary>
    public static EventSession StartRecording()
    {
        var session = new EventSession(Stopwatch.GetTimestamp());
        if (Interlocked.CompareExchange(ref _currentSession, session, null) is not null)
        {
            throw new InvalidOperationException("A profiling session is already active.");
        }
        return session;
    }

    /// <summary>
    /// Enters a synchronous profiling zone.
    ///
    /// The name of the zone will be replaced with the name of the method <see cref="EnterZone"/> was called from.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Zone EnterZone()
    {
        throw new InvalidOperationException("The call to this method was not intercepted. Reference the Netprof.Generators package and enable Netprof using InterceptorsNamespaces in your project file.");
    }

    /// <summary>
    /// Enters a synchronous profiling zone with the name <paramref name="name"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Zone EnterZone(string name)
    {
        var writer = _activeSyncWriter;
        if (writer is not null)
        {
            return writer.Enter(name);
        }

        var session = Volatile.Read(ref _currentSession);
        if (session is null || !session.TryEnterSyncRoot())
        {
            return default;
        }

        writer = GetOrCreateWriter(session);
        _activeSyncSession = session;
        _activeSyncWriter = writer;

        return writer.Enter(name);
    }

    /// <summary>
    /// Enters a logical profiling zone that can survive <c>await</c>.
    /// </summary>
    public static AsyncZone EnterAsyncZone(string name)
    {
        var session = Volatile.Read(ref _currentSession);
        if (session is null || !session.TryEnterAsyncSpan())
        {
            return default;
        }

        var previous = _asyncContext.Value;
        var spanId = session.NextSpanId();
        var parentSpanId = previous is not null && ReferenceEquals(previous.Session, session) ? previous.SpanId : 0;
        var context = new AsyncSpanContext(session, spanId);
        var writer = GetOrCreateWriter(session);
        var state = new AsyncSpanState(session, name, previous, context, parentSpanId, Stopwatch.GetTimestamp(), writer.ThreadIndex);
        _asyncContext.Value = context;

        return new AsyncZone(state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ExitZone(EventWriter writer, ref EventRecord record, int eventIndex, long endTimestamp)
    {
        if (writer.Exit(ref record, eventIndex, endTimestamp))
        {
            var session = _activeSyncSession;
            _activeSyncWriter = null;
            _activeSyncSession = null;
            session?.ExitSyncRoot();
        }
    }

    internal static void ExitAsyncZone(AsyncSpanState state, long endTimestamp)
    {
        _asyncContext.Value = state.PreviousContext;
        var writer = GetOrCreateWriter(state.Session);
        writer.WriteAsync(new AsyncEventRecord(state.Name, state.Context.SpanId, state.ParentSpanId, state.StartTimestamp,
                                               endTimestamp, state.StartThreadIndex, writer.ThreadIndex));
        state.Session.ExitAsyncSpan();
    }

    internal static void StopRecording(EventSession session)
    {
        if (session.IsStopped) return;

        var current = Interlocked.CompareExchange(ref _currentSession, null, session);
        if (ReferenceEquals(current, session))
        {
            session.MarkStopped(Stopwatch.GetTimestamp());
            return;
        }

        if (!session.IsStopped)
        {
            throw new InvalidOperationException("The supplied session is not the active profiling session.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EventWriter GetOrCreateWriter(EventSession session)
    {
        if (ReferenceEquals(_cachedSession, session) && _cachedWriter is not null)
        {
            return _cachedWriter;
        }

        return BindCurrentThread(session);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static EventWriter BindCurrentThread(EventSession session)
    {
        var writer = session.CreateWriterForCurrentThread();
        _cachedSession = session;
        _cachedWriter = writer;
        return writer;
    }
}
