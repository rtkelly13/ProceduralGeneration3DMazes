# Locate the editor executable inside the extracted fork bundle.
#
# The fork's zip layout is not guaranteed stable, so the exe is discovered rather than assumed.
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
