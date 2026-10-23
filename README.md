# AI-ready .NET Workspace

[Português (Brasil)](README.pt-BR.md)

A compact, provider-neutral GitHub template for reproducible .NET development with
versioned AI guidance, least-privilege tooling, deterministic checks, and a working
Minimal API example. It is designed to be renamed after repository creation and
does not select an AI model, application database, or production cloud provider.

## Goals

- Keep human and AI contributors aligned through reviewable repository context.
- Make local, Dev Container, and CI workflows equivalent and reproducible.
- Separate application code, tests, development tools, and external integrations.
- Demonstrate narrow MCP capabilities and explicit security boundaries.
- Evaluate AI-assisted work without presenting structural checks as model evidence.

## Quick start

Use the Dev Container, or install the local prerequisites, then run:

```bash
dotnet tool restore
dotnet tool run adr-guard -- check
dotnet restore AiReadyWorkspace.slnx --locked-mode
dotnet build AiReadyWorkspace.slnx --configuration Release --no-restore
dotnet test AiReadyWorkspace.slnx --configuration Release --no-build
dotnet run --project src/SampleApi --urls http://localhost:5080
```

Try `GET /health` or `GET /api/greeting?name=Ada`. On Linux, macOS, WSL,
or inside the Dev Container, validate the complete local toolchain with:

```bash
./scripts/verify-environment.sh
```

## Environment requirements

Choose one development path:

- Docker and a Dev Container-compatible editor. The pinned container includes the
  .NET SDK and runs as the non-root `vscode` user.
- .NET 10 SDK `10.0.400` or a compatible patch selected by `global.json`, Git, and
  Bash. Template and repository validation also require common POSIX tools such as
  `grep`, `sed`, `tar`, and `mktemp`; on Windows, use WSL or the Dev Container.

GitHub CLI is optional and is needed only for repository and pull-request tasks.
Copilot, Codex, and other AI clients are optional and may require separate plans,
authentication, workspace trust, or organization policy.

## Included capabilities

- .NET 10 Minimal API and HTTP integration tests.
- Central package versions, committed lock files, nullable analysis, warnings as
  errors, deterministic builds, and NuGet auditing.
- Digest-pinned Dev Container, VS Code recommendations/tasks, and read-only CI.
- Version-pinned ADR Guard governance through a local .NET tool, an immutable
  GitHub Action, and an optional VS Code extension.
- Shared `AGENTS.md`, scoped Copilot instructions, prompts, a specialized reviewer,
  and an on-demand validation skill.
- A local stdio MCP server exposing only sanitized project metadata and architecture
  rules, with unit and protocol-level integration tests.
- Security baseline, threat model, paired AI evaluation protocol, and isolated
  template initialization validation.

See [Architecture](docs/architecture.md), [ADRs](docs/adr/README.md), the
[case study](docs/case-study/README.md), and the [security policy](SECURITY.md).

## Repository structure

```text
src/SampleApi/                    Example Minimal API
tests/                            Application and MCP tests
tools/Workspace.McpServer/        Read-only local MCP server
.agents/skills/                   Reusable agent workflows
.github/                          Copilot context, CI, and community files
.devcontainer/                    Reproducible development environment
evaluations/                      Versioned scenarios and evidence contract
scripts/                          Initialization and deterministic validation
.config/dotnet-tools.json         Versioned local .NET tools
docs/adr/                         Canonical architecture decision records
docs/                             Architecture, security, and case study
```

## Architecture governance with ADR Guard

