# ADR-002: Agents, skills and MCP responsibilities

- Status: Accepted
- Date: 2026-10-22

## Context

Roles, workflows, and capabilities are often conflated in AI integrations. That
makes permission review difficult and encourages broad, opaque tools.

## Decision

Use agents for role and response behavior, skills for reusable procedures, prompts
for user-invoked coordination, and MCP for narrow capabilities. The Architecture
Reviewer is read-only. The `validate-change` skill selects existing checks rather
than owning duplicate scripts. `Workspace.McpServer` exposes only
`list_dotnet_projects` and `get_architecture_rules`, both annotated read-only,
non-destructive, idempotent, and closed-world.

## Alternatives considered

- One autonomous agent with shell, filesystem, and network access: rejected as too
  broad for an architecture-review demonstration.
- Put validation execution inside MCP: rejected because repository scripts already
  own those commands and command execution would expand the trust boundary.
- Encode every workflow in prompts: rejected because procedures would be harder to
  discover, reuse, and validate independently.

## Consequences

Capabilities and behavior can be reviewed separately, and MCP remains usable by
more than one compatible client. Coordination requires several small files, and a
read-only reviewer must report commands it cannot execute instead of implying they ran.
