#!/usr/bin/env bash
# Verify the downloaded asset against its pinned SHA-256 before it is republished.
#
# Mirroring unverified bytes would launder a compromised upstream into a trusted location, which is worse than having no mirror.
#
# Inputs (environment):
#   ASSET
set -euo pipefail

# Mirroring unverified bytes would launder a compromised upstream into a trusted
# location, which is worse than having no mirror at all.
expected=$(grep -v '^\s*#' .github/editor-checksums.txt | grep "$ASSET" | awk '{print $1}')
if [ -z "$expected" ]; then
  echo "::error::No pinned checksum for $ASSET in .github/editor-checksums.txt."
  echo "::error::Add it first (see docs/WEB_EXPORT.md) — refusing to mirror unverified bytes."
  exit 1
fi
actual=$(sha256sum "$ASSET" | awk '{print $1}')
if [ "$expected" != "$actual" ]; then
  echo "::error::Checksum mismatch for $ASSET (expected $expected, got $actual)."
  exit 1
fi
echo "Checksum OK: $actual"
