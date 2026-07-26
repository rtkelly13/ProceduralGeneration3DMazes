#!/usr/bin/env bash
# Verify the upstream Godot editor against its pinned SHA-256.
#
# Same supply-chain guard as the patched Windows editor; the scene-test job runs this binary.
#
# Inputs (environment):
#   GODOT_ASSET
set -euo pipefail

# Same supply-chain guard as the patched web editor: never run an unverified binary.
expected=$(grep -v '^\s*#' .github/editor-checksums.txt | grep "$GODOT_ASSET.zip" | awk '{print $1}')
if [ -z "$expected" ]; then
  echo "::error::No pinned checksum for $GODOT_ASSET.zip in .github/editor-checksums.txt"
  exit 1
fi
actual=$(sha256sum "$RUNNER_TEMP/godot/$GODOT_ASSET.zip" | awk '{print $1}')
if [ "$expected" != "$actual" ]; then
  echo "::error::Checksum mismatch for $GODOT_ASSET.zip (expected $expected, got $actual)"
  exit 1
fi
echo "Checksum OK: $actual"
