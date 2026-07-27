# Fail fast (before the 165 MB editor download) if the Godot.NET.Sdk version (csproj),
# the patched-editor tag, and the export-template folder disagree at PATCH level, or if
# the target editor asset has no pinned checksum.
#
# The three values spell the same version differently; normalise to bare x.y[.z]:
#   csproj    "4.7.1"             -> 4.7.1
#   template  "4.7.1.stable.mono" -> 4.7.1
#   fork tag  "4.7.1-stable"      -> 4.7.1
# Godot names x.y releases without a patch component (template "4.7.stable.mono"),
# so a csproj SDK of "4.7.0" normalises to "4.7".
#
# Env in: TEMPLATE_VERSION, FORK_TAG, ASSET

$ErrorActionPreference = 'Stop'

function Get-BareVersion([string]$s) {
  return ($s -replace '[-.](stable|beta|rc|dev)[-.0-9]*.*$', '')
}

$csproj = Get-Content ProceduralGeneration3DMazes.csproj -Raw
if ($csproj -notmatch 'Godot\.NET\.Sdk/(\d+\.\d+\.\d+)') { throw "Could not parse Godot.NET.Sdk version from csproj." }
$sdkVersion = $Matches[1] -replace '\.0$', ''
$tplVersion = Get-BareVersion $env:TEMPLATE_VERSION
$tagVersion = Get-BareVersion $env:FORK_TAG

if (($sdkVersion -ne $tplVersion) -or ($sdkVersion -ne $tagVersion)) {
  throw @"
Version drift - these must all describe the same Godot version:
  Godot.NET.Sdk (csproj)  -> $sdkVersion
  template_version        -> $tplVersion   (raw: $env:TEMPLATE_VERSION)
  godot_fork_tag          -> $tagVersion   (raw: $env:FORK_TAG)
Align them in ProceduralGeneration3DMazes.csproj and .github/web-toolchain.env.
See docs/WEB_EXPORT.md -> 'Updating the pinned editor'.
"@
}

$pinned = Get-Content .github/editor-checksums.txt |
          Where-Object { $_ -notmatch '^\s*#' -and $_ -match [regex]::Escape($env:ASSET) }
if (-not $pinned) {
  throw "No pinned SHA-256 for '$env:ASSET' in .github/editor-checksums.txt — refusing to run an unverified binary. See docs/WEB_EXPORT.md -> 'Updating the pinned editor'."
}

Write-Host "Version check OK: Godot.NET.Sdk, templates and editor tag all agree on $sdkVersion (checksum pinned for $env:ASSET)."
