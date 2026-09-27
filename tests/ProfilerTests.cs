using Netprof;

namespace Netprof.Tests;

[TestClass]
public sealed class ProfilerTests
{
    [TestMethod]
    public void TestProfileSynchronousZone()
    {
        using var session = Profiler.StartRecording();
        using (Profiler.EnterZone())
        {
            Thread.SpinWait(100_000);
        }
        session.Stop();

        var recording = session.Capture();
        Assert.AreEqual(1, recording.Threads.Count);
        Assert.AreEqual(1, recording.Threads[0].Events.Count);

        var profileEvent = recording.Threads[0].Events[0];
        Assert.AreEqual(-1, profileEvent.ParentIndex);
        Assert.AreEqual("Netprof.Tests.ProfilerTests.TestProfileSynchronousZone()", profileEvent.Name);
    }

    [TestMethod]
    public void TestProfileAsyncZone()
    {
        using var session = Profiler.StartRecording();
        var task = Task.Run(async () =>
        {
            using (Profiler.EnterAsyncZone("Netprof.Tests.ProfilerTests.TestProfileAsyncZone()")) // TODO(alex): Intercept calls to EnterAsyncZone as well!
            {
                await Task.Delay(1);
            }
        });
        task.Wait();
        session.Stop();

        var recording = session.Capture();
        Assert.AreEqual(1, recording.Threads.Count);
        Assert.AreEqual(1, recording.Threads[0].AsyncEvents.Count);

        var profileEvent = recording.Threads[0].AsyncEvents[0];
        Assert.AreEqual(0, profileEvent.ParentSpanId);
        Assert.AreEqual("Netprof.Tests.ProfilerTests.TestProfileAsyncZone()", profileEvent.Name);
    }
}
