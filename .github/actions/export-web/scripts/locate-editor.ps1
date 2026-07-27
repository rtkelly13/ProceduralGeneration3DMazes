# Find the Godot editor executable inside the extracted fork bundle (prefer the
# console build) and report it plus its directory for later steps.
# Outputs: godot_exe, bundle_dir

$ErrorActionPreference = 'Stop'

$root = "$env:RUNNER_TEMP/godot"
$exe = Get-ChildItem -Path $root -Recurse -Filter "*.exe" |
       Where-Object { $_.Name -match "console" } | Select-Object -First 1
if (-not $exe) {
  $exe = Get-ChildItem -Path $root -Recurse -Filter "*.exe" |
         Where-Object { $_.Name -notmatch "crash|handler" } | Select-Object -First 1
}
if (-not $exe) { throw "Could not find a Godot editor executable in the fork zip." }
Write-Host "Editor exe: $($exe.FullName)"
"godot_exe=$($exe.FullName)" >> $env:GITHUB_OUTPUT
"bundle_dir=$($exe.Directory.FullName)" >> $env:GITHUB_OUTPUT
