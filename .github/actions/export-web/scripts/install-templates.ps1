# Install the web export templates into the editor bundle (self-contained mode).
#
# A `._sc_` marker makes the editor look for templates beside itself instead of in the user profile.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   TEMPLATE_VERSION, BUNDLE_DIR

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

$bundle = "$env:BUNDLE_DIR"
New-Item -ItemType File -Force -Path (Join-Path $bundle "._sc_") | Out-Null
$tplDir = Join-Path $bundle "editor_data/export_templates/${env:TEMPLATE_VERSION}"
New-Item -ItemType Directory -Force -Path $tplDir | Out-Null
$templates = Get-ChildItem -Path "$env:RUNNER_TEMP/godot" -Recurse -Include "web_release.zip","web_debug.zip"
if (-not $templates) {
  Get-ChildItem -Path "$env:RUNNER_TEMP/godot" -Recurse -File | Select-Object -ExpandProperty FullName
  throw "No web_release.zip / web_debug.zip found in the fork bundle."
}
$templates | ForEach-Object { Copy-Item $_.FullName -Destination $tplDir -Force; Write-Host "Installed template: $($_.Name)" }
Write-Host "Templates in ${tplDir}:"; Get-ChildItem $tplDir
