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

VS Code tasks named `dotnet: restore`, `dotnet: build`, and `dotnet: test` provide the same workflow from the command palette.

## Repository structure

```text
src/SampleApi/          Minimal API
tests/SampleApi.Tests/  Integration tests
.devcontainer/          Reproducible development environment
.github/workflows/      Continuous integration
scripts/                Environment verification
```

## AI-assisted development

The repository keeps shared, durable rules in [`AGENTS.md`](AGENTS.md) and adds client-specific context only where the clients differ:

| Source | Discovery and scope |
| --- | --- |
| `AGENTS.md` | Codex reads it from the project root before work. It combines global-to-local files, with guidance nearer the working directory applied later. Supported Copilot clients can also load it as agent instructions. |
| `.github/copilot-instructions.md` | Copilot repository-wide behavior that is not part of the shared contract. |
| `.github/instructions/*.instructions.md` | Copilot attaches a file when its `applyTo` glob matches a file being changed; support varies by Copilot client and feature. |
| `.github/prompts/*.prompt.md` | Reusable, manually invoked workflows in supported IDE clients; they are not always-on instructions and are not Codex project prompts. |

Codex project configuration is intentionally absent. Current Codex clients support trusted-project overrides in `.codex/config.toml`, but this template has no project-specific setting that should override a contributor's model, approval, sandbox, network, or provider choices. The shared `AGENTS.md` works without that file and avoids distributing environment policy.

Precedence differs by client. Codex assembles `AGENTS.md` from the global scope through the project tree and lets closer files override earlier guidance. Most Copilot clients combine applicable repository and path instructions without a general precedence rule; Copilot Chat on GitHub.com documents personal instructions first, then path-specific, repository-wide, agent, and organization instructions. Therefore these files are complementary and must not depend on conflicts being resolved in a particular order. See the official [Codex `AGENTS.md` guide](https://developers.openai.com/codex/guides/agents-md), [Codex configuration reference](https://developers.openai.com/codex/config-reference), and [GitHub Copilot custom-instructions guide](https://docs.github.com/en/copilot/concepts/prompting-and-context/custom-instructions).

When Copilot or Codex is unavailable, do not treat file validation as a real agent run. Run `./scripts/validate-ai-context.sh`, then validate discovery manually when the client is available:

- In Copilot Chat, inspect the response references or agent debug log, edit a file under `src/` or `tests/`, and confirm the matching scoped instructions appear. Invoke `/implement-feature` or `/review-changes` in a client that supports workspace prompt files.
- In Codex, start a new session at the repository root and ask it to list the active instruction sources. Confirm that the root `AGENTS.md` is present; project `.codex` configuration should not be listed.

## Use as a template

Create a repository from this template, then rename `AiReadyWorkspace.slnx`, the `SampleApi` projects, and their namespaces for your product. Keep project paths aligned in the solution, test project reference, VS Code settings/tasks, workflow, and scripts.

This repository is prepared for later stages without preselecting an AI runtime provider, model, database, or secret-management approach.
