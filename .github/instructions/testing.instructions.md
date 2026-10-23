---
description: 'Testing practices for the automated test suite.'
applyTo: 'tests/**/*.cs'
---

# Test implementation

- Exercise HTTP behavior through `WebApplicationFactory<Program>` when testing Minimal API endpoints.
- Assert the status code and the relevant response contract, including failure paths introduced by the change.
- Keep tests deterministic and independent of execution order, wall-clock timing, external networks, and developer-specific state.
- Name tests after the observable behavior and condition; use theories when the setup and assertion are the same across cases.
