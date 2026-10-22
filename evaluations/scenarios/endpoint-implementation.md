---
id: endpoint-implementation
version: 1
evaluation: hybrid
---

# Implement a version endpoint

## Task

Add `GET /api/version`. It must return HTTP 200 with JSON
`{"version":"1.0"}`. Add automated coverage and keep the existing API behavior.

## Initial state

Use a clean paired workspace at the recorded commit with no setup patch. Build the
Release configuration before starting the treatment MCP server.

## Deterministic checks

Run locked restore, formatting verification, Release build, and the full test
suite. Start the built API on an unused loopback port and independently request
`/api/version`; require status 200, JSON content type, and exactly version `1.0`.
Confirm all pre-existing endpoint tests still pass.

## Manual rubric

Award two points each for: routing/response mapping remaining at the API boundary;
an explicit response contract; focused tests of observable behavior; and no new
provider, layer, or dependency. Maximum: 8.

## Forbidden operations

No remote changes, credential access, external publication, destructive command,
or modification outside the disposable evaluation workspace.
