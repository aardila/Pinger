using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nito.AsyncEx;

namespace Pinger.Tests
{
    [TestClass]
    public class PingerTests
    {
        [TestMethod]
        [Timeout(30 * 1000)]
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task TestPingSucceedingProp(bool pingSucceeds)
        {
            //Arrange
            AsyncAutoResetEvent autoResetEvent = new AsyncAutoResetEvent();
            var sut = new MockPinger(() => pingSucceeds, A.Fake<ITrackActivePing>());
            sut.PingResultChanged += (s, e) => autoResetEvent.Set();

            //Act
            await sut.InitAsync();
            await autoResetEvent.WaitAsync();

            //Assert
            Assert.IsTrue(sut.PingSucceeding.HasValue && sut.PingSucceeding.Value == pingSucceeds);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task TestStatusGetsSetOnDependecy(bool active)
        {
            AsyncAutoResetEvent autoResetEvent = new AsyncAutoResetEvent();
            var dependencyFake = A.Fake<ITrackActivePing>();
            var sut = new MockPinger(() => active, dependencyFake);
            sut.PingResultChanged += (s, e) => autoResetEvent.Set();

            await sut.InitAsync();

            A.CallTo(() => dependencyFake.SetActiveAsync(active, A<CancellationToken>.Ignored))
                .MustHaveHappened();
        }
    }
}
