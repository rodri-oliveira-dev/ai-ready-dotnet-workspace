# AI-ready .NET Workspace

A small, reusable foundation for .NET development environments. It contains a .NET 10 Minimal API, integration tests, a Dev Container, VS Code tasks, continuous integration, and versioned guidance for AI-assisted development. It intentionally does not include an AI runtime, model provider, or database.

## Prerequisites

Choose one of these development options:

- [Docker](https://docs.docker.com/get-docker/) and a Dev Container-compatible editor. The container provides the .NET 10 SDK, Git, a non-root `vscode` user, and recommended C# extensions.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and Git installed locally.

## Get started

Open the repository in its Dev Container, or run locally:

```bash
dotnet restore AiReadyWorkspace.slnx
dotnet build AiReadyWorkspace.slnx --no-restore
dotnet test AiReadyWorkspace.slnx --no-build
dotnet run --project src/SampleApi --urls http://localhost:5080
```

The example API exposes:

- `GET /health` for a basic health response.
- `GET /api/greeting?name=Ada` for an example response.

To check the complete local toolchain on Linux, macOS, WSL, or inside the Dev Container, run:

```bash
./scripts/verify-environment.sh
```

To validate the versioned AI context files, their frontmatter, scoped globs, prompt metadata, and duplicate rules, run:

```bash
./scripts/validate-ai-context.sh
```

To validate the deterministic security policy for CI, the Dev Container, Codex,
and the MCP allowlist, run:

```bash
./scripts/validate-security.sh
```

The [threat model](docs/security/threat-model.md) describes the relevant abuse
cases and residual risks. The [security baseline](docs/security/security-baseline.md)
separates mandatory repository controls from recommendations and controls that
must be enforced by the user's environment or organization.

To validate the versioned AI evaluation contract and exercise initialization in
an isolated derived copy, run:

```bash
./scripts/validate-evaluations.sh
./scripts/validate-template.sh
```

VS Code tasks named `dotnet: restore`, `dotnet: build`, and `dotnet: test` provide the same workflow from the command palette.

## Repository structure

```text
src/SampleApi/                    Minimal API
tests/                            Application and MCP integration tests
tools/Workspace.McpServer/        Read-only workspace MCP server
.agents/skills/                   Reusable agent workflows
.devcontainer/                    Reproducible development environment
.github/                          Copilot configuration and CI
scripts/                          Reproducible repository checks
```

## AI-assisted development

The repository keeps shared, durable rules in [`AGENTS.md`](AGENTS.md) and adds client-specific context only where the clients differ:

| Source | Discovery and scope |
| --- | --- |
| `AGENTS.md` | Codex reads it from the project root before work. It combines global-to-local files, with guidance nearer the working directory applied later. Supported Copilot clients can also load it as agent instructions. |
| `.github/copilot-instructions.md` | Copilot repository-wide behavior that is not part of the shared contract. |
| `.github/instructions/*.instructions.md` | Copilot attaches a file when its `applyTo` glob matches a file being changed; support varies by Copilot client and feature. |
| `.github/agents/*.agent.md` | A specialized Copilot/VS Code role with its own responsibilities and least-privilege tool list. |
| `.agents/skills/*/SKILL.md` | A reusable procedure discovered by compatible clients and loaded only when selected or relevant. |
| `.github/prompts/*.prompt.md` | Reusable, manually invoked workflows in supported IDE clients; they are not always-on instructions and are not Codex project prompts. |
| `.mcp.json` | Portable Copilot workspace configuration for controlled MCP capabilities. |
| `.codex/config.toml` | Trusted-project Codex configuration for the same local MCP server and its tool allowlist. |

The Codex project configuration limits writes to the workspace, disables command
network access and login shells, keeps human-reviewed on-request elevation, and
allowlists only the workspace MCP tools. It does not select a model or credential.
Codex loads project configuration only after the project is trusted; higher-level
or managed configuration and client capabilities still determine the effective
boundary.

Precedence differs by client. Codex assembles `AGENTS.md` from the global scope through the project tree and lets closer files override earlier guidance. Most Copilot clients combine applicable repository and path instructions without a general precedence rule; Copilot Chat on GitHub.com documents personal instructions first, then path-specific, repository-wide, agent, and organization instructions. Therefore these files are complementary and must not depend on conflicts being resolved in a particular order. See the official [Codex `AGENTS.md` guide](https://developers.openai.com/codex/guides/agents-md), [Codex configuration reference](https://developers.openai.com/codex/config-reference), and [GitHub Copilot custom-instructions guide](https://docs.github.com/en/copilot/concepts/prompting-and-context/custom-instructions).

### Agent, skill, and MCP demonstration

These mechanisms have separate responsibilities:

- `Architecture Reviewer` defines read-only review behavior and a standardized, evidence-based response. Its tools are limited to repository reads, search, and `workspace/*` MCP tools.
- `validate-change` defines the repeatable verification procedure and reports failures or checks that could not run. It adds no executable wrapper because the repository already owns the commands.
- `Workspace.McpServer` exposes capabilities, not behavior: `list_dotnet_projects` returns selected project metadata and `get_architecture_rules` returns only the applicable section of `AGENTS.md`.
- `/review-architecture` selects the reviewer, references the skill, and directs it to the MCP tools. A prompt coordinates the three mechanisms but does not grant additional capabilities.

The MCP server uses the official `ModelContextProtocol` C# SDK over stdio. It has no command-execution tool, network dependency, write tool, credential input, or arbitrary file-read tool. Paths are constrained to the configured workspace; malformed or excessive inputs, sensitive paths, traversal, absolute paths, and symbolic links are rejected. Client-facing errors are sanitized. Build the Release configuration before a client starts the checked-in `--no-build` command:

```bash
dotnet build AiReadyWorkspace.slnx --configuration Release
```

The root `.mcp.json` is the current portable Copilot format and is preferred here because no VS Code-specific input, working-directory, development, or sandbox property is needed. Avoid adding a duplicate `.vscode/mcp.json`. The equivalent Codex entry uses `.codex/config.toml`, sets the working directory, allowlists both read-only tools, and keeps tool approval in `prompt` mode. See the [VS Code MCP configuration reference](https://code.visualstudio.com/docs/agents/reference/mcp-configuration), [VS Code custom-agent reference](https://code.visualstudio.com/docs/agents/reference/custom-agent-file-reference), [Codex MCP guide](https://developers.openai.com/codex/mcp), and [official MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk).

When Copilot or Codex is unavailable, do not treat file validation or unit tests as a real agent session. Run `./scripts/validate-ai-context.sh` and the .NET tests, then validate client discovery manually when available:

- In VS Code, trust the reviewed workspace, open **MCP: List Servers**, start `workspace`, and inspect its two tools. Select **Architecture Reviewer** and invoke `/review-architecture` in a session that supports workspace prompts. Confirm the response references both MCP results and the validation skill without offering edits.
- In Codex, trust the project, run `codex mcp list`, then use `/mcp` in a new local session. Confirm `workspace` exposes only the two allowlisted tools and invoke `$validate-change` separately to inspect skill discovery.

## Use as a template

Create a repository from this template, then run the targeted initializer from its
root:

```bash
./scripts/initialize-template.sh \
  --solution-name ContosoWorkspace \
  --project-name Contoso.Api
```

The initializer renames the solution, application and test projects, namespaces,
project references, and known command/configuration references. It deliberately
keeps `Workspace.McpServer` provider-neutral, rejects unsafe names or conflicting
targets, records the selected names in `.template-initialized`, and treats a
second run with the same arguments as a no-op. A second run with different names
fails rather than partially renaming the repository. Commit the state file with
the initialized repository so its CI can verify idempotence.

`./scripts/validate-template.sh` copies tracked and unignored files to a uniquely
named temporary directory, initializes that copy twice, checks for functional
references to the template names, and runs locked restore, formatting, Release
build, and tests. Its cleanup is restricted to the temporary directory it creates;
the source repository is compared before and after the run.

## AI evaluation protocol

The [evaluation suite](evaluations/README.md) defines versioned endpoint, bug-fix,
test-creation, architecture-review, prompt-injection, and unauthorized-access
scenarios. Its [paired protocol](evaluations/protocol.md) compares identical clean
copies with and without repository AI context while holding the model, version,
task, initial commit, client, sandbox, network policy, and other recorded
conditions constant when possible.

The JSON report template starts as `not_run`. Real Copilot or Codex results must
include captured commands, logs, tool audit, duration, client-reported tokens when
available, manual corrections, and limitations. Repository validators check the
scenario and report contract but do not claim that an LLM experiment ran.

This repository is prepared for later stages without preselecting an AI runtime provider, model, database, or secret-management approach.
