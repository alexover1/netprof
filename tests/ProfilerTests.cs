using Netprof;

namespace Netprof.Tests;

[TestClass]
public sealed class ProfilerTests
{
    [TestMethod]
    public void TestEnterZoneWithManualIndexing()
    {
        var profiler = new Profiler(1); // NOTE(alex): Create a new profiler with the specified number of zones.

        using (profiler.EnterZone(0))
        {
            Thread.SpinWait(100_000);
        }

        var counter = profiler.GetCounter(0);
        Assert.AreEqual(1L, counter.HitCount);
        Assert.IsTrue(counter.InclusiveTicks > 0);
        Assert.AreEqual(counter.InclusiveTicks, counter.ExclusiveTicks);
    }

    [TestMethod]
    public void TestNestedZonesTrackExclusiveTime()
    {
        var profiler = new Profiler(2);

        using (profiler.EnterZone(0))
        {
            Thread.SpinWait(100_000); // NOTE(alex): Sleep a little before...

            using (profiler.EnterZone(1))
            {
                Thread.SpinWait(100_000); // NOTE(alex): Time in a nested zone shouldn't be included in the parent's exclusive time.
            }

            Thread.SpinWait(100_000); // NOTE(alex): Sleep a little after...
        }

        var counters = profiler.GetCounters().Where(static c => c.HitCount != 0).ToArray();
        Assert.AreEqual(2, counters.Length);
        Assert.IsTrue(counters.All(static c => c.HitCount == 1));
        Assert.IsTrue(counters.All(static c => c.InclusiveTicks > 0));
        Assert.IsTrue(counters.Any(static c => c.InclusiveTicks > c.ExclusiveTicks)); // NOTE(alex): One zone (the outer zone) should have time that is spent in the child zone.
        Assert.IsTrue(counters.All(static c => c.InclusiveTicks >= c.ExclusiveTicks)); // NOTE(alex): But all zones (even leaf zones) track their own time, and the inclusive time is always greater or equal, but never less, than the inclusive time.
    }

    [TestMethod]
    public void TestInvokeEnterZoneWithNoArgumentsThrows()
    {
        var profiler = new Profiler(1);
        Func<Zone> enterZone = profiler.EnterZone;
        Assert.Throws<InvalidOperationException>(() => enterZone());
    }

    [TestMethod]
    public void TestRecursiveMethodCanBeProfiled()
    {
        var profiler = new Profiler(1);

        int factorial = Factorial(profiler, 5);
        Assert.AreEqual(120, factorial); // NOTE(alex): Math should work.

        var counter = profiler.GetCounter(0);
        Assert.AreEqual(5, counter.HitCount); // NOTE(alex): HitCount tells you how many times the method recursed!
        Assert.IsTrue(counter.InclusiveTicks > 0);
        Assert.IsTrue(counter.ExclusiveTicks > 0);

        return;

        int Factorial(Profiler profiler, int value)
        {
            using var zone = profiler.EnterZone(0);

            if (value <= 1)
            {
                return 1;
            }

            return value * Factorial(profiler, value - 1);
        }
    }

    [TestMethod]
    public void TestAutomaticZones()
    {
        var profiler = new Profiler(); // NOTE(alex): Notice how we don't have to provide the number of zones.

        using (profiler.EnterZone()) // NOTE(alex): Notice how we don't have to provide the zone index.
        {
            Thread.SpinWait(100_000);
        }

        var counter = profiler.GetCounter(0);
        Assert.AreEqual(1L, counter.HitCount);
        Assert.IsTrue(counter.InclusiveTicks > 0);
        Assert.AreEqual(counter.InclusiveTicks, counter.ExclusiveTicks);
    }
}
