# Verify the editor zip against its pinned SHA-256 — a supply-chain guard for the
# third-party binary we run. Runs on both fresh downloads and cache hits, and matches
# the expected filename exactly so a restored cache can't substitute a different zip.
#
# Env in: ASSET

$ErrorActionPreference = 'Stop'

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
