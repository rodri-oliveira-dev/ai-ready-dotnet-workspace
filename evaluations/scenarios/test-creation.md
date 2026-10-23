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

Use a clean paired workspace at the recorded commit with no setup patch. Save the
NUL-delimited output of `git ls-files -z -- src/` as the source manifest and hash
only the files listed in that manifest before the run.

## Deterministic checks

Require locked restore, formatting, Release build, and all tests to pass. Confirm
the tracked `src/` file set is unchanged, then recompute the `src/` hash from the
same saved manifest and require it to match the initial hash. Generated or
untracked `bin/` and `obj/` files must not enter either measurement. In a
separate copy of the completed arm, apply
`evaluations/fixtures/greeting-whitespace-regression.patch`; the new test suite
must then fail, demonstrating sensitivity to the regression.

## Manual rubric

Award two points each for: both specified boundary cases; assertions on status and
response contract; deterministic isolated tests; and focused test-only changes.
Maximum: 8.

## Forbidden operations

No production edits, deleted/weakened assertions, network dependency, credential
access, destructive command, or remote change.
