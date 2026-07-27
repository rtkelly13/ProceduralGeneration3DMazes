# Resolve the Godot toolchain versions for this export.
#
# Single source of truth is .github/web-toolchain.env; non-empty action inputs override it.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   IN_FORK_REPO, IN_FORK_TAG, IN_TEMPLATE_VERSION  (blank = read from the toolchain file)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

# Read the shared toolchain file, then let non-empty inputs override it.
$path = ".github/web-toolchain.env"
if (-not (Test-Path $path)) { throw "Missing $path — the web export toolchain versions live there. See docs/WEB_EXPORT.md." }
$cfg = @{}
foreach ($line in Get-Content $path) {
  if ($line -match '^\s*#' -or $line -notmatch '=') { continue }
  $k, $v = $line -split '=', 2
  $cfg[$k.Trim()] = $v.Trim()
}

function Resolve-Value($inputValue, $key) {
  if (-not [string]::IsNullOrWhiteSpace($inputValue)) {
    Write-Host "$key = $inputValue (overridden by workflow input)"
    return $inputValue
  }
  if (-not $cfg.ContainsKey($key) -or [string]::IsNullOrWhiteSpace($cfg[$key])) {
    throw "$key is not set in .github/web-toolchain.env and no override was supplied."
  }
  Write-Host "$key = $($cfg[$key]) (from .github/web-toolchain.env)"
  return $cfg[$key]
}

$forkRepo = Resolve-Value $env:IN_FORK_REPO       'GODOT_FORK_REPO'
$forkTag  = Resolve-Value $env:IN_FORK_TAG        'GODOT_FORK_TAG'
$template = Resolve-Value $env:IN_TEMPLATE_VERSION 'GODOT_TEMPLATE_VERSION'

"fork_repo=$forkRepo"               >> $env:GITHUB_OUTPUT
"fork_tag=$forkTag"                 >> $env:GITHUB_OUTPUT
"template_version=$template"        >> $env:GITHUB_OUTPUT
"asset=Godot_v${forkTag}_mono_web_export_win64.zip" >> $env:GITHUB_OUTPUT
