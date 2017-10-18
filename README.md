# YamsLocal

Use YAMS deployment model during local development. Update your DeploymentConfig.json to include a "LocalExePath" in each application and they will automatically be copied and launched when you run your Yams project when initializing with the LocalDevelopmentFactory included in this package.

Rebuilding your Application should also automatically be picked up, causing your active YAMS process to exit the old version of your app and relaunch the new version.


## DeploymentConfig.json

```json
{
  "Applications": [
    {
      "Id": "My.Project.Name",
      "Version": "1.0.0",
      "TargetClusters": [ "MY_SERVICE" ],
      "Properties": {
        "LocalExePath": "../../../My.Project.Name/bin/Debug"
      }
    }
  ]
}
```

## Program.cs

```c#
[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
public static void Main(string[] args)
{
    var clusterId                   = "MY_SERVICE";
    var updateDomain                = ...;
    var instanceId                  = Environment.MachineName;
    var applicationInstallDirectory = Path.Combine(Environment.CurrentDirectory, "LocalStore");

    // Follow YAMS tutorials found in their main
    // repository in order to create a YAMS configuration
    // which matches your needs
    var yamsConfig = new YamsConfigBuilder(
        clusterId,
        updateDomain,
        instanceId,
        applicationInstallDirectory).Build();

    // ...
    var yamsService = LocalDevelopmentFactory.Create(yamsConfig);

    yamsService.Start().Wait();
}

```
