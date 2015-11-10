properties {

  $base = @{}
  $base.dir = Resolve-Path ..

  $source = @{}
  $source.dir = "$($base.dir)\src"

  $build = @{}
  $build.dir = "$($base.dir)\build"

  $solution = @{}
  $solution.name = "Prim"
  $solution.file = "$($source.dir)\$($solution.name).sln"

  $version = @{}
	$version.full = if($env:APPVEYOR_BUILD_VERSION) {$env:APPVEYOR_BUILD_VERSION} else {"0.0.0"}
}
