#!/usr/bin/env bash
# Publish (or refresh) the mirrored editor release on this repository.
#
# Idempotent: re-running replaces the asset rather than failing on an existing tag.
#
# Inputs (environment):
#   TAG, ASSET, SOURCE_REPO, TARGET_REPO, GH_TOKEN
set -euo pipefail

notes="Mirror of \`$SOURCE_REPO@$TAG\` for the C#/WASM web export.

Byte-identical to upstream and verified against the SHA-256 pinned in
\`.github/editor-checksums.txt\` before upload. Exists so the export pipeline does not
depend on a third-party release staying published — see docs/WEB_EXPORT_ROADMAP.md.

Not a fork and not modified. To use it, set \`GODOT_FORK_REPO\` in
\`.github/web-toolchain.env\` to this repository."

# Idempotent: re-running replaces the asset rather than failing on an existing tag.
if gh release view "$TAG" --repo "$TARGET_REPO" >/dev/null 2>&1; then
  gh release upload "$TAG" "$ASSET" --repo "$TARGET_REPO" --clobber
else
  gh release create "$TAG" "$ASSET" --repo "$TARGET_REPO" \
    --title "Mirrored editor $TAG" --notes "$notes"
fi

echo "### Mirrored \`$ASSET\`" >> "$GITHUB_STEP_SUMMARY"
echo "" >> "$GITHUB_STEP_SUMMARY"
echo "Point \`GODOT_FORK_REPO\` in \`.github/web-toolchain.env\` at \`$TARGET_REPO\` to use it." >> "$GITHUB_STEP_SUMMARY"
