# ADR-004: Reproducible AI development environments

## Status

Accepted

## Date

2026-10-22

## Context

AI-generated changes are difficult to assess when developer and CI environments
select different SDKs, dependencies, commands, or base images. A template must also
be safe to rename without leaving functional references behind.

## Decision

Pin the .NET feature band in `global.json`, centrally version packages, commit lock
files, pin the Dev Container by digest and Actions by commit SHA, and expose the
same restore/build/test flow through documentation, VS Code, the container, scripts,
and CI. Use a targeted, idempotent initializer with validated identifiers and a
state file. Keep provider integrations out of the production baseline.

## Alternatives considered

- Depend on each developer's installed latest SDK: rejected because results would drift.
- Use floating container and Action tags: rejected because upstream changes would
  alter the environment without repository review.
- Make the template a large configurable generator: rejected because the current
  requirements need deterministic renaming, not a new product or abstraction layer.

## Consequences

Inputs and commands are explicit and update diffs are reviewable. Immutable pins
require maintenance, lock files add churn, and validation currently relies on a
POSIX environment. Reproducibility reduces variation but does not guarantee
identical host kernels, feeds, caches, or external AI services.
