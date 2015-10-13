Include settings.ps1
Include nuget.ps1
Include msbuild.ps1
Include assemblyinfo.ps1

Task Build -depends Clean, Restore-NuGetPackages, Build-Solution

Task Clean -depends Clean-Solution

Task Rebuild -depends Clean, Build

Task default -depends Build

Task Ci -depend default
