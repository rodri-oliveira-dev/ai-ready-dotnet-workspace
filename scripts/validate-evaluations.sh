#!/usr/bin/env bash

set -euo pipefail

fail() {
  printf 'Evaluation validation failed: %s\n' "$1" >&2
  exit 1
}

readonly -a SCENARIO_IDS=(
  "endpoint-implementation"
  "bug-fix"
  "test-creation"
  "architecture-review"
  "instruction-violation"
  "unauthorized-access"
)

readonly -a REQUIRED_SECTIONS=(
  "## Task"
  "## Initial state"
  "## Deterministic checks"
  "## Manual rubric"
  "## Forbidden operations"
)

[[ -s "evaluations/README.md" ]] || fail "evaluations/README.md is missing"
[[ -s "evaluations/protocol.md" ]] || fail "evaluations/protocol.md is missing"
[[ -s "evaluations/report-template.json" ]] || fail "report template is missing"
[[ -s "evaluations/fixtures/greeting-whitespace-regression.patch" ]] ||
  fail "bug fixture is missing"
[[ -s "evaluations/fixtures/untrusted-instructions.txt" ]] ||
  fail "prompt-injection fixture is missing"

for scenario_id in "${SCENARIO_IDS[@]}"; do
  scenario="evaluations/scenarios/${scenario_id}.md"
  [[ -s "$scenario" ]] || fail "scenario is missing: $scenario_id"
  grep -Fqx "id: ${scenario_id}" "$scenario" || fail "$scenario has an invalid id"
  grep -Eq '^version: [1-9][0-9]*$' "$scenario" || fail "$scenario needs a version"
  grep -Eq '^evaluation: (automated|hybrid|manual)$' "$scenario" ||
    fail "$scenario has an invalid evaluation mode"
  for section in "${REQUIRED_SECTIONS[@]}"; do
    grep -Fqx "$section" "$scenario" || fail "$scenario is missing section: $section"
  done
done

for status in not_run success failure; do
  grep -Fq "\"$status\"" evaluations/README.md ||
    fail "evaluation status is not documented: $status"
done

grep -Fq '"status": "not_run"' evaluations/report-template.json ||
  fail "report templates must start as not_run"
grep -Fq '"inputTokens": null' evaluations/report-template.json ||
  fail "unmeasured token consumption must remain null"
grep -Fq '"durationSeconds": null' evaluations/report-template.json ||
  fail "unmeasured duration must remain null"
grep -Fq '"initialCommit": null' evaluations/report-template.json ||
  fail "report templates must not claim an initial commit"

printf 'Evaluation scenarios and report contract are structurally valid.\n'
