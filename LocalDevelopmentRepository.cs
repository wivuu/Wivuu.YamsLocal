using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Etg.Yams;
using Etg.Yams.Application;
using Etg.Yams.Json;
using Etg.Yams.Storage;
using Etg.Yams.Storage.Config;
using Etg.Yams.Utils;
using Newtonsoft.Json.Serialization;

namespace Wivuu.Yams.Local
{
    public class LocalDevelopmentRepository : IDeploymentRepository
    {
        readonly string _path;
        readonly string _deploymentConfigPath;
        readonly IDeploymentConfigSerializer _serializer;

        IDictionary<string, AppDeploymentConfig> ActiveConfiguration { get; set; }

        public LocalDevelopmentRepository(string path, IDeploymentConfigSerializer serializer)
        {
            _path                 = path;
            _deploymentConfigPath = Path.Combine(_path, Constants.DeploymentConfigFileName);
            _serializer           = serializer;
        }

        string GetBinariesPath(AppIdentity appIdentity)
        {
            if (ActiveConfiguration.TryGetValue(appIdentity.Id, out var app) &&
                app.Properties.TryGetValue("ExePath", out var exePath))
                return exePath;

            else
                throw new BinariesNotFoundException($"{appIdentity.Id} needs an 'ExePath' property");
        }

        public async Task<DeploymentConfig> FetchDeploymentConfig()
        {
            using (var file = File.Open(_deploymentConfigPath, FileMode.Open, FileAccess.Read))
            using (var sr   = new StreamReader(file))
            {
                var config = _serializer.Deserialize(await sr.ReadToEndAsync());

                ActiveConfiguration = config.ToDictionary(app => app.AppIdentity.Id);

                return config;
            }
        }

        public Task PublishDeploymentConfig(DeploymentConfig deploymentConfig)
        {
            File.WriteAllText(_deploymentConfigPath, _serializer.Serialize(deploymentConfig));
            return Task.CompletedTask;
        }

        public Task UploadApplicationBinaries(AppIdentity appIdentity, string localPath, ConflictResolutionMode conflictResolutionMode)
        {
            if (FileUtils.DirectoryDoesntExistOrEmpty(localPath))
                throw new BinariesNotFoundException(
                    $"Binaries were not be uploaded because they were not found at the given path {localPath}");

            var destPath      = GetBinariesPath(appIdentity);
            var binariesExist = FileUtils.DirectoryDoesntExistOrEmpty(destPath);

            if (binariesExist)
            {
                if (conflictResolutionMode == ConflictResolutionMode.DoNothingIfBinariesExist)
                    return Task.CompletedTask;

                if (conflictResolutionMode == ConflictResolutionMode.FailIfBinariesExist)
                    throw new DuplicateBinariesException();
            }

            return FileUtils.CopyDir(_path, localPath, true);
        }

        public Task DeleteApplicationBinaries(AppIdentity appIdentity)
        {
            var path = GetBinariesPath(appIdentity);
            if (FileUtils.DirectoryDoesntExistOrEmpty(path))
                throw new BinariesNotFoundException(
                    $"Cannot delete binaries for application {appIdentity} because they were not found");

            Directory.Delete(path, true);
            return Task.CompletedTask;
        }

        public Task<bool> HasApplicationBinaries(AppIdentity appIdentity)
        {
            string path = GetBinariesPath(appIdentity);
            return Task.FromResult(!FileUtils.DirectoryDoesntExistOrEmpty(path));
        }

        public async Task DownloadApplicationBinaries(AppIdentity appIdentity, string localPath, ConflictResolutionMode conflictResolutionMode)
        {
            var exists = !FileUtils.DirectoryDoesntExistOrEmpty(localPath);

            if (exists)
            {
                if (conflictResolutionMode == ConflictResolutionMode.DoNothingIfBinariesExist)
                    return;

                if (conflictResolutionMode == ConflictResolutionMode.FailIfBinariesExist)
                    throw new DuplicateBinariesException(
                        $"Cannot download the binaries because the destination directory {localPath} contains files");
            }

            var path = GetBinariesPath(appIdentity);
            if (FileUtils.DirectoryDoesntExistOrEmpty(path))
                throw new BinariesNotFoundException($"The binaries were not found in the Yams repository");

            await FileUtils.CopyDir(path, localPath, true);
        }
    }

    public static class LocalDevelopmentFactory
    {
        /// <summary>
        /// Create new YAMS service for local development
        /// </summary>
        public static IYamsService Create(YamsConfig config, string activeDirectory = null)
        {
            if (activeDirectory == null)
                activeDirectory = Environment.CurrentDirectory;

            var updateSessionManager = new LocalUpdateSessionManager();
            var serializer           = new JsonSerializer(new DiagnosticsTraceWriter());
            var configSerializer     = new JsonDeploymentConfigSerializer(serializer);
            var deploymentRepository = new LocalDevelopmentRepository(activeDirectory, configSerializer);

            return new YamsDiModule(config, deploymentRepository, updateSessionManager).YamsService;
        }
    }
}