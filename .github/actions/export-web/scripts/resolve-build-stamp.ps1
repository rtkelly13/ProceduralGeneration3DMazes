# Resolve the build stamp compiled into the binary.
#
# Derived here rather than trusted from the caller: an issue_comment run takes the workflow file from the default branch, so a caller's inputs can be silently absent.
#
# Extracted from action.yml rather than inlined there: a 40-line script inside a YAML block
# scalar cannot be linted, cannot be run outside CI, and silently breaks on constructs the
# block scalar swallows -- a PowerShell here-string (@"..."@) needs its terminator at column 0,
# which ends the YAML block. See docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   IN_COMMIT, IN_BRANCH, IN_REPOSITORY, IN_RUN_ID, IN_RUN_ATTEMPT  (overrides; blank = derive)
#   CTX_REPOSITORY, CTX_RUN_ID, CTX_RUN_ATTEMPT                     (from the github context)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Actions' pwsh wrapper sets these; set them here too so the script behaves identically when
# run by hand, and so a failing native command (git/gh/dotnet/godot) is not silently ignored.
$PSNativeCommandUseErrorActionPreference = $true

# Derived HERE, with the caller's inputs as optional overrides, because the caller
# cannot always be trusted to supply them.
#
# A `/preview` run proved why: for issue_comment events GitHub loads the WORKFLOW FILE
# from the default branch, while the composite action comes from the checked-out PR
# head. A PR that adds `with: build_commit:` therefore has its own caller ignored, and
# every caller-supplied field arrived empty — the artefact built fine and reported
# itself as untraceable. Reading the commit from the checkout instead makes the stamp
# correct no matter which caller invoked us, or whether it passed anything at all.
function Resolve-Stamp($override, $fallback, $name) {
  if (-not [string]::IsNullOrWhiteSpace($override)) { return $override.Trim() }
  return $fallback
}

# The commit that is actually checked out — the thing being built, by definition.
$gitCommit = ""
try {
  $gitCommit = (git rev-parse HEAD 2>$null | Out-String).Trim()
} catch {
  Write-Host "::warning::git rev-parse HEAD failed; build will be unstamped."
}
if ($gitCommit -notmatch '^[0-9a-fA-F]{40}$') { $gitCommit = "" }

# A checkout by SHA is detached, so this is usually "HEAD" — not a branch name. Only
# report it when it is a real one; a wrong branch is worse than a missing one.
$gitBranch = ""
try {
  $b = (git rev-parse --abbrev-ref HEAD 2>$null | Out-String).Trim()
  if ($b -and $b -ne "HEAD") { $gitBranch = $b }
} catch { }

$commit     = Resolve-Stamp $env:IN_COMMIT      $gitCommit            'commit'
$branch     = Resolve-Stamp $env:IN_BRANCH      $gitBranch            'branch'
$repository = Resolve-Stamp $env:IN_REPOSITORY  $env:CTX_REPOSITORY   'repository'
$runId      = Resolve-Stamp $env:IN_RUN_ID      $env:CTX_RUN_ID       'runId'
$runAttempt = Resolve-Stamp $env:IN_RUN_ATTEMPT $env:CTX_RUN_ATTEMPT  'runAttempt'

# Recorded here so it reflects when the artefact was actually produced. The same
# string reaches the About screen and build-info.json — they are meant to be
# compared literally.
$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

"commit=$commit"         >> $env:GITHUB_OUTPUT
"branch=$branch"         >> $env:GITHUB_OUTPUT
"repository=$repository" >> $env:GITHUB_OUTPUT
"run_id=$runId"          >> $env:GITHUB_OUTPUT
"run_attempt=$runAttempt" >> $env:GITHUB_OUTPUT
"timestamp=$timestamp"   >> $env:GITHUB_OUTPUT

Write-Host "Build stamp:"
Write-Host "  commit     = $(if ($commit) { $commit } else { '(none — build will report itself untraceable)' })"
Write-Host "  branch     = $branch"
Write-Host "  repository = $repository"
Write-Host "  run        = $runId (attempt $runAttempt)"
Write-Host "  timestamp  = $timestamp"
if (-not $commit) {
  Write-Host "::warning::No commit resolved — the About screen will report this as a local, untraceable build."
}
