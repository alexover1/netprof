using Netprof;
using Netprof.Example;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

using var host = builder.Build();

await host.StartAsync();
await host.WaitForShutdownAsync();

Console.WriteLine();
// Profiler.WriteReport(Console.Out);
