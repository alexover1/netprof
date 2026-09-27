namespace Netprof.Example;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var session = Profiler.StartRecording();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = await ReceiveBatchAsync(stoppingToken);
                ProcessBatch(batch);
            }
        }
        finally
        {
            session.Stop();
            var recording = session.Capture();
            foreach (var ev in recording.Threads[0].Events)
            {
                Console.WriteLine(ev.Name);
            }
        }
    }

    private void ProcessBatch(TemperatureReading[] readings)
    {
        using var zone = Profiler.EnterZone();

        var average = CalculateAverageTemperature(readings);

        UploadResults(average);
    }

    private double CalculateAverageTemperature(TemperatureReading[] readings)
    {
        using var zone = Profiler.EnterZone();

        var total = 0.0;

        foreach (var reading in readings)
        {
            total += reading.Temperature / readings.Length;
        }

        return total;
    }

    private void UploadResults(double averageTemperature)
    {
        using var zone = Profiler.EnterZone();

        logger.LogInformation("Average temperature: {Temperature:F2}", averageTemperature);
    }

    private static async Task<TemperatureReading[]> ReceiveBatchAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(250, cancellationToken);

        return Enumerable
            .Range(0, 100)
            .Select(i => new TemperatureReading(i, 20 + Random.Shared.NextDouble() * 15))
            .ToArray();
    }
}
