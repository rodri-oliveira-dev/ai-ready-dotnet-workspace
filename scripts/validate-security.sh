#!/usr/bin/env bash

set -euo pipefail

readonly REQUIRED_FILES=(
  "docs/security/threat-model.md"
  "docs/security/security-baseline.md"
  ".github/dependabot.yml"
  ".github/workflows/ci.yml"
  ".devcontainer/devcontainer.json"
  ".codex/config.toml"
  "scripts/validate-evaluations.sh"
  "scripts/validate-json.cs"
  "scripts/validate-template.sh"
)

fail() {
  printf 'Security baseline validation failed: %s\n' "$1" >&2
  exit 1
}

for file in "${REQUIRED_FILES[@]}"; do
  [[ -s "$file" ]] || fail "$file is missing or empty"
done

command -v dotnet >/dev/null 2>&1 || fail "required dependency not found: dotnet"

shopt -s nullglob
readonly -a WORKFLOW_FILES=(.github/workflows/*.yml .github/workflows/*.yaml)
((${#WORKFLOW_FILES[@]} > 0)) || fail "no GitHub Actions workflows were found"

while IFS= read -r action_reference; do
  [[ "$action_reference" =~ ^[^@]+@[0-9a-f]{40}$ ]] ||
    fail "GitHub Actions dependencies must use a full commit SHA: $action_reference"
done < <(sed -n 's/^[[:space:]]*-\{0,1\}[[:space:]]*uses:[[:space:]]*\([^[:space:]#]*\).*$/\1/p' \
  "${WORKFLOW_FILES[@]}")

grep -Eq 'uses:[[:space:]]*actions/checkout@[0-9a-f]{40}' .github/workflows/ci.yml ||
  fail "CI must use a commit-pinned checkout action"
grep -Eq 'uses:[[:space:]]*actions/setup-dotnet@[0-9a-f]{40}' .github/workflows/ci.yml ||
  fail "CI must use a commit-pinned setup-dotnet action"
grep -Fq './scripts/validate-ai-context.sh' .github/workflows/ci.yml ||
  fail "CI must validate AI context"
grep -Fq './scripts/validate-security.sh' .github/workflows/ci.yml ||
  fail "CI must validate the security baseline"
grep -Fq './scripts/validate-evaluations.sh' .github/workflows/ci.yml ||
  fail "CI must validate evaluation definitions"
grep -Fq './scripts/validate-template.sh' .github/workflows/ci.yml ||
  fail "CI must validate template initialization"

grep -Fq 'permissions:' .github/workflows/ci.yml || fail "CI must declare token permissions"
grep -Fq '  contents: read' .github/workflows/ci.yml || fail "CI contents permission must be read-only"
grep -Fq 'persist-credentials: false' .github/workflows/ci.yml ||
  fail "checkout credentials must not persist"
if grep -Eq '(^|[[:space:]])(write-all|[a-z-]+:[[:space:]]*write)([[:space:]]|$)' \
  "${WORKFLOW_FILES[@]}"; then
  fail "CI requests write permissions"
fi
if grep -Fq '${{ secrets.' "${WORKFLOW_FILES[@]}"; then
  fail "the baseline CI must not consume repository secrets"
fi
if grep -Fq 'pull_request_target:' "${WORKFLOW_FILES[@]}"; then
  fail "pull_request_target is not allowed in the baseline CI"
fi

grep -Eq '"image":[[:space:]]*"[^\"]+@sha256:[0-9a-f]{64}"' \
  .devcontainer/devcontainer.json || fail "the Dev Container image must be digest-pinned"
grep -Fq '"containerUser": "vscode"' .devcontainer/devcontainer.json ||
  fail "the Dev Container must run as the non-root vscode user"
if ! dotnet run --file scripts/validate-json.cs \
  --property:RestorePackagesWithLockFile=false -- \
  .devcontainer/devcontainer.json devcontainer >/dev/null; then
  fail "the Dev Container arguments, mounts, or environment violate the isolation policy"
fi

grep -Fq 'approval_policy = "on-request"' .codex/config.toml ||
  fail "Codex must request approval for sandbox elevation"
grep -Fq 'approvals_reviewer = "user"' .codex/config.toml ||
  fail "Codex approvals must remain human-reviewed"
grep -Fq 'sandbox_mode = "workspace-write"' .codex/config.toml ||
  fail "Codex must limit writes to the workspace"
grep -Fq 'network_access = false' .codex/config.toml ||
  fail "Codex command network access must default to disabled"
grep -Fq 'allow_login_shell = false' .codex/config.toml ||
  fail "Codex login shells must be disabled"

grep -Fq 'enabled_tools = ["list_dotnet_projects", "get_architecture_rules"]' \
  .codex/config.toml || fail "Codex MCP tools must remain allowlisted"
if grep -Eq 'danger-full-access|approval_policy[[:space:]]*=[[:space:]]*"never"' \
  .codex/config.toml; then
  fail "Codex broad access or suppressed approvals are forbidden"
fi

printf 'Security baseline files are structurally valid.\n'
