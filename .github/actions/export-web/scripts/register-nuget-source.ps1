# Register the fork's bundled local NuGet source, if the bundle ships one.

$ErrorActionPreference = 'Stop'

$nuget = Get-ChildItem -Path "$env:RUNNER_TEMP/godot" -Recurse -Directory -Filter "nuget" | Select-Object -First 1
if ($nuget) {
  Write-Host "Adding local NuGet source: $($nuget.FullName)"
  dotnet nuget add source "$($nuget.FullName)" --name godot-web-fork
} else {
  Write-Host "No bundled nuget folder found; relying on nuget.org."
}
