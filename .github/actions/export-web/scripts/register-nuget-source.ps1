# Register the fork's bundled NuGet source, when it ships one.
#
# The patched editor may carry Godot NuGet packages that are not on nuget.org.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

$nuget = Get-ChildItem -Path "$env:RUNNER_TEMP/godot" -Recurse -Directory -Filter "nuget" | Select-Object -First 1
if ($nuget) {
  Write-Host "Adding local NuGet source: $($nuget.FullName)"
  dotnet nuget add source "$($nuget.FullName)" --name godot-web-fork
} else {
  Write-Host "No bundled nuget folder found; relying on nuget.org."
}
