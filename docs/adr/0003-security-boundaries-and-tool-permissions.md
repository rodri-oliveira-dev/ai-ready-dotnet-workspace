# ADR-003: Security boundaries and tool permissions

## Status

Accepted

## Date

2026-10-22

## Context

AI-assisted development processes untrusted repository and external content while
potentially operating tools with local or remote effects. Instructions alone are
not a security boundary.

## Decision

Apply least privilege at every repository-controlled surface. Codex defaults to
workspace-only writes, human-reviewed elevation, no login shell, and no command
network. The MCP allowlist contains two read-only tools; path handling rejects
absolute paths, traversal, sensitive segments, links/reparse points, oversized
input, and unsafe XML, and returns sanitized errors. CI has `contents: read`, no
secrets, no persisted credentials, and immutable action references. The Dev
Container runs non-root, drops capabilities, and prevents privilege escalation.

## Alternatives considered

- Rely on agent instructions and human review alone: rejected because they cannot
  technically constrain tool execution.
- Allow generic MCP filesystem or shell tools: rejected because current use cases
  need only selected metadata.
- Make the container privileged or mount the Docker socket: rejected because no
  baseline requirement justifies host-level access.

## Consequences

Common prompt-injection, path-escape, credential, and supply-chain exposure is
reduced and policy invariants are testable. Local race conditions, compromised
hosts, AI service traffic, extensions, and organization settings remain external
risks; higher-level managed policy is still required where enforcement matters.
