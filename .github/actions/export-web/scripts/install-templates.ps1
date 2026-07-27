# Install the bundled web export templates in self-contained mode (next to the
# editor, via a "._sc_" marker — no AppData).
# Env in: BUNDLE_DIR, TEMPLATE_VERSION

$ErrorActionPreference = 'Stop'

$bundle = $env:BUNDLE_DIR
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
