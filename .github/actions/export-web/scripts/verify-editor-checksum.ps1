# Verify the downloaded editor against its pinned SHA-256.
#
# Runs on cache hits as well as fresh downloads, and matches the expected filename exactly so a restored cache cannot substitute a different editor.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   ASSET  (expected zip filename)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

# Runs on both fresh downloads and cache hits — verifies the exact bytes we execute.
# Match the expected filename explicitly: a restored cache must not be able to
# substitute a differently-named editor zip for the one we pinned.
$zip = Get-ChildItem "${env:RUNNER_TEMP}/godot-zip" -Filter *.zip |
       Where-Object { $_.Name -eq $env:ASSET } | Select-Object -First 1
if (-not $zip) {
  Write-Host "Contents of ${env:RUNNER_TEMP}/godot-zip:"
  Get-ChildItem "${env:RUNNER_TEMP}/godot-zip" -ErrorAction SilentlyContinue | ForEach-Object { Write-Host "  $($_.Name)" }
  throw "Expected editor zip '$env:ASSET' not found (cache or download failed)."
}
$line = Get-Content .github/editor-checksums.txt |
        Where-Object { $_ -notmatch '^\s*#' -and $_ -match [regex]::Escape($zip.Name) } | Select-Object -First 1
if (-not $line) { throw "No pinned checksum for '$($zip.Name)' in .github/editor-checksums.txt — refusing to run an unverified binary. See docs/WEB_EXPORT.md." }
$expected = (($line -split '\s+') | Where-Object { $_ })[0].ToLower()
$actual = (Get-FileHash $zip.FullName -Algorithm SHA256).Hash.ToLower()
if ($actual -ne $expected) {
  throw "Checksum MISMATCH for $($zip.Name).`n  expected: $expected`n  actual:   $actual`nThe fork release may have been re-published — verify and update .github/editor-checksums.txt."
}
Write-Host "Checksum OK for $($zip.Name): $actual"
