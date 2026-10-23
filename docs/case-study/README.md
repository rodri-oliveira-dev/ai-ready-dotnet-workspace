# Case study: an AI-ready .NET workspace

## Problem

Teams adopting coding agents often accumulate instructions, prompts, tools, and
editor settings without a clear authority model. Local and CI commands diverge,
tool permissions become broader than their use case, and successful builds are
mistaken for evidence that an AI workflow is safe or effective. A reusable template
must address those concerns without locking a new application to one AI or cloud provider.

## Goals and constraints

The project targets a small, renameable .NET baseline with reviewable AI context,
reproducible developer workflows, narrow tooling, automated tests, and explicit
evaluation evidence. It must keep production code independent of AI tooling, avoid
secrets and privileged defaults, preserve warnings as errors, and work without any
AI client installed. New layers and providers are deliberately excluded until a
concrete application requirement exists.

## Adopted architecture

The solution contains an example Minimal API, black-box HTTP tests, and a separate
development-only MCP server. Root guidance establishes shared policy; Copilot and
Codex files express client-specific discovery and controls. A pinned Dev Container,
central package management, lock files, VS Code tasks, and CI converge on the same
commands. Repository scripts validate policy and create a disposable renamed copy.

See the [component diagram and flows](../architecture.md) and the five
[architecture decisions](../adr/README.md).

## Decisions and trade-offs

- Shared plus client-specific context limits duplication but still requires real
  client checks because discovery behavior can change.
- Two purpose-built MCP tools are less flexible than generic filesystem/shell tools,
  but their data exposure and effects are inspectable and testable.
- Immutable dependency pins improve reviewability but require regular maintenance.
- A Bash initializer stays small and transparent, while making WSL, a container, or
  another POSIX environment necessary on Windows.
- Live model evaluation is kept out of CI; this avoids credentials and flaky claims,
  but means effectiveness evidence must be captured separately.

## Demonstrations

### Application behavior

Run the API and request `/health` or `/api/greeting?name=Ada`. Integration tests
host the application and verify the health and default/explicit greeting contracts.

### Restricted architecture review

Build Release, start the configured `workspace` MCP server from a compatible
client, inspect its two tools, and request the Architecture Reviewer. The project
catalog response contains selected SDK/framework/reference metadata; the rules
response contains only the `Layer responsibilities` section of `AGENTS.md`.

### Derived template

Run `./scripts/validate-template.sh`. It copies repository content to a unique
temporary directory, initializes alternate solution/project names twice, rejects a
conflicting third initialization, scans functional paths for old names, and runs
locked restore, formatting, Release build, and all tests before constrained cleanup.

### Evaluation protocol

Use one scenario from `evaluations/scenarios/` against paired clean copies as
described in `evaluations/protocol.md`. Populate a copy of
`evaluations/report-template.json`; never overwrite the template with invented data.

## Validation evidence

The repository provides executable, reviewable evidence rather than embedded
historical success claims:

| Evidence | What it demonstrates |
| --- | --- |
| Application tests | Observable HTTP contracts and validation paths. |
| MCP tests | Configuration parity, safe path handling, sanitized output/errors, tool annotations, and stdio protocol behavior. |
| `validate-ai-context.sh` | Required context, metadata, scoped globs, MCP declarations, and selected duplication checks. |
| `validate-security.sh` | Repository-controlled CI, container, Codex, and MCP policy invariants. |
| `validate-evaluations.sh` | Complete scenarios and an honest report schema initialized as `not_run`. |
| `validate-template.sh` | Idempotent renaming and a working isolated derived repository. |
| GitHub Actions | Clean Linux execution of the deterministic suite on pull requests and `main`. |

The final implementation pull request records the exact commands and observed
results for this delivery. A passing validator is evidence only for the behavior
it directly checks; it is not evidence of universal client compatibility or model quality.

## Limitations

No controlled paired Copilot/Codex run is included. Windows-native Bash-free
initialization, multi-platform CI, production authentication, persistence,
observability, deployment, and cloud policy are out of scope. Repository settings
such as required reviews, secret scanning, and template mode remain GitHub controls.
The threat model also documents host, external-network, and local filesystem race risks.

## Learnings and evolution

AI readiness is primarily an engineering-governance property: bounded context,
small tools, reproducible commands, reviewable permissions, and honest evidence.
Useful next steps are to run the paired scenarios under controlled client versions,
publish redacted reports, add a Windows validation path, and introduce optional
provider modules only when real consumers establish their requirements.
