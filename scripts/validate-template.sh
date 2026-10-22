#!/usr/bin/env bash

set -euo pipefail

readonly DERIVED_SOLUTION_NAME="TemplateValidation"
readonly DERIVED_PROJECT_NAME="TemplateValidation.Api"

fail() {
  printf 'Template validation failed: %s\n' "$1" >&2
  exit 1
}

for dependency in dotnet git tar grep; do
  command -v "$dependency" >/dev/null 2>&1 || fail "required dependency not found: $dependency"
done

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

temporary_base="${TMPDIR:-/tmp}"
temporary_root="$(mktemp -d "${temporary_base%/}/ai-ready-template-validation.XXXXXX")"
derived_root="$temporary_root/derived"
status_before="$temporary_root/original-status.before"

cleanup() {
  case "$temporary_root" in
    "${temporary_base%/}"/ai-ready-template-validation.*)
      [[ -d "$temporary_root" ]] && rm -rf -- "$temporary_root"
      ;;
    *)
      printf 'Refusing to remove unexpected temporary path: %s\n' "$temporary_root" >&2
      ;;
  esac
}
trap cleanup EXIT

git status --porcelain=v1 --untracked-files=all > "$status_before"
mkdir -p "$derived_root"

git ls-files --cached --others --exclude-standard -z |
  tar --null --files-from=- -cf - |
  tar -xf - -C "$derived_root"

(
  cd "$derived_root"

  validation_solution_name="$DERIVED_SOLUTION_NAME"
  validation_project_name="$DERIVED_PROJECT_NAME"
  if [[ -f ".template-initialized" ]]; then
    validation_solution_name="$(sed -n 's/^solution_name=//p' .template-initialized)"
    validation_project_name="$(sed -n 's/^project_name=//p' .template-initialized)"
  fi

  ./scripts/initialize-template.sh \
    --solution-name "$validation_solution_name" \
    --project-name "$validation_project_name"

  # A second identical run must be a no-op and succeed.
  ./scripts/initialize-template.sh \
    --solution-name "$validation_solution_name" \
    --project-name "$validation_project_name"

  if ./scripts/initialize-template.sh \
    --solution-name "ConflictingValidation" \
    --project-name "ConflictingValidation.Api" >/dev/null 2>&1; then
    fail "reinitialization with conflicting names must fail"
  fi

  [[ -f "${validation_solution_name}.slnx" ]] || fail "renamed solution is missing"
  [[ -f "src/${validation_project_name}/${validation_project_name}.csproj" ]] ||
    fail "renamed application project is missing"
  [[ -f "tests/${validation_project_name}.Tests/${validation_project_name}.Tests.csproj" ]] ||
    fail "renamed test project is missing"
  [[ ! -e "AiReadyWorkspace.slnx" ]] || fail "original solution remains"
  [[ ! -e "src/SampleApi" ]] || fail "original application directory remains"
  [[ ! -e "tests/SampleApi.Tests" ]] || fail "original test directory remains"

  readonly -a RESIDUAL_SCAN_TARGETS=(
    "src"
    "tests"
    ".agents"
    ".codex"
    ".devcontainer"
    ".github"
    ".vscode"
    "docs"
    "evaluations"
    "AGENTS.md"
    "CONTRIBUTING.md"
    "README.md"
    "README.pt-BR.md"
    "scripts/verify-environment.sh"
  )

  if grep -R -n -F -e "AiReadyWorkspace" -e "SampleApi" \
    "${RESIDUAL_SCAN_TARGETS[@]}"; then
    fail "derived repository contains residual template names"
  fi

  dotnet restore "${validation_solution_name}.slnx" --locked-mode
  dotnet format "${validation_solution_name}.slnx" --verify-no-changes --no-restore
  dotnet build "${validation_solution_name}.slnx" --configuration Release --no-restore
  dotnet test "${validation_solution_name}.slnx" --configuration Release --no-build
)

status_after="$temporary_root/original-status.after"
git status --porcelain=v1 --untracked-files=all > "$status_after"
cmp -s "$status_before" "$status_after" ||
  fail "validation changed the original repository"

printf 'Template initialization and derived repository validation passed.\n'
