# Fail fast if the Godot SDK, export templates and editor tag disagree.
#
# Compares at PATCH level -- major.minor alone lets a 4.7.0-vs-4.7.1 mismatch through, which is the most common kind of bump.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   TEMPLATE_VERSION, FORK_TAG, ASSET

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

# The Godot.NET.Sdk version (csproj), the patched-editor tag, and the
# export-template folder must agree at PATCH level — comparing only
# major.minor lets a 4.7.0-vs-4.7.1 mismatch through, which is exactly the
# class of bump that happens most often. Catch it in seconds, before we
# download a 165 MB editor.
#
# Normalise the three spellings of the same version to a bare x.y[.z]:
#   csproj    "4.7.1"             -> 4.7.1     ("4.7.0" -> 4.7, see below)
#   template  "4.7.1.stable.mono" -> 4.7.1
#   fork tag  "4.7.1-stable"      -> 4.7.1
# Godot names x.y releases without a patch component, so an SDK version of
# "4.7.0" corresponds to template "4.7.stable.mono": strip a trailing ".0".
function Get-BareVersion([string]$s) {
  return ($s -replace '[-.](stable|beta|rc|dev)[-.0-9]*.*$', '')
}

$csproj = Get-Content ProceduralGeneration3DMazes.csproj -Raw
if ($csproj -notmatch 'Godot\.NET\.Sdk/(\d+\.\d+\.\d+)') { throw "Could not parse Godot.NET.Sdk version from csproj." }
$sdkVersion = $Matches[1] -replace '\.0$', ''
$tplVersion = Get-BareVersion $env:TEMPLATE_VERSION
$tagVersion = Get-BareVersion $env:FORK_TAG

$mismatch = ($sdkVersion -ne $tplVersion) -or ($sdkVersion -ne $tagVersion)
if ($mismatch) {
  # Built as an array rather than a here-string: a PowerShell here-string needs
  # its closing "@ at column 0, which would terminate this YAML block scalar.
  throw (@(
    "Version drift - these must all describe the same Godot version:",
    "  Godot.NET.Sdk (csproj)  -> $sdkVersion",
    "  template_version        -> $tplVersion   (raw: $env:TEMPLATE_VERSION)",
    "  godot_fork_tag          -> $tagVersion   (raw: $env:FORK_TAG)",
    "Align them in ProceduralGeneration3DMazes.csproj and .github/web-toolchain.env.",
    "See docs/WEB_EXPORT.md -> 'Updating the pinned editor'."
  ) -join "`n")
}

# Fail fast if the editor we're about to fetch has no pinned checksum, rather
# than after spending a runner-minute downloading it.
$pinned = Get-Content .github/editor-checksums.txt |
          Where-Object { $_ -notmatch '^\s*#' -and $_ -match [regex]::Escape($env:ASSET) }
if (-not $pinned) {
  throw "No pinned SHA-256 for '$env:ASSET' in .github/editor-checksums.txt — refusing to run an unverified binary. See docs/WEB_EXPORT.md -> 'Updating the pinned editor'."
}

Write-Host "Version check OK: Godot.NET.Sdk, templates and editor tag all agree on $sdkVersion (checksum pinned for $env:ASSET)."
