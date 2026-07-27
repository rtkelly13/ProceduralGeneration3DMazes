# Write build-info.json beside index.html.
#
# Lets the served build be identified with a plain curl, without downloading ~96 MB of WASM.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   BUILD_COMMIT, BUILD_BRANCH, BUILD_REPOSITORY, BUILD_RUN_ID, BUILD_RUN_ATTEMPT,
#   BUILD_TIMESTAMP, FORK_TAG

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

$info = [ordered]@{
  commit      = $env:BUILD_COMMIT
  branch      = $env:BUILD_BRANCH
  repository  = $env:BUILD_REPOSITORY
  runId       = $env:BUILD_RUN_ID
  runAttempt  = $env:BUILD_RUN_ATTEMPT
  builtUtc    = $env:BUILD_TIMESTAMP
  godotEditor = $env:FORK_TAG
}
$info | ConvertTo-Json | Set-Content -Path "build/web/build-info.json" -Encoding utf8
Write-Host "build-info.json:"; Get-Content "build/web/build-info.json"
