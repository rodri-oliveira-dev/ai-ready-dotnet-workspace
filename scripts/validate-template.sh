#!/usr/bin/env bash

set -euo pipefail

readonly DERIVED_SOLUTION_NAME="TemplateValidation"
readonly DERIVED_PROJECT_NAME="TemplateValidation.Api"
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

fail() {
  printf 'Template validation failed: %s\n' "$1" >&2
  exit 1
}

assert_no_residual_template_names() {
  local selected_solution_name="$1"
  local selected_project_name="$2"
  local file
  local residual_found=false
  local -a scan_targets=("${RESIDUAL_SCAN_TARGETS[@]}")

  [[ -f "${selected_solution_name}.code-workspace" ]] &&
    scan_targets+=("${selected_solution_name}.code-workspace")

  while IFS= read -r -d '' file; do
    if awk \
      -v solution="$selected_solution_name" \
      -v project="$selected_project_name" \
      -v filename="$file" '
        function remove_literal(text, literal, position) {
          while ((position = index(text, literal)) > 0) {
            text = substr(text, 1, position - 1) substr(text, position + length(literal))
          }
          return text
        }
        {
          remaining = remove_literal($0, solution)
          remaining = remove_literal(remaining, project)
          if (index(remaining, "AiReadyWorkspace") || index(remaining, "SampleApi")) {
            print filename ":" NR ":" $0
            found = 1
          }
        }
        END { exit found ? 0 : 1 }
      ' "$file"; then
      residual_found=true
    fi
  done < <(grep -RIlZ -F -e "AiReadyWorkspace" -e "SampleApi" \
    "${scan_targets[@]}" || true)

  [[ "$residual_found" == false ]] ||
    fail "derived repository contains residual template names"
}

for dependency in dotnet git tar grep awk; do
  command -v "$dependency" >/dev/null 2>&1 || fail "required dependency not found: $dependency"
done

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

temporary_base="${TMPDIR:-/tmp}"
temporary_root="$(mktemp -d "${temporary_base%/}/ai-ready-template-validation.XXXXXX")"
derived_root="$temporary_root/derived"
collision_root="$temporary_root/collision"
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
mkdir -p "$derived_root" "$collision_root"

git ls-files --cached --others --exclude-standard -z |
  tar --null --files-from=- -cf - |
  tar -xf - -C "$derived_root"

git ls-files --cached --others --exclude-standard -z |
  tar --null --files-from=- -cf - |
  tar -xf - -C "$collision_root"

(
  cd "$collision_root"

  ./scripts/initialize-template.sh \
    --solution-name "SampleApiWorkspace" \
    --project-name "SampleApiV2"

  [[ -f "SampleApiWorkspace.slnx" ]] || fail "collision-case solution is missing"
  [[ -f "SampleApiWorkspace.code-workspace" ]] ||
    fail "collision-case VS Code workspace is missing"
  grep -Fq "SampleApiWorkspace.code-workspace" README.md ||
    fail "collision-case README does not reference the renamed VS Code workspace"
  grep -Fq "SampleApiWorkspace.slnx" README.md ||
    fail "project replacement corrupted the solution name"
  if grep -R -n -F "SampleApiV2Workspace.slnx" \
    README.md README.pt-BR.md CONTRIBUTING.md .agents .devcontainer .github .vscode; then
    fail "project replacement rewrote part of the solution name"
  fi

  assert_no_residual_template_names "SampleApiWorkspace" "SampleApiV2"
)

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
  [[ -f "${validation_solution_name}.code-workspace" ]] ||
    fail "renamed VS Code workspace is missing"
  grep -Fq "${validation_solution_name}.code-workspace" README.md ||
    fail "README does not reference the renamed VS Code workspace"
  grep -Fq "${validation_solution_name}.code-workspace" README.pt-BR.md ||
    fail "Portuguese README does not reference the renamed VS Code workspace"
  [[ -f "src/${validation_project_name}/${validation_project_name}.csproj" ]] ||
    fail "renamed application project is missing"
  [[ -f "tests/${validation_project_name}.Tests/${validation_project_name}.Tests.csproj" ]] ||
    fail "renamed test project is missing"
  [[ ! -e "AiReadyWorkspace.slnx" ]] || fail "original solution remains"
  [[ ! -e "ai-ready-dotnet-workspace.code-workspace" ]] ||
    fail "original VS Code workspace remains"
  [[ ! -e "src/SampleApi" ]] || fail "original application directory remains"
  [[ ! -e "tests/SampleApi.Tests" ]] || fail "original test directory remains"

  assert_no_residual_template_names "$validation_solution_name" "$validation_project_name"

  dotnet tool restore
  dotnet tool run adr-guard -- --version
  dotnet tool run adr-guard -- check
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