[ADR Guard](https://github.com/rodri-oliveira-dev/adr-guard) validates the
canonical ADR collection in `docs/adr` according to `.adrguard.yml`. Restore the
version-pinned [NuGet tool](https://www.nuget.org/packages/RodriOliveira.AdrGuard/1.9.0)
and run the read-only check from the repository root:

```bash
dotnet tool restore
dotnet tool run adr-guard -- --version
dotnet tool run adr-guard -- check
```

The local CLI provides reproducible authoring and validation commands. The
SHA-pinned GitHub Action runs the same `1.9.0` validation as the CI gate without
AI services or extra tokens. The optional
[VS Code extension](https://marketplace.visualstudio.com/items?itemName=rodrioliveira.adr-guard)
adds editor navigation and diagnostics but does not bundle the CLI.

Open `ai-ready-dotnet-workspace.code-workspace` directly to use the recommended
extensions and the **ADR Guard: Restore Tool**, **ADR Guard: Check**, **ADR Guard:
Index**, and **Workspace: Validate All** tasks. Index generation is explicit and
is never run on save. The Dev Container restores the manifest automatically; run
the same CLI commands in its terminal.

```bash
code ai-ready-dotnet-workspace.code-workspace
```

The extension looks for a standalone `adr-guard` executable on the local or
remote extension host `PATH`; it does not discover manifest-only tools invoked by
`dotnet tool run`. Its UI is optional, and every repository task works without a
global installation. To enable the UI, install the same pinned package into a
user-owned directory on the relevant host, add that directory to `PATH`, or set
the machine-scoped `adrGuard.cli.path` in your personal VS Code settings:

```bash
dotnet tool install --tool-path "$HOME/.dotnet/tools" RodriOliveira.AdrGuard --version 1.9.0
```

Run that optional command inside a Dev Container, WSL, SSH host, or Codespace
when the extension runs there; a host installation is not visible remotely. Do
not commit a machine-specific executable path.

To update ADR Guard, select a published version, update
`.config/dotnet-tools.json`, the Action `version` input and its immutable `uses`
SHA together, then restore, check `--version`, validate the ADRs, and run the full
workspace validation. This keeps local and CI behavior aligned.

## Use this GitHub template

After the repository owner enables **Template repository**, select **Use this
template** on GitHub, create a new repository, clone it, and initialize its names:

```bash
./scripts/initialize-template.sh \
  --solution-name ContosoWorkspace \
  --project-name Contoso.Api
```

The initializer renames the solution, application and test projects, namespaces,
project references, and known documentation/configuration references. It records
the chosen names in `.template-initialized`; commit that file so later validation
can verify idempotence. It rejects unsafe names, conflicting targets, and a second
initialization with different names.

Until this feature branch is merged, the complete template is not available from
the default branch. Maintainers should follow [Repository settings](docs/repository-settings.md)
after the merge.

## Configure AI-assisted development

Review every file before trusting the workspace. These sources have distinct jobs:

| Mechanism | Responsibility |
| --- | --- |
| `AGENTS.md` | Shared repository rules for compatible agents. |
| `.github/copilot-instructions.md` | Copilot-wide guidance; scoped files under `.github/instructions/` apply to matching paths where supported. |
| `.github/agents/` and `.github/prompts/` | A read-only architecture reviewer and manually invoked workflows for supported Copilot surfaces. |
| `.agents/skills/validate-change/SKILL.md` | On-demand, repeatable validation procedure for compatible agents. |
| `.mcp.json` | Portable workspace MCP declaration used by supported clients. |
| `.codex/config.toml` | Trusted-project Codex sandbox, approval, and MCP allowlist defaults. |

Build the Release configuration before starting the checked-in MCP command:

```bash
dotnet build AiReadyWorkspace.slnx --configuration Release
```

The server exposes `list_dotnet_projects` and `get_architecture_rules`. It has no
write, network, command-execution, credential, or arbitrary-file tool. In a
supported Copilot client, inspect the `workspace` MCP server and select
**Architecture Reviewer** or invoke `/review-architecture`. In Codex, trust the
reviewed project, inspect the effective configuration and MCP tool list, then use
the `validate-change` skill when relevant. Client discovery and precedence vary;
repository validation does not prove compatibility with every client or version.

## Customize the template

1. Run the initializer once and review its diff.
2. Replace the example API while preserving the `src/`, `tests/`, and `tools/`
   responsibility boundaries that still apply.
3. Update `AGENTS.md`, scoped instructions, evaluations, and ADRs when architecture
   or policy changes.
4. Add external providers only for a concrete requirement and document credentials
   without storing values in the repository.
5. Update the Dev Container, VS Code tasks, and CI together so workflows remain
   equivalent.
6. Run the full validation suite before making the new repository public.

## Build, test, validate, and evaluate

```bash
dotnet tool restore
dotnet tool run adr-guard -- check
dotnet restore AiReadyWorkspace.slnx --locked-mode
dotnet format AiReadyWorkspace.slnx --verify-no-changes --no-restore
dotnet build AiReadyWorkspace.slnx --configuration Release --no-restore
dotnet test AiReadyWorkspace.slnx --configuration Release --no-build
./scripts/validate-ai-context.sh
./scripts/validate-security.sh
./scripts/validate-evaluations.sh
./scripts/validate-template.sh
```

The last command copies tracked and unignored content into a uniquely named
temporary directory, initializes it twice, checks residual names, restores,
formats, builds, tests, and verifies that the source repository was unchanged.

The [evaluation protocol](evaluations/protocol.md) defines paired runs with and
without repository AI context. Scenario and report validators only verify the
contract. Real model results require recorded client/model versions, commands,
logs, tool audit, corrections, duration, and limitations; none are claimed here.

## Limitations and next steps

- No real Copilot or Codex evaluation run is bundled; current evidence is
  deterministic repository, .NET, and MCP validation.
- Repository instructions influence behavior but do not enforce a client sandbox.
- Branch protection, required reviews, secret scanning, organization policy, and
  template activation must be configured on GitHub.
- Dev Container isolation does not protect against a compromised host or container
  runtime; filesystem race risks remain for local readers.
- The sample intentionally omits authentication, persistence, observability, and
  deployment. Add them only with explicit requirements, tests, and threat updates.
- Future work can capture controlled paired AI runs, expand cross-platform CI, and
  add provider-specific examples as optional modules rather than baseline coupling.

## Contributing and license

See [CONTRIBUTING.md](CONTRIBUTING.md) and [SECURITY.md](SECURITY.md). Licensed
under the [MIT License](LICENSE), copyright Rodrigo de Oliveira.
