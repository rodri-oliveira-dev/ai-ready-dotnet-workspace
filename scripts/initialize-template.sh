#!/usr/bin/env bash

set -euo pipefail

readonly ORIGINAL_SOLUTION_NAME="AiReadyWorkspace"
readonly ORIGINAL_PROJECT_NAME="SampleApi"
readonly STATE_FILE=".template-initialized"

fail() {
  printf 'Template initialization failed: %s\n' "$1" >&2
  exit 1
}

usage() {
  cat <<'EOF'
Usage: ./scripts/initialize-template.sh --solution-name <Name> --project-name <Name.Api>

Names must be valid dotted .NET identifiers. The application test project is
named <project-name>.Tests. Workspace.McpServer remains provider-neutral tooling.
EOF
}

solution_name=""
project_name=""

for dependency in grep sed tr mv rm; do
  command -v "$dependency" >/dev/null 2>&1 || fail "required dependency not found: $dependency"
done

while (($# > 0)); do
  case "$1" in
    --solution-name)
      (($# >= 2)) || fail "--solution-name requires a value"
      solution_name="$2"
      shift 2
      ;;
    --project-name)
      (($# >= 2)) || fail "--project-name requires a value"
      project_name="$2"
      shift 2
      ;;
    --help|-h)
      usage
      exit 0
      ;;
    *)
      fail "unknown argument: $1"
      ;;
  esac
done

[[ "$solution_name" =~ ^[A-Z][A-Za-z0-9]*$ ]] ||
  fail "solution name must be a PascalCase identifier without dots"
[[ "$project_name" =~ ^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)*$ ]] ||
  fail "project name must contain valid PascalCase namespace segments"
[[ "$solution_name" != "$ORIGINAL_SOLUTION_NAME" ]] ||
  fail "solution name must differ from the template name"
[[ "$project_name" != "$ORIGINAL_PROJECT_NAME" ]] ||
  fail "project name must differ from the template name"

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

test_project_name="${project_name}.Tests"
project_name_lower="$(printf '%s' "$project_name" | tr '[:upper:]' '[:lower:]')"
target_solution="${solution_name}.slnx"
target_project_directory="src/${project_name}"
target_test_directory="tests/${test_project_name}"

if [[ -f "$STATE_FILE" ]]; then
  stored_solution="$(sed -n 's/^solution_name=//p' "$STATE_FILE")"
  stored_project="$(sed -n 's/^project_name=//p' "$STATE_FILE")"
  [[ "$stored_solution" == "$solution_name" && "$stored_project" == "$project_name" ]] ||
    fail "the template was already initialized with different names"
  [[ -f "$target_solution" ]] || fail "the initialized solution is missing"
  [[ -f "$target_project_directory/${project_name}.csproj" ]] ||
    fail "the initialized application project is missing"
  [[ -f "$target_test_directory/${test_project_name}.csproj" ]] ||
    fail "the initialized test project is missing"
  printf 'Template is already initialized as %s with project %s.\n' \
    "$solution_name" "$project_name"
  exit 0
fi

[[ -f "${ORIGINAL_SOLUTION_NAME}.slnx" ]] || fail "template solution was not found"
[[ -d "src/${ORIGINAL_PROJECT_NAME}" ]] || fail "template application project was not found"
[[ -d "tests/${ORIGINAL_PROJECT_NAME}.Tests" ]] || fail "template test project was not found"
[[ ! -e "$target_solution" ]] || fail "$target_solution already exists"
[[ ! -e "$target_project_directory" ]] || fail "$target_project_directory already exists"
[[ ! -e "$target_test_directory" ]] || fail "$target_test_directory already exists"

replace_literal() {
  local file="$1"
  local old_value="$2"
  local new_value="$3"

  [[ -f "$file" ]] || fail "expected file is missing: $file"
  grep -Fq "$old_value" "$file" ||
    fail "expected value '$old_value' is missing from $file"
  sed -i.bak "s/${old_value}/${new_value}/g" "$file"
  rm -- "${file}.bak"
}

readonly -a SOLUTION_REFERENCE_FILES=(
  "AGENTS.md"
  "README.md"
  "README.pt-BR.md"
  "CONTRIBUTING.md"
  ".agents/skills/validate-change/SKILL.md"
  ".devcontainer/devcontainer.json"
  ".devcontainer/post-create.sh"
  ".github/workflows/ci.yml"
  ".vscode/settings.json"
  ".vscode/tasks.json"
  "scripts/verify-environment.sh"
  "tests/Workspace.McpServer.Tests/ConfigurationTests.cs"
  "tests/Workspace.McpServer.Tests/McpServerIntegrationTests.cs"
)

readonly -a PROJECT_REFERENCE_FILES=(
  "${ORIGINAL_SOLUTION_NAME}.slnx"
  "README.md"
  "README.pt-BR.md"
  "docs/architecture.md"
  "src/${ORIGINAL_PROJECT_NAME}/${ORIGINAL_PROJECT_NAME}.csproj"
  "src/${ORIGINAL_PROJECT_NAME}/Program.cs"
  "src/${ORIGINAL_PROJECT_NAME}/ApiResponses.cs"
  "tests/${ORIGINAL_PROJECT_NAME}.Tests/${ORIGINAL_PROJECT_NAME}.Tests.csproj"
  "tests/${ORIGINAL_PROJECT_NAME}.Tests/ApiEndpointsTests.cs"
  "evaluations/fixtures/greeting-whitespace-regression.patch"
)

for file in "${SOLUTION_REFERENCE_FILES[@]}"; do
  [[ -f "$file" ]] || fail "expected file is missing: $file"
  grep -Fq "$ORIGINAL_SOLUTION_NAME" "$file" ||
    fail "expected value '$ORIGINAL_SOLUTION_NAME' is missing from $file"
done

for file in "${PROJECT_REFERENCE_FILES[@]}"; do
  [[ -f "$file" ]] || fail "expected file is missing: $file"
  grep -Fq "$ORIGINAL_PROJECT_NAME" "$file" ||
    fail "expected value '$ORIGINAL_PROJECT_NAME' is missing from $file"
done

[[ -f "tests/${ORIGINAL_PROJECT_NAME}.Tests/packages.lock.json" ]] ||
  fail "test project lockfile is missing"
grep -Fq "sampleapi" "tests/${ORIGINAL_PROJECT_NAME}.Tests/packages.lock.json" ||
  fail "test project lockfile does not contain the expected project reference"

for file in "${SOLUTION_REFERENCE_FILES[@]}"; do
  replace_literal "$file" "$ORIGINAL_SOLUTION_NAME" "$solution_name"
done

for file in "${PROJECT_REFERENCE_FILES[@]}"; do
  replace_literal "$file" "$ORIGINAL_PROJECT_NAME" "$project_name"
done

replace_literal \
  "tests/${ORIGINAL_PROJECT_NAME}.Tests/packages.lock.json" \
  "sampleapi" \
  "$project_name_lower"

mv -- \
  "src/${ORIGINAL_PROJECT_NAME}/${ORIGINAL_PROJECT_NAME}.csproj" \
  "src/${ORIGINAL_PROJECT_NAME}/${project_name}.csproj"
mv -- "src/${ORIGINAL_PROJECT_NAME}" "$target_project_directory"

mv -- \
  "tests/${ORIGINAL_PROJECT_NAME}.Tests/${ORIGINAL_PROJECT_NAME}.Tests.csproj" \
  "tests/${ORIGINAL_PROJECT_NAME}.Tests/${test_project_name}.csproj"
mv -- "tests/${ORIGINAL_PROJECT_NAME}.Tests" "$target_test_directory"
mv -- "${ORIGINAL_SOLUTION_NAME}.slnx" "$target_solution"

printf 'solution_name=%s\nproject_name=%s\n' "$solution_name" "$project_name" > "$STATE_FILE"

printf 'Initialized solution %s with application %s and tests %s.\n' \
  "$solution_name" "$project_name" "$test_project_name"
