properties {
    Write-Output "Loading nuget properties"
    $nuget = @{}
    $nuget.exe = $source.dir + "/.nuget/NuGet.exe"
    $nuget.server = "http://nuget-local/api/v2;http://nuget/nuget/Default"
    $nuget.push_server = "http://nuget-local/"
    $nuget.api_key = "fc04c61c-ced2-4041-833c-3237194577b5"
}

Task Create-NuGetPackage -depends Set-NuSpecVersion {
	$version.package = $version.full
	Assert (Test-Path($nuget.exe)) "Could not find $($nuget.exe)"
	$specfiles = Get-ChildItemToDepth -ToDepth 5 -Path $($source.dir) | where {$_.FullName -like "*.versioned.nuspec" -and $_.FullName -notlike "*\bin\*.versioned.nuspec"}
	foreach ($specfile in $specfiles) {
	Write-Host "Creating Nuget package from: $($specfile.FullName)"
			$base_path = Split-Path $specfile.FullName
			exec { & $nuget.exe pack $specfile.FullName -Version $version.package -OutputDirectory $base_path -BasePath $base_path -NoPackageAnalysis}
	}
}

Task Publish-NuGetPackage {
	$version.package = $version.full
	$packages = Get-ChildItemToDepth -ToDepth 5 -Path $($source.dir) | where {$_.FullName -like "*.$($version.package).nupkg" -and $_.Directory.Parent.Name -ne "packages"}

	foreach ($package in $packages) {
		Exec { & "$($nuget.exe)" push -Source "$($nuget.push_server)" "$($package.FullName)" "$($nuget.api_key)" }
	}
}

Task Set-NuSpecVersion {
	$version.package = SlugifyVersion
    try {
				Push-Location "$($source.dir)"
				$files = Get-ChildItemToDepth -ToDepth 5 -Path $($source.dir) | where {$_.FullName -like "*.nuspec" -and $_.FullName -notlike "*.versioned.nuspec" -and $_.FullName -notlike "*\packages\*.nuspec" -and $_FullName -notlike "*\bin\*.nuspec" }

				foreach ($file in $files) {
						Write-Host "nuspec $($file.FullName)"
						$version_pattern = "<version></version>"

            $outfile = $file.FullName -replace ".nuspec$", ".versioned.nuspec"

						Get-Content $file.FullName | Out-File $outfile
						$content = Get-Content $outfile | % { [Regex]::Replace($_, $version_pattern, "<version>$($version.package)</version>") }
						Set-Content -Value $content -Path $outfile
				}
    } catch { Pop-Location; throw; }
}

Task Restore-NuGetPackages {
    Assert (Test-Path($nuget.exe)) "Could not find $($nuget.exe)"
		Write-Output "Retoring Nuget Dependencies"
    Exec { .$nuget.exe restore $solution.file }
}
