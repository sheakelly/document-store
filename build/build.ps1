Include settings.ps1
Include commonfunctions.ps1
Include nuget.ps1
Include msbuild.ps1
Include nunit.ps1

Task Build -depends Clean, Restore-NuGetPackages, Build-Solution

Task Clean -depends Clean-Solution

Task Rebuild -depends Clean, Build

Task default -depends Build, Run-Tests

Task Ci -depend Build, Create-NuGetPackage
