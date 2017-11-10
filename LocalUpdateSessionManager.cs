using System.Threading.Tasks;
using Etg.Yams.Update;

namespace Wivuu.Yams.Local
{
    public class LocalUpdateSessionManager : IUpdateSessionManager
    {
        public Task EndUpdateSession(string applicationId) =>
            Task.CompletedTask;

        public Task<bool> TryStartUpdateSession(string applicationId) => 
            Task.FromResult(true);
    }
}
