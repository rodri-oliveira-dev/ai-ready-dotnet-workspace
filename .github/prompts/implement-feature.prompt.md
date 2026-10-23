---
name: 'implement-feature'
description: 'Implement a scoped feature and verify it against repository guidance.'
argument-hint: 'Describe the behavior, constraints, and acceptance criteria.'
agent: 'agent'
---

# Implement a feature

Implement this feature: `${input:feature:describe the requested behavior and acceptance criteria}`.

Use the [shared repository guidance](../../AGENTS.md) and the instruction files that apply to each changed path. Inspect the current implementation and tests, state any material assumption, then make the smallest coherent change.

Run the relevant trusted checks. Finish with the files changed, observable behavior, checks actually run, and any remaining risk. Do not create a commit or change remote state unless the user explicitly requests it.
