using System.Text;

namespace Netprof;

public static class ProfileExportExtensions
{
    /// <summary>
    /// Writes a compact, text-based report to the specified text writer.
    /// </summary>
    public static void WriteReport(this ProfileRecording recording, TextWriter writer)
    {
        foreach (var thread in recording.Threads)
        {
            writer.WriteLine($"Thread {thread.ThreadIndex}{FormatThreadName(thread.Name)}{FormatAsyncCompletions(thread.AsyncEvents.Count)}");

            foreach (var e in thread.Events)
            {
                var depth = GetDepth(thread.Events, e.ParentIndex);
                var ms = TicksToMilliseconds(e.DurationTicks, recording.TimestampFrequency);
                writer.Write(new string(' ', depth));
                writer.WriteLine($"{e.Name}  {ms:F3} ms");
            }
        }
    }

    private static int GetDepth(IReadOnlyList<ProfileEvent> events, int parentIndex)
    {
        var depth = 0;
        while (parentIndex >= 0)
        {
            depth += 1;
            parentIndex = events[parentIndex].ParentIndex;
        }
        return depth;
    }

    private static double TicksToMilliseconds(long ticks, long frequency) => ticks * 1000.0 / frequency;
    private static string FormatThreadName(string? name) => string.IsNullOrEmpty(name) ? "" : $" \"{name}\"";
    private static string FormatAsyncCompletions(int count) => count == 0 ? "" : $", {count} async completions";
}
