#!/usr/bin/env bash
# Decide which editor asset to mirror.
#
# Reads the same toolchain file the export action uses, so a mirror cannot drift from the version actually being built with.
#
# Inputs (environment):
#   IN_TAG, IN_REPO  (blank = read from .github/web-toolchain.env)
set -euo pipefail

# Same single source of truth the export action uses, so a mirror cannot drift from
# the version actually being built with.
cfg=.github/web-toolchain.env
tag="${IN_TAG:-$(grep -E '^GODOT_FORK_TAG=' $cfg | cut -d= -f2)}"
repo="${IN_REPO:-$(grep -E '^GODOT_FORK_REPO=' $cfg | cut -d= -f2)}"
asset="Godot_v${tag}_mono_web_export_win64.zip"
echo "tag=$tag"     >> "$GITHUB_OUTPUT"
echo "repo=$repo"   >> "$GITHUB_OUTPUT"
echo "asset=$asset" >> "$GITHUB_OUTPUT"
echo "Mirroring $asset from $repo@$tag"
