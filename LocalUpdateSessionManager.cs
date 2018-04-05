using System.Threading.Tasks;
using Etg.Yams.Update;

namespace Wivuu.Yams.Local
{
    public class LocalUpdateSessionManager : IUpdateSessionManager
    {
        public Task EndUpdateSession() => Task.CompletedTask;

        public Task<bool> TryStartUpdateSession() => Task.FromResult(true);
    }
}
