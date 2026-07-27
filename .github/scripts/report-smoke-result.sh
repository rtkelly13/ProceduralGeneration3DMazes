#!/usr/bin/env bash
# Post the preview smoke-test verdict back to the pull request.
#
# Runs with if: always() so a failure is reported rather than only showing in the run.
#
# Inputs (environment):
#   SMOKE_OUTCOME, ISSUE_NUMBER, REPOSITORY, GH_TOKEN
set -euo pipefail

if [ "$SMOKE_OUTCOME" = "success" ]; then
  gh pr comment "$ISSUE_NUMBER" --repo "$REPOSITORY" --body "✅ Smoke test passed on the preview (boots, cross-origin isolated, canvas rendered)."
else
  gh pr comment "$ISSUE_NUMBER" --repo "$REPOSITORY" --body "❌ Smoke test FAILED on the preview — the build served but did not boot correctly. Check the workflow logs."
fi
