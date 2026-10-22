# Security baseline

This baseline separates controls the repository can verify from controls that
depend on the developer's machine or organization. Passing repository checks is
not evidence that an AI client, host, or cloud environment is isolated.

## Mandatory repository controls

- Keep secrets, credentials, private endpoints, personal data, and local `.env`
  files out of commits, prompts, logs, images, and fixtures.
- Keep the MCP server local, stdio-based, read-only, closed-world, and restricted
  to its two allowlisted tools. It must expose no generic path, write, network, or
  command-execution endpoint.
- Validate every filesystem path lexically and against symlink/reparse-point
  escape before reading it. Return generic client errors without absolute paths,
  file contents, environment values, or stack traces.
- Run the Dev Container as `vscode`, without privileged mode, added capabilities,
  Docker socket access, extra mounts, or embedded environment secrets. Keep
  `no-new-privileges` and the pinned image digest.
- Keep `GITHUB_TOKEN` at `contents: read`, use full commit SHAs for Actions, disable
  persisted checkout credentials, and do not use `pull_request_target` or CI
  secrets in the baseline workflow.
- Keep NuGet versions centralized, commit generated `packages.lock.json` files,
  restore with `--locked-mode` in CI and the Dev Container, and leave auditing
  enabled. Review Dependabot changes before merging them.
- Run `./scripts/validate-ai-context.sh`, `./scripts/validate-security.sh`, the
  formatting check, Release build, and full tests before handoff.

The repository currently needs no application secret or runtime parameter, so it
does not include `.env.example`. Add one only when a concrete local parameter is
introduced; include names and safe placeholders, never working values.

## AI client operating policy

Use read/search operations for exploration and review. Enable mutation only for a
task that requests a change, and inspect the complete diff before accepting it.
Destructive commands, credential access, permission changes, publishing, commits,
pushes, and any external side effect require explicit human authorization for that
specific action.

The checked-in Codex configuration selects `workspace-write`, disables command
network access and login shells, uses `on-request` elevation with a human reviewer,
and allowlists the two MCP tools. For a review-only session, select read-only mode
in the client. Do not use full-access/no-approval modes for repository work.

Copilot repository instructions and the Architecture Reviewer define the same
read-before-write split, while the reviewer is deterministically limited to read,
search, and the read-only MCP server. General Copilot agent capabilities and
approval behavior vary by product surface; review the effective tool list and
workspace trust prompt before use.

## Recommended controls

- Use a dedicated development account with disk encryption, host patching, and a
  supported container runtime. Keep unrelated sensitive directories outside the
  workspace.
- Prefer short-lived, least-privilege credentials from an OS or organization
  secret store. Rotate exposed values immediately and review logs/history.
- Require protected branches, reviewed pull requests, passing CI, signed commits
  or artifacts where appropriate, and periodic dependency/provenance review.
- Rebuild the Dev Container after reviewing image-digest updates. Do not add the
  Docker socket; use a separately isolated service if container builds become a
  requirement.
- Use read-only AI sessions for analysis and review. Grant network destinations or
  additional tools individually and remove the grant after use.

## Environment-dependent controls and limitations

- Repository-local Codex configuration is loaded only after the project is
  trusted and may be overridden or constrained by CLI, user, platform, or managed
  policy. Sandbox enforcement varies by OS and client. Administrators should use
  managed requirements when settings must be non-overridable, then test the actual
  execution surface. See the official [Codex configuration reference](https://developers.openai.com/codex/config-reference)
  and [sandbox and approvals guidance](https://learn.chatgpt.com/docs/agent-approvals-security).
- Command-network restrictions do not necessarily govern web search, MCP, browser,
  extensions, connectors, the model service, or other application traffic. Disable
  or allowlist each surface through its own client or organization controls.
- Dev Container restrictions reduce privileges inside the container; they do not
  protect against a compromised Docker daemon, kernel, host mount, editor
  extension, or user account. The workspace remains mounted read-write because
  development requires edits.
- GitHub branch protection, required reviewers, secret scanning, Dependabot
  availability, runner hardening, organization Actions allowlists, and log
  retention must be configured in GitHub. This repository cannot enforce them.
- Symlink checks reduce path escape but cannot eliminate filesystem race conditions
  against a malicious local process. Use an isolated VM or disposable environment
  for hostile repositories or dependencies.

## Secrets procedure

When a secret becomes necessary, document only its variable name, purpose, scope,
and provisioning method. Store local values outside the repository; store CI
values in the platform secret store; inject them only into the step that needs
them; mask output; never echo them; and grant read/use access to the smallest
principal. Revoke and rotate after suspected exposure. A future `.env.example`
must contain inert placeholders and remain covered by tests and ignore rules.
