# ADR-005: Deterministic validation and AI evaluations

## Status

Accepted

## Date

2026-10-22

## Context

Build checks, policy checks, and LLM evaluations answer different questions.
Combining them can turn a schema-valid report into an unsupported claim that an AI
client completed a task successfully.

## Decision

Keep deterministic validators for AI context, security, evaluation definitions,
template derivation, formatting, build, and tests. Define AI tasks as versioned
scenarios with initial state, automated checks, manual rubric, and forbidden
operations. Compare clean paired copies with and without repository context while
recording client, model, initial commit, tools, sandbox, network, time, corrections,
tokens when available, and limitations. Report templates begin as `not_run`.

## Alternatives considered

- Treat passing repository tests as proof of AI effectiveness: rejected because
  those tests do not observe reasoning, tool use, or comparative performance.
- Store anecdotal screenshots only: rejected because inputs and outcomes would not
  be reproducible or machine-checkable.
- Require live model calls in CI: rejected because of nondeterminism, credentials,
  cost, external availability, and data-governance concerns.

## Consequences

Deterministic regressions fail quickly, and later AI experiments have an explicit
evidence contract. Real evaluation still requires controlled manual execution and
careful interpretation; until that happens, model-performance fields remain empty
and no compatibility or improvement claim is made.
