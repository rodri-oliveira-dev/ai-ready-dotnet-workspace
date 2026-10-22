#!/usr/bin/env bash

set -euo pipefail

readonly REQUIRED_FILES=(
  "docs/security/threat-model.md"
  "docs/security/security-baseline.md"
  ".github/dependabot.yml"
  ".github/workflows/ci.yml"
  ".devcontainer/devcontainer.json"
  ".codex/config.toml"
)

fail() {
  printf 'Security baseline validation failed: %s\n' "$1" >&2
  exit 1
}

for file in "${REQUIRED_FILES[@]}"; do
  [[ -s "$file" ]] || fail "$file is missing or empty"
done

while IFS= read -r action_reference; do
  [[ "$action_reference" =~ ^[^@]+@[0-9a-f]{40}$ ]] ||
    fail "GitHub Actions dependencies must use a full commit SHA: $action_reference"
done < <(sed -n 's/^[[:space:]]*uses:[[:space:]]*\([^[:space:]#]*\).*$/\1/p' .github/workflows/*.yml)

grep -Eq 'uses:[[:space:]]*actions/checkout@[0-9a-f]{40}' .github/workflows/ci.yml ||
  fail "CI must use a commit-pinned checkout action"
grep -Eq 'uses:[[:space:]]*actions/setup-dotnet@[0-9a-f]{40}' .github/workflows/ci.yml ||
  fail "CI must use a commit-pinned setup-dotnet action"
grep -Fq './scripts/validate-ai-context.sh' .github/workflows/ci.yml ||
  fail "CI must validate AI context"
grep -Fq './scripts/validate-security.sh' .github/workflows/ci.yml ||
  fail "CI must validate the security baseline"

grep -Fq 'permissions:' .github/workflows/ci.yml || fail "CI must declare token permissions"
grep -Fq '  contents: read' .github/workflows/ci.yml || fail "CI contents permission must be read-only"
grep -Fq 'persist-credentials: false' .github/workflows/ci.yml ||
  fail "checkout credentials must not persist"
if grep -Eq '(^|[[:space:]])(write-all|[a-z-]+:[[:space:]]*write)([[:space:]]|$)' \
  .github/workflows/*.yml; then
  fail "CI requests write permissions"
fi
if grep -Fq '${{ secrets.' .github/workflows/*.yml; then
  fail "the baseline CI must not consume repository secrets"
fi
if grep -Fq 'pull_request_target:' .github/workflows/*.yml; then
  fail "pull_request_target is not allowed in the baseline CI"
fi

grep -Eq '"image":[[:space:]]*"[^\"]+@sha256:[0-9a-f]{64}"' \
  .devcontainer/devcontainer.json || fail "the Dev Container image must be digest-pinned"
grep -Fq '"containerUser": "vscode"' .devcontainer/devcontainer.json ||
  fail "the Dev Container must run as the non-root vscode user"
grep -Fq '"--cap-drop=ALL"' .devcontainer/devcontainer.json ||
  fail "the Dev Container must drop Linux capabilities"
grep -Fq '"--security-opt=no-new-privileges:true"' .devcontainer/devcontainer.json ||
  fail "the Dev Container must disable privilege escalation"
if grep -Eq '"privileged"[[:space:]]*:[[:space:]]*true|docker\.sock|"(containerEnv|remoteEnv)"' \
  .devcontainer/devcontainer.json; then
  fail "the Dev Container enables a forbidden privilege, mount, or environment injection"
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
