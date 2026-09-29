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
            recording.WriteReport(Console.Out);
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

        return SumTemperatures(readings, 0, readings.Length) / readings.Length;
    }

    // NOTE(alex): Please don't actually write it this way. This is purely to demonstrate recursion.
    private double SumTemperatures(TemperatureReading[] readings, int start, int count)
    {
        using var zone = Profiler.EnterZone();

        if (count <= 16)
        {
            var total = 0.0;

            foreach (var reading in readings)
            {
                total += reading.Temperature / readings.Length;
            }

            return total;
        }

        var leftCount = count / 2;
        var rightCount = count - leftCount;

        return SumTemperatures(readings, start, leftCount) + SumTemperatures(readings, start + leftCount, rightCount);
    }

    private void UploadResults(double averageTemperature)
    {
        using var zone = Profiler.EnterZone();

        logger.LogInformation("Average temperature: {Temperature:F2}", averageTemperature);
    }

    private static async Task<TemperatureReading[]> ReceiveBatchAsync(CancellationToken cancellationToken)
    {
        using var zone = Profiler.EnterAsyncZone();

        await Task.Delay(250, cancellationToken);

        return Enumerable
            .Range(0, 100)
            .Select(i => new TemperatureReading(i, 20 + Random.Shared.NextDouble() * 15))
            .ToArray();
    }
}
