# ADR-001: Shared and tool-specific AI instructions

- Status: Accepted
- Date: 2026-10-22

## Context

AI clients discover context through different paths, scopes, and precedence rules.
Duplicating all policy for every client creates drift, while using only one
client-specific format excludes other tools and prevents precise path guidance.

## Decision

Keep durable, provider-neutral repository rules in root `AGENTS.md`. Put only
Copilot-specific discovery, scoped instructions, agents, and prompts under
`.github/`. Put Codex project configuration under `.codex/`, while reusable
procedures live as skills under `.agents/skills/`. Validate required files,
frontmatter, globs, and duplicate bullet rules with `validate-ai-context.sh`.

Files must complement rather than contradict one another because effective
precedence varies by client. Documentation must qualify compatibility claims.

## Alternatives considered

- Maintain complete duplicated instruction sets for each client: rejected because
  policy would drift and review would become noisy.
- Use only `AGENTS.md`: rejected because client-specific discovery and scoped
  workflows still require native configuration.
- Generate instruction files during setup: rejected because generated policy is
  harder to review in pull requests and introduces another toolchain.

## Consequences

Shared rules have one reviewable source, and native features remain available.
Some purposeful overlap in concepts remains, and maintainers must test discovery
in actual client versions when claiming support. Repository checks can validate
structure, not the behavior of an external AI client.
