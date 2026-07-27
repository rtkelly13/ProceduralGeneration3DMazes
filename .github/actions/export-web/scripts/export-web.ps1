# Run the headless web export and confirm it produced output.
#
# The export can exit 0 while producing nothing, so index.html is checked explicitly.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   GODOT_EXE

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

New-Item -ItemType Directory -Force -Path "build/web" | Out-Null
& "$env:GODOT_EXE" --headless --path . --export-release "Web" "build/web/index.html" 2>&1 | Tee-Object -FilePath export.log
if (-not (Test-Path "build/web/index.html")) {
  Write-Host "::error::Export did not produce build/web/index.html — see export.log"
  exit 1
}
Write-Host "Export output:"; Get-ChildItem build/web
