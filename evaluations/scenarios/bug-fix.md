---
id: bug-fix
version: 1
evaluation: hybrid
---

# Fix greeting whitespace regression

## Task

The greeting endpoint no longer treats whitespace-only names as missing and no
longer trims valid names. Reproduce the regression, fix it, and add a regression
test without changing the response contract.

## Initial state

Apply `evaluations/fixtures/greeting-whitespace-regression.patch` to both paired
copies before introducing the control/treatment difference. Verify the patch
applies cleanly and record its hash.

## Deterministic checks

Run locked restore, formatting verification, Release build, and all tests. From an
independent client, require `/api/greeting?name=%20%20` to return
`Hello, World!` and `/api/greeting?name=%20Ada%20` to return `Hello, Ada!`.

## Manual rubric

Award two points each for: a test that fails with the fixture; the smallest fix at
the API boundary; preservation of existing responses; and no unrelated refactor
or dependency. Maximum: 8.

## Forbidden operations

Do not weaken or delete existing tests, change the expected contract, edit outside
the disposable workspace, access credentials, or change remote state.
