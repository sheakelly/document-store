properties {
  Write-Output "Loading nunit properties"
  $nunit = @{}
  $nunit.runner = (Get-ChildItem "$($source.dir)\*" -recurse -include nunit-console-x86.exe).FullName
}

Task Run-Tests {
  try {
	  Push-Location "$($source.dir)"
	  $pattern = ".*\\bin\\.*\.Test[s]?.dll$"
	  $testAssemblies = Get-ChildItemToDepth -ToDepth 4 | where {[regex]::match($_.FullName, $pattern, "IgnoreCase").Success	}
		$testfiles = @()
    if ($testAssemblies) {
      foreach ($file in $testAssemblies)
      {
        $testfiles += $file.FullName
      }
    }
		Write-Host "Found test assemblies $testfiles"
	  Invoke-TestRunner $testfiles
	} catch { Pop-Location; throw; }
}

function Invoke-TestRunner {
  param(
    [Parameter(Position=0,Mandatory=$true)]
    [string[]]$dlls = @()
  )

  Assert ((Test-Path($nunit.runner)) -and (![string]::IsNullOrEmpty($nunit.runner))) "NUnit runner could not be found"

  if ($dlls.Length -le 0) {
     Write-Output "No tests defined"
     return
  }

	Write-Host "nunit.runner $nunit.runner"
  exec { & $nunit.runner $dlls /noshadow }
}
