#!/usr/bin/env bash

set -euo pipefail

readonly REQUIRED_FILES=(
  "AGENTS.md"
  ".github/copilot-instructions.md"
  ".github/instructions/dotnet.instructions.md"
  ".github/instructions/architecture.instructions.md"
  ".github/instructions/testing.instructions.md"
  ".github/prompts/implement-feature.prompt.md"
  ".github/prompts/review-changes.prompt.md"
)

fail() {
  printf 'AI context validation failed: %s\n' "$1" >&2
  exit 1
}

for file in "${REQUIRED_FILES[@]}"; do
  [[ -s "$file" ]] || fail "$file is missing or empty"
done

validate_frontmatter() {
  local file="$1"
  local closing_line

  [[ "$(sed -n '1p' "$file")" == "---" ]] || fail "$file must start with YAML frontmatter"
  closing_line="$(awk 'NR > 1 && $0 == "---" { print NR; exit }' "$file")"
  [[ -n "$closing_line" ]] || fail "$file has no closing YAML frontmatter delimiter"
  [[ "$closing_line" -gt 2 ]] || fail "$file has empty YAML frontmatter"
}

validate_apply_to() {
  local file="$1"
  local expected="$2"
  local actual
  local pattern
  local repository_file
  local matched=false
  local -a patterns
  local -a repository_files

  validate_frontmatter "$file"
  actual="$(sed -n "2,/^---$/s/^applyTo: ['\"]\(.*\)['\"]$/\1/p" "$file")"
  [[ "$actual" == "$expected" ]] || fail "$file has invalid applyTo; expected $expected"

  IFS=',' read -r -a patterns <<< "$actual"
  while IFS= read -r repository_file; do
    repository_files+=("$repository_file")
  done < <(git ls-files --cached --others --exclude-standard)
  for pattern in "${patterns[@]}"; do
    for repository_file in "${repository_files[@]}"; do
      if [[ "$repository_file" == $pattern ]]; then
        matched=true
        break 2
      fi
    done
  done
  [[ "$matched" == true ]] || fail "$file applyTo does not match any repository file"
}

validate_apply_to ".github/instructions/dotnet.instructions.md" "**/*.cs,**/*.csproj,**/*.props,**/*.targets,**/*.slnx"
validate_apply_to ".github/instructions/architecture.instructions.md" "src/**/*.cs"
validate_apply_to ".github/instructions/testing.instructions.md" "tests/**/*.cs"

for file in .github/prompts/*.prompt.md; do
  validate_frontmatter "$file"
  sed -n '2,/^---$/p' "$file" | grep -Eq '^description: ' || fail "$file needs a description"
  sed -n '2,/^---$/p' "$file" | grep -Eq "^agent: '(agent|ask|plan)'$" || fail "$file has an unsupported agent"
done

if [[ -f .codex/config.toml ]]; then
  if grep -Eq '(^|[[:space:]])(danger-full-access|never)([[:space:]]|$)' .codex/config.toml; then
    fail ".codex/config.toml broadens permissions or suppresses approvals"
  fi
fi

duplicate_rule="$(sed -n 's/^- /- /p' AGENTS.md .github/copilot-instructions.md .github/instructions/*.instructions.md | sort | uniq -d | head -n 1)"
[[ -z "$duplicate_rule" ]] || fail "duplicated instruction: $duplicate_rule"

for file in "${REQUIRED_FILES[@]}"; do
  fence_count="$(grep -c '^```' "$file" || true)"
  (( fence_count % 2 == 0 )) || fail "$file has an unclosed fenced code block"
done

printf 'AI context files are structurally valid.\n'
