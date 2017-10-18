using System;
using System.Collections.Generic;
using System.IO;
using Etg.Yams.Application;
using Etg.Yams.Json;
using Etg.Yams.Storage.Config;
using Newtonsoft.Json.Serialization;

namespace Wivuu.Yams.Local
{
    public class LocalAppDeploymentConfig : AppDeploymentConfig
    {
        public string ExeRoot { get; }

        public DateTimeOffset? EntryPointCreatedDate { get; }

        public LocalAppDeploymentConfig(string exeRoot, AppIdentity appIdentity) : base(appIdentity)
        {
            ExeRoot               = exeRoot;
            EntryPointCreatedDate = GetWatchFileModified(exeRoot, appIdentity);
        }

        public LocalAppDeploymentConfig(string exeRoot, AppIdentity appIdentity, IEnumerable<string> targetClusters, IReadOnlyDictionary<string, string> properties) : base(appIdentity, targetClusters, properties)
        {
            ExeRoot               = exeRoot;
            EntryPointCreatedDate = GetWatchFileModified(exeRoot, appIdentity);
        }

        static DateTimeOffset? GetWatchFileModified(string exeRoot, AppIdentity identity)
        {
            var configPath = Path.Combine(exeRoot, "AppConfig.json");

            if (File.Exists(configPath))
            {
                var parser = new ApplicationConfigParser(
                    new ApplicationConfigSymbolResolver(null, null),
                    new JsonSerializer(new DiagnosticsTraceWriter()));

                // Parse config file and return ExeName
                var appConfig = parser.ParseFile(
                    Path.Combine(exeRoot, "AppConfig.json"), 
                    new Etg.Yams.Install.AppInstallConfig(identity)).Result;

                var watchPath = Path.Combine(exeRoot, appConfig.ExeName);

                if (File.Exists(watchPath))
                    return File.GetLastWriteTime(watchPath);
            }

            // Config file does not exist
            return null;
        }
    }
}
