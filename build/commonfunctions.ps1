Function Get-ChildItemToDepth {
  Param(
    [String]$Path = $PWD,
    [String]$Filter = "*",
    [Byte]$ToDepth = 255,
    [Byte]$CurrentDepth = 0,
    [Switch]$DebugMode
  )
  $CurrentDepth++
  If ($DebugMode) { $DebugPreference = "Continue" }
  Get-ChildItem $Path | %{
    $_ | ?{ $_.Name -Like $Filter }
    If ($_.PsIsContainer) {
      If ($CurrentDepth -le $ToDepth) {
        # Callback to this function
        Get-ChildItemToDepth -Path $_.FullName -Filter $Filter `
          -ToDepth $ToDepth -CurrentDepth $CurrentDepth
      } Else {
        Write-Debug $("Skipping GCI for Folder: $($_.FullName) " + `
          "(Why: Current depth $CurrentDepth vs limit depth $ToDepth)")
      }
    }
  }
}
