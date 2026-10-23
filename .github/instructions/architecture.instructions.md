---
description: 'Architecture boundaries for production source changes.'
applyTo: 'src/**/*.cs'
---

# Production architecture

- Keep HTTP concerns at the API boundary and map them to explicit request or response contracts.
- Keep business decisions free of web-host, persistence, and vendor-specific dependencies.
- Put external I/O behind a narrow interface when a second implementation or isolated test requires that boundary.
- Preserve the template's provider-neutral baseline unless the requested feature explicitly selects a provider.
