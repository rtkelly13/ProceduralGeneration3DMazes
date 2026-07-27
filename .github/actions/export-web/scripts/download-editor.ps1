# Download the patched Godot editor zip from the fork release.
# Env in: GH_TOKEN, FORK_REPO, FORK_TAG, ASSET

$ErrorActionPreference = 'Stop'

New-Item -ItemType Directory -Force -Path "${env:RUNNER_TEMP}/godot-zip" | Out-Null
Write-Host "Downloading $env:ASSET from $env:FORK_REPO@$env:FORK_TAG"
gh release download "$env:FORK_TAG" --repo "$env:FORK_REPO" --pattern "$env:ASSET" --dir "${env:RUNNER_TEMP}/godot-zip"
if ($LASTEXITCODE -ne 0) { throw "gh release download failed (exit $LASTEXITCODE)." }
