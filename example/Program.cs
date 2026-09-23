using Netprof;
using Netprof.Example;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSingleton<Profiler>()
    .AddHostedService<Worker>();

using var host = builder.Build();

var profiler = host.Services.GetRequiredService<Profiler>();

await host.StartAsync();
await host.WaitForShutdownAsync();

Console.WriteLine();
profiler.WriteReport(Console.Out);
