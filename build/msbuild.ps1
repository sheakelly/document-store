properties {
	Write-Output "Loading MSBuild properties"
	$build.configuration = if($build.configuration) { $build.configuration } else { "Release" }
	Write-Output "$($build.configuration)"
}

Task Clean-Solution {
  Exec { msbuild "$($solution.file)" /t:Clean "/p:Configuration=$($build.configuration)" /verbosity:quiet /nologo }
}

Task Build-Solution -depends Clean-Solution {
  $verbosity = ($build.verbosity, "quiet" -ne $null)[0]
  Exec { msbuild "$($solution.file)" /t:Build "/p:Configuration=$($build.configuration)" /verbosity:$verbosity /nologo }
}
