---
name: 'review-changes'
description: 'Review current changes for correctness, regressions, security, and missing tests.'
argument-hint: 'Optionally identify a comparison ref or review focus.'
agent: 'ask'
---

# Review changes

Review the current changes using the [shared repository guidance](../../AGENTS.md) and applicable path-specific instructions. Use this optional focus: `${input:focus:working tree or comparison ref, plus any review focus}`.

Remain read-only. Inspect the diff and enough surrounding code to validate each observation. Report actionable findings first, ordered by severity, with file and line references. Focus on defects, regressions, security issues, architecture boundary violations, and missing tests; omit stylistic preferences already enforced by tooling. If there are no findings, say so and identify residual validation gaps.
