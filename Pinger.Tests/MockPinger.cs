using System;
using System.Collections.Generic;

namespace Pinger.Tests
{
    internal sealed class MockPinger : Pinger<bool?>
    {
        public MockPinger(Func<bool?> pingSucceeds, ITrackActivePing io)
            : base(new MockPingProvider(pingSucceeds),
                   new MockPingStatusProvider(),
                   EqualityComparer<bool?>.Default,
                   io,
                   "localhost")
        {
            
        }
    }
}
