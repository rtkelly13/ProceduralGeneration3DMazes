# Resolve the web-export toolchain versions: read .github/web-toolchain.env (the
# single source of truth), then let non-empty action inputs override individual values.
#
# Env in:  IN_FORK_REPO, IN_FORK_TAG, IN_TEMPLATE_VERSION (action inputs, may be blank)
# Outputs: fork_repo, fork_tag, template_version, asset

$ErrorActionPreference = 'Stop'

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

$forkRepo = Resolve-Value $env:IN_FORK_REPO        'GODOT_FORK_REPO'
$forkTag  = Resolve-Value $env:IN_FORK_TAG         'GODOT_FORK_TAG'
$template = Resolve-Value $env:IN_TEMPLATE_VERSION 'GODOT_TEMPLATE_VERSION'

"fork_repo=$forkRepo"        >> $env:GITHUB_OUTPUT
"fork_tag=$forkTag"          >> $env:GITHUB_OUTPUT
"template_version=$template" >> $env:GITHUB_OUTPUT
"asset=Godot_v${forkTag}_mono_web_export_win64.zip" >> $env:GITHUB_OUTPUT
