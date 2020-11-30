using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pinger
{
    public interface IPingProvider<T>
    {
        Task<T> SendPingAsync(string host, CancellationToken cancellationToken);
    }

    public interface IPingStatusProvider<T>
    {
        bool IsPingSuccesful(T pingResult);
    }

    public abstract class Pinger<T> : IInitializable
    {
        private readonly TimeSpan PingInterval = TimeSpan.FromSeconds(1);

        private readonly IPingProvider<T> _pingProvider;
        private readonly IPingStatusProvider<T> _pingStatusProvider;
        private readonly IEqualityComparer<T> _equalityComparer;
        private readonly ITrackActivePing _io;
        private readonly string _host;

        private Task pingTask;
        private CancellationTokenSource _cts;

        public DateTime? LastSuccessfulPing { get; private set; }
        public bool? PingSucceeding { get; private set; }
        public event EventHandler< EventArgs<bool> > PingResultChanged;

        public Pinger(
            IPingProvider<T> pingProvider, 
            IPingStatusProvider<T> pingStatusProvider, 
            IEqualityComparer<T> equalityComparer,
            ITrackActivePing io, 
            string host)
        {
            _pingProvider = pingProvider ?? throw new ArgumentNullException(nameof(pingProvider));
            _pingStatusProvider = pingStatusProvider ?? throw new ArgumentNullException(nameof(pingStatusProvider));
            _equalityComparer = equalityComparer ?? throw new ArgumentNullException(nameof(equalityComparer));
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public Task InitAsync()
        {
            this._cts = new CancellationTokenSource();

            pingTask = Task.Run(async () => await PingUntilCancelled(_cts.Token))
                .ContinueWith(
                    t => t.Exception?
                          .Flatten()
                          .InnerExceptions
                          .ToList()
                          .ForEach(e => Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace)), 
                TaskContinuationOptions.OnlyOnFaulted);

            return Task.CompletedTask;                              
        }

        public async Task DeinitAsync()
        {
            _cts.Cancel();
            await _io.SetActiveAsync(false, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
        }

        private async Task PingUntilCancelled(CancellationToken cancellationToken)
        {
            T lastPingReply = default(T);

            while (!cancellationToken.IsCancellationRequested)
            {
                var currentPingReply = await _pingProvider.SendPingAsync(_host, cancellationToken);

                if (currentPingReply == null)
                {
                    Console.Out.WriteLine($"Ping to {_host} returned null");
                    await Task.Delay(PingInterval);
                    continue;
                }

                UpdateStatus(_pingStatusProvider.IsPingSuccesful(currentPingReply));

                if (!(_equalityComparer.Equals(lastPingReply, currentPingReply)))
                {
                    OnPingStatusChanged(_pingStatusProvider.IsPingSuccesful(currentPingReply));
                    await _io.SetActiveAsync(_pingStatusProvider.IsPingSuccesful(currentPingReply), cancellationToken);
                }

                lastPingReply = currentPingReply;

                await Task.Delay(PingInterval);
            }
        }

        internal void UpdateStatus(bool currentPintResult)
        {
            this.LastSuccessfulPing = DateTime.UtcNow;
            this.PingSucceeding = currentPintResult;
        }

        internal void OnPingStatusChanged(bool newValue)
        {
            var handler = this.PingResultChanged;
            if (handler != null)
            {
                handler.Invoke(this, new EventArgs<bool>(newValue));
            }
        }
    }
}
