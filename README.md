# Netprof

.NET Instrumentation Library

## Quick Start

```csharp
var profiler = new Profiler();

using (profiler.EnterZone())
{
    Thread.SpinWait(10_000);
}

profiler.WriteReport(Console.Out);
```
