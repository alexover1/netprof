using Netprof;

namespace Netprof.Tests;

[TestClass]
public sealed class ProfilerTests
{
    [TestMethod]
    public void TestProfileNamedSynchronousZone()
    {
        using var session = Profiler.StartRecording();
        using (Profiler.EnterZone("SpinWait"))
        {
            Thread.SpinWait(100_000);
        }
        session.Stop();

        var recording = session.Capture();
        Assert.AreEqual(1, recording.Threads.Count);
        Assert.AreEqual(1, recording.Threads[0].Events.Count);

        var profileEvent = recording.Threads[0].Events[0];
        Assert.AreEqual(-1, profileEvent.ParentIndex);
        Assert.AreEqual("SpinWait", profileEvent.Name);
    }

    [TestMethod]
    public void TestProfileGeneratedSynchronousZone()
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
        Assert.AreEqual("Netprof.Tests.ProfilerTests.TestProfileGeneratedSynchronousZone()", profileEvent.Name);
    }

    [TestMethod]
    public void TestProfileNamedAsyncZone()
    {
        using var session = Profiler.StartRecording();
        var task = Task.Run(async () =>
        {
            using (Profiler.EnterAsyncZone("Netprof.Tests.ProfilerTests.TestProfileNamedAsyncZone()"))
            {
                await Task.Delay(1);
            }
        });
        task.Wait();
        session.Stop();

        var recording = session.Capture();
        var events = recording.Threads.SelectMany(thread => thread.AsyncEvents);
        Assert.IsTrue(events.Any(e => e.Name == "Netprof.Tests.ProfilerTests.TestProfileNamedAsyncZone()"));

        var profileEvent = events.First(e => e.Name == "Netprof.Tests.ProfilerTests.TestProfileNamedAsyncZone()");
        Assert.AreEqual(0, profileEvent.ParentSpanId);
    }

    [TestMethod]
    public void TestProfileGeneratedAsyncZone()
    {
        using var session = Profiler.StartRecording();

        var task = Task.Run(async () =>
        {
            using (Profiler.EnterAsyncZone())
            {
                await Task.Delay(1);
            }
        });
        task.Wait();
        session.Stop();

        var recording = session.Capture();
        var events = recording.Threads.SelectMany(thread => thread.AsyncEvents);
        Assert.IsTrue(events.Any(e => e.Name == "Netprof.Tests.ProfilerTests.TestProfileGeneratedAsyncZone()"));

        var profileEvent = events.First(e => e.Name == "Netprof.Tests.ProfilerTests.TestProfileGeneratedAsyncZone()");
        Assert.AreEqual(0, profileEvent.ParentSpanId);
    }
}
