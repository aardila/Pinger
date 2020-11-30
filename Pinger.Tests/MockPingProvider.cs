using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pinger.Tests
{
    class MockPingProvider : IPingProvider<bool?>
    {
        private readonly Func<bool?> pingSucceeds;

        public MockPingProvider(Func<bool?> pingSucceeds)
        {
            this.pingSucceeds = pingSucceeds;
        }
        public Task<bool?> SendPingAsync(string host, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.pingSucceeds());
        }
    }

    class MockPingStatusProvider : IPingStatusProvider<bool?>
    {
        public bool IsPingSuccesful(bool? pingResult) => pingResult.HasValue && pingResult.Value;
    }

    class NullableBoolEqualityComparer : IEqualityComparer<bool?>
    {
        public bool Equals(bool? x, bool? y)
        {
            return x == y;
        }

        public int GetHashCode(bool? obj)
        {
            return obj == null ? 0 : obj.Value ? 1 : 2;
        }
    }
}
