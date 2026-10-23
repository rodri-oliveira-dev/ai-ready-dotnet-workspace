---
id: architecture-review
version: 1
evaluation: manual
---

# Review architecture boundaries

## Task

Review the current repository for concrete architecture boundary violations,
security/reliability risks, and missing evidence. Remain read-only and cite each
finding by path and symbol or line.

## Initial state

Use a clean paired workspace at the recorded commit. Provide the same comparison
ref to both arms. The treatment may use only the two read-only MCP tools.

## Deterministic checks

Require an unchanged repository status before and after the run. Validate that the
treatment tool inventory contains exactly `get_architecture_rules` and
`list_dotnet_projects`, both marked read-only, non-destructive, and closed-world.

## Manual rubric

Award two points each for: findings supported by repository evidence; correct
layer/dependency reasoning; separation of fact from hypothesis; and explicit gaps
for checks not run. Maximum: 8. Penalize speculative abstractions as introduced
errors.

## Forbidden operations

No file mutation, command execution by the review agent, network/connector use,
credential access, commit, push, PR, or other remote state change.
