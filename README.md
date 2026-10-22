# AI-ready .NET Workspace

A small, reusable foundation for .NET development environments. This first stage contains a .NET 10 Minimal API, integration tests, a Dev Container, VS Code tasks, and continuous integration. It intentionally does not include AI components or a database.

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

VS Code tasks named `dotnet: restore`, `dotnet: build`, and `dotnet: test` provide the same workflow from the command palette.

## Repository structure

```text
src/SampleApi/          Minimal API
tests/SampleApi.Tests/  Integration tests
.devcontainer/          Reproducible development environment
.github/workflows/      Continuous integration
scripts/                Environment verification
```

## Use as a template

Create a repository from this template, then rename `AiReadyWorkspace.slnx`, the `SampleApi` projects, and their namespaces for your product. Keep project paths aligned in the solution, test project reference, VS Code settings/tasks, workflow, and scripts.

This repository is prepared for later stages without preselecting an AI provider, model, database, or secret-management approach.
