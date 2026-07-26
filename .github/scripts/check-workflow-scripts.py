#!/usr/bin/env python3
"""Verify every workflow/action step that calls a script file can actually run it.

Moving step bodies out of YAML into .ps1/.sh files trades one failure mode for another: the
script no longer sees `${{ ... }}` interpolation, so every value it needs must be handed over
through `env:`. Miss one and the step does not fail loudly — it reads an empty string and does
something subtly wrong. That happened three times while doing the extraction (BUNDLE_DIR,
GODOT_EXE, IN_TARGET), which is why this is a committed check rather than a one-off.

Checks, for each step whose `run:` invokes a file under .github/:
  1. the script exists at the path the step names
  2. every environment variable the script reads is provided by the step, the job, the
     workflow, or the runner itself

Run from the repository root:  python3 .github/scripts/check-workflow-scripts.py
Exits non-zero if anything is unsatisfied, so it works as a CI gate.
"""

import glob
import os
import re
import sys

import yaml

# Supplied by the runner or by Actions itself, so a step need not declare them.
RUNNER_PROVIDED = {
    "GITHUB_OUTPUT",
    "GITHUB_STEP_SUMMARY",
    "GITHUB_ENV",
    "GITHUB_PATH",
    "GITHUB_WORKSPACE",
    "GITHUB_ACTION_PATH",
    "GITHUB_REPOSITORY",
    "GITHUB_SHA",
    "GITHUB_REF",
    "GITHUB_RUN_ID",
    "GITHUB_RUN_ATTEMPT",
    "GITHUB_EVENT_NAME",
    "RUNNER_TEMP",
    "RUNNER_OS",
    "HOME",
    "PATH",
}


def steps_with_env(path):
    """Yield (step, env visible to it from workflow/job level) for one YAML file."""
    doc = yaml.safe_load(open(path))
    workflow_env = doc.get("env") or {}

    if "runs" in doc:  # composite action
        for step in doc["runs"].get("steps") or []:
            yield step, dict(workflow_env)
        return

    for job in (doc.get("jobs") or {}).values():
        inherited = dict(workflow_env)
        inherited.update(job.get("env") or {})
        for step in job.get("steps") or []:
            yield step, inherited


def invoked_script(run, containing_yaml):
    """The script path a `run:` body calls, or None if it is an ordinary inline command."""
    m = re.search(r"\$env:GITHUB_ACTION_PATH/(\S+?)\"", run)
    if m:  # relative to the composite action's own directory
        return os.path.join(os.path.dirname(containing_yaml), m.group(1))
    m = re.search(r"\$env:GITHUB_WORKSPACE/(\S+?)\"", run)
    if m:
        return m.group(1)
    m = re.search(r"\bbash (\.github/\S+)", run)
    if m:
        return m.group(1)
    return None


def env_vars_read(path):
    """Environment variables a script depends on, excluding ones it defines itself."""
    text = open(path).read()

    if path.endswith(".ps1"):
        return set(re.findall(r"\$env:([A-Za-z_][A-Za-z0-9_]*)", text))

    assigned = set(re.findall(r"^\s*([A-Za-z_][A-Za-z0-9_]*)=", text, re.M))
    assigned |= set(re.findall(r"\bfor\s+([A-Za-z_][A-Za-z0-9_]*)\s+in\b", text))
    used = set(re.findall(r"\$\{?([A-Z_][A-Z0-9_]*)\}?", text))
    return used - assigned


def main():
    targets = [".github/actions/export-web/action.yml"] + sorted(glob.glob(".github/workflows/*.yml"))
    problems = 0
    checked = 0

    for yml in targets:
        for step, inherited in steps_with_env(yml):
            run = step.get("run") or ""
            script = invoked_script(run, yml)
            if not script:
                continue

            name = step.get("name", "(unnamed)")
            if not os.path.exists(script):
                print(f"  MISSING  {script}  (step: {name} in {yml})")
                problems += 1
                continue

            provided = set(inherited) | set(step.get("env") or {}) | RUNNER_PROVIDED
            needed = env_vars_read(script)
            unsatisfied = sorted(needed - provided)
            checked += 1

            if unsatisfied:
                print(f"  FAIL     {script}  (step: {name} in {yml})")
                print(f"           not provided by the step: {', '.join(unsatisfied)}")
                problems += 1
            else:
                print(f"  ok       {script}  ({len(needed)} env refs satisfied)")

    print(f"\n{checked} script invocation(s) checked, {problems} problem(s)")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
