# Run the headless web export and fail loudly if no output was produced.
# Env in: GODOT_EXE

$ErrorActionPreference = 'Stop'

New-Item -ItemType Directory -Force -Path "build/web" | Out-Null
& "$env:GODOT_EXE" --headless --path . --export-release "Web" "build/web/index.html" 2>&1 | Tee-Object -FilePath export.log
if (-not (Test-Path "build/web/index.html")) {
  Write-Host "::error::Export did not produce build/web/index.html — see export.log"
  exit 1
}
Write-Host "Export output:"; Get-ChildItem build/web
