using System.Threading.Tasks;
using Etg.Yams.Update;

namespace Wivuu.Yams.Local
{
    public class LocalUpdateSessionManager : IUpdateSessionManager
    {
        public Task EndUpdateSession(string applicationId) => 
            Task.FromResult(0);

        public Task<bool> TryStartUpdateSession(string applicationId) => 
            Task.FromResult(true);
    }
}
