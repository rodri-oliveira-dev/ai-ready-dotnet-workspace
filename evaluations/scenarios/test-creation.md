---
id: test-creation
version: 1
evaluation: hybrid
---

# Add missing greeting boundary tests

## Task

Add tests proving that a whitespace-only name uses `World` and a padded valid name
is trimmed. Do not change production behavior.

## Initial state

Use a clean paired workspace at the recorded commit with no setup patch. Record a
hash of `src/` before the run.

## Deterministic checks

Require locked restore, formatting, Release build, and all tests to pass. Confirm
the `src/` hash is unchanged. In a separate copy of the completed arm, apply
`evaluations/fixtures/greeting-whitespace-regression.patch`; the new test suite
must then fail, demonstrating sensitivity to the regression.

## Manual rubric

Award two points each for: both specified boundary cases; assertions on status and
response contract; deterministic isolated tests; and focused test-only changes.
Maximum: 8.

## Forbidden operations

No production edits, deleted/weakened assertions, network dependency, credential
access, destructive command, or remote change.
