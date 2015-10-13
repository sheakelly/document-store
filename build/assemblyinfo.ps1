properties {
  Write-Output "Loading assembly info properties"
  # The assemblyinfo filename and location can be overridden in the settings.ps1
  # You can also override the version pattern to "\d*\.\d*\.\d*\.\d*" if you want 4 digit
  $assemblyinfo = @{}
  $assemblyinfo.dir = "$($source.dir)"
  $assemblyinfo.version_pattern = "\d*\.\d*\.\d*\.\d*"
  $assemblyinfo.file = "GlobalAssemblyInfo.cs"
  $assemblyinfo.contents = @"
using System.Reflection;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyCompany(`"`")]
[assembly: AssemblyProduct(`"`")]
[assembly: AssemblyCopyright(`"`")]
[assembly: AssemblyTrademark(`"`")]
[assembly: AssemblyCulture(`"`")]
[assembly: AssemblyVersion(`"0.0.0.0`")]
[assembly: AssemblyFileVersion(`"0.0.0.0`")]
"@
}

Task Version-AssemblyInfo {
  $file = "$($assemblyinfo.dir)\$($assemblyinfo.file)"
	Write-Host "assembly info file: $file"
  if(!(Test-Path($file))) {
    Set-Content -Value $assemblyinfo.contents -Path $file
    Write-Host -ForegroundColor Yellow "GlobalAssemblyInfo was not detected has has been created: $($assemblyinfo.file)"
  }
	Write-Host "Using build version $($version.full)"
  $content = Get-Content $file | % { [Regex]::Replace($_, "$($assemblyinfo.version_pattern)", "$(($version.full_numeric, $version.full -ne $null)[0])") }
  Set-Content -Value $content -Path $file
}
