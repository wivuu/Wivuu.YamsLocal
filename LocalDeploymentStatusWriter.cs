using System.Threading.Tasks;
using Etg.Yams.Storage;
using Etg.Yams.Storage.Status;

namespace Wivuu.Yams.Local
{
    internal class LocalDeploymentStatusWriter : IDeploymentStatusWriter
    {
        public LocalDeploymentStatusWriter()
        {
        }

        public Task PublishInstanceDeploymentStatus(string clusterId, string instanceId, InstanceDeploymentStatus instanceDeploymentStatus)
        {
            return Task.CompletedTask;
        }
    }
}