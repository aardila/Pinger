using System.Threading;
using System.Threading.Tasks;

namespace Pinger
{
    public interface ITrackActivePing
    {
        Task SetActiveAsync(bool value, CancellationToken cancellationToken);
    }
}
