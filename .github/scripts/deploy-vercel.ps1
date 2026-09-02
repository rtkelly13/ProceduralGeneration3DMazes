# Deploy build/web to Vercel, to preview or production.
#
# Shared by the production and preview jobs so the two cannot drift: both get the same target guard, the same DEPLOY_URL marker line and the same step summary.
#
# Lives in a file rather than a YAML block scalar so it can be linted and read; see
# docs/WEB_EXPORT.md.
#
# Inputs (environment):
#   EVENT_NAME, GIT_REF, IN_TARGET (blank = preview), SLOT_NUMBER, BUILD_COMMIT, VERCEL_TOKEN

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

Copy-Item infra/vercel/vercel.json build/web/vercel.json -Force
npm i -g vercel@latest

# Determine if this deploy targets a preview slot (slot/1..3)
$slot = if ($env:GIT_REF -match '^refs/heads/slot/([0-9]+)$') { $Matches[1] }
        elseif (![string]::IsNullOrWhiteSpace($env:SLOT_NUMBER)) { $env:SLOT_NUMBER }
        else { $null }

# Target is STATED, not inferred from the ref.
$target = if ($env:EVENT_NAME -eq 'push' -and $env:GIT_REF -eq 'refs/heads/main') { 'production' }
          elseif ($slot) { 'slot' }
          elseif ([string]::IsNullOrWhiteSpace($env:IN_TARGET)) { 'preview' }
          else { $env:IN_TARGET }

if ($target -eq 'production' -and $env:GIT_REF -ne 'refs/heads/main') {
  throw "deploy_target=production is only allowed on refs/heads/main (got $env:GIT_REF). Use 'preview'."
}

if ($target -eq 'production') {
  $url = (vercel deploy build/web --prod --yes --token=$env:VERCEL_TOKEN) | Select-Object -Last 1
} else {
  $url = (vercel deploy build/web --yes --token=$env:VERCEL_TOKEN) | Select-Object -Last 1
}

# Alias preview slot if slot is specified (e.g. p1.maze.ryankelly.dev)
if ($slot) {
  $aliasDomain = "p$slot.maze.ryankelly.dev"
  vercel alias set $url $aliasDomain --token=$env:VERCEL_TOKEN
  Write-Host "DEPLOY_ALIAS=https://$aliasDomain"
}

"url=$url" >> $env:GITHUB_OUTPUT
"target=$target" >> $env:GITHUB_OUTPUT

# A single machine-readable line so a caller reading the job log (an agent via the
# GitHub API, for instance) does not have to parse vercel's own chatter.
Write-Host "DEPLOY_URL=$url"
Write-Host "DEPLOY_TARGET=$target"

# Also in the step summary, which is one short API call rather than a whole log.
@(
  "### $target deploy",
  "",
  "| | |",
  "|---|---|",
  "| URL | <$url> |",
  "| Commit | ``$env:BUILD_COMMIT`` |",
  "| Build info | <$url/build-info.json> |"
) -join "`n" | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
