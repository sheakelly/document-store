Include settings.ps1
Include commonfunctions.ps1
Include nuget.ps1
Include msbuild.ps1
Include assemblyinfo.ps1
Include nunit.ps1

Task Build -depends Clean, Restore-NuGetPackages, Build-Solution, Run-Tests

Task Clean -depends Clean-Solution

Task Rebuild -depends Clean, Build

Task default -depends Build

Task Ci -depend default, Create-NuGetPackage
