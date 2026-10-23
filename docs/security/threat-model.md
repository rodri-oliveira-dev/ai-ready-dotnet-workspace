# Threat model for AI-assisted development

## Scope and trust boundaries

This model covers developers, Copilot or Codex clients, the local Dev Container,
the repository-scoped MCP server, package restore, and GitHub Actions. Source files,
issues, pull-request content, tool output, dependency metadata, generated text, and
external pages are untrusted inputs. The developer account, host OS, container
runtime, AI client, GitHub organization settings, package registries, and upstream
images remain external trust boundaries.

The protected assets are source history, credentials, personal and proprietary
data, the developer filesystem, CI tokens, build integrity, and the correctness of
reviewed changes. Repository instructions influence agent behavior but are not a
security boundary.

## Threats and controls

| Threat | Example and impact | Deterministic controls in this repository | Residual risk and required handling |
| --- | --- | --- | --- |
| Prompt injection | A source file, issue, dependency, web page, or tool result tells an agent to ignore policy, reveal data, or run a command. | MCP responses expose selected metadata only; its tools are read-only and closed-world. Codex command networking is off by default, and the Copilot reviewer has a read-only tool set. | Models can still be influenced by untrusted text. Treat retrieved content as data, inspect proposed actions, and use client/organization policy to restrict tools and external access. |
| Data exfiltration | An agent sends source, credentials, environment values, or local files to a network service. | Codex workspace-write command networking is disabled; MCP has no network or arbitrary-file tool; the baseline CI consumes no secrets. | AI clients, web search, extensions, connectors, and the host may use separate network paths. Configure those externally, minimize context, and never place sensitive data in prompts. |
| Unauthorized tool execution | A model invokes shell, connector, write, or publish tools beyond the user's intent. | Codex uses on-request elevation with a human reviewer; MCP exposes only two allowlisted read-only tools; the architecture reviewer exposes only read/search/MCP tools. | Commands allowed inside a writable sandbox can execute without a prompt. Review the task scope and diff; enforce organization policy for clients that can override repository configuration. |
| Credential exposure | Tokens appear in files, images, prompts, process environments, logs, or commits. | Local `.env*` files are ignored; the Dev Container embeds no environment secrets; checkout credentials do not persist; CI references no secrets; MCP rejects `.env`, `.git`, `.ssh`, and `.nuget` paths. | Host and editor extensions can still expose credentials. Use an external secret store, short-lived credentials, masked CI secrets, and rotate any value that is exposed. |
| Compromised dependencies | A mutable action, container image, SDK, or transitive NuGet package is replaced or compromised. | GitHub Actions and the Dev Container image are pinned by digest/SHA; NuGet sources are cleared to nuget.org; package versions are centralized and transitive graphs are locked; restore auditing is enabled; Dependabot proposes reviewed updates. | A pinned artifact can already be malicious, registries and update PRs remain external, and vulnerability data can be incomplete. Review provenance and update diffs; apply organization-level artifact policy where required. |
| Filesystem escape | Traversal, an absolute path, or a symlink reaches data outside the authorized workspace. | The MCP path policy rejects rooted paths, traversal, control characters, excessive paths, sensitive segments, reparse-point roots, and symlink/reparse-point components. Enumeration skips hidden, system, and reparse-point entries. | Filesystem checks can have time-of-check/time-of-use races, and the host/client may grant broader access than MCP. Do not expose a writable workspace to untrusted users; rely on OS/container isolation for hostile workloads. |
| Destructive commands or unreviewed changes | An agent deletes files, rewrites history, publishes, pushes, or changes remote resources. | Repository policy requires explicit authorization; Codex protects access outside its sandbox and keeps human approval for elevation; Git metadata is protected by supported Codex workspace sandboxes; CI has a read-only token. | Instructions cannot stop an agent or user with broad host access. Use branch protection, backups, required reviews, and managed approval/sandbox policy; inspect `git diff` before commit or push. |

## Abuse cases verified by tests

Negative tests reject empty, malformed, excessive, absolute, traversal, sensitive,
and symlink-escape paths. Integration tests enumerate the MCP surface and verify
that it consists only of read-only tools. Configuration tests check the tool
allowlist, approval and sandbox defaults, Dev Container restrictions, immutable
Action references, token scope, and absence of CI secret consumption. Malformed
project metadata produces a generic MCP error without an absolute workspace path.

## Review triggers

Review this model whenever a tool gains parameters or side effects, a connector or
network path is added, a secret becomes necessary, the workspace gains an
untrusted multi-user boundary, CI permissions change, or the container receives a
mount/capability. A write, command-execution, arbitrary-file, Docker socket, or
remote-publish capability requires a new threat analysis before it is enabled.
