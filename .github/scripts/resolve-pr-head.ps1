# Resolve the PR head SHA and branch for a /preview run.
#
# A preview builds the PR head; github.sha on an issue_comment event is the default branch, so it cannot be used.
#
# Lives in a file rather than a YAML block scalar so it can be linted and read; see
# docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   ISSUE_NUMBER, REPOSITORY, GH_TOKEN

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$sha = gh pr view $env:ISSUE_NUMBER --repo $env:REPOSITORY --json headRefOid -q ".headRefOid"
"sha=$sha" >> $env:GITHUB_OUTPUT
# Needed for the build stamp: a preview builds the PR head, so github.sha (the
# default branch, for an issue_comment event) would stamp the wrong commit entirely.
$ref = gh pr view $env:ISSUE_NUMBER --repo $env:REPOSITORY --json headRefName -q ".headRefName"
"ref=$ref" >> $env:GITHUB_OUTPUT
