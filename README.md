# Netprof

.NET Instrumentation Library

## Quick Start

```csharp
using Netprof;

using var session = Profiler.StartRecording();

using (profiler.EnterZone())
{
    Thread.SpinWait(10_000);
}

session.Stop();

var recording = session.Capture();
profile.WriteReport(Console.Out);
```

## Nested Zones

```csharp
using (Profiler.EnterZone("Outer Zone"))
{
    ...
    using (Profiler.EnterZone("Inner Zone"))
    {
        ...
    }
    ...
}
```

Netprof records the parent relationship and timing of every zone, allowing inclusive and exclusive time to be calculated after recording.

## Recording

```csharp
using var session = Profiler.StartRecording();

RunSingleIteration();

session.Stop();
var profile = recording.Capture();
```

Recording uses an event-based system. Each thread maintains a separate event stream, which avoids synchronization in the hot path.

## Reports

```
var profile = recording.Capture();
profile.WriteReport(Console.Out);
```

Reports include call counts, inclusive time, exclusive time, and per-call timing.

Because the original events are retained, the same recording can also be used for flame graphs, timelines, latency distributions, and comparisons between runs.

If you would like to write a report in a custom format, we recommend using [Extension Methods](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods).
