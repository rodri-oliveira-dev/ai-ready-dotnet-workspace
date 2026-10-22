# Contributing

Thank you for improving this template. Keep contributions small, provider-neutral,
and safe to rename.

## Before opening a pull request

1. Create a focused branch from the current default branch.
2. Read `AGENTS.md` and the instructions that apply to changed paths.
3. Do not include secrets, personal data, generated output, local environment files,
   or unrelated formatting/refactoring.
4. Add or update tests and documentation for behavior, commands, or policy changes.
5. Add package versions only to `Directory.Packages.props` and update lock files
   through a reviewed restore when dependencies change.

Run the applicable checks from the repository root; for a full change use:

```bash
dotnet restore AiReadyWorkspace.slnx --locked-mode
dotnet format AiReadyWorkspace.slnx --verify-no-changes --no-restore
dotnet build AiReadyWorkspace.slnx --configuration Release --no-restore
dotnet test AiReadyWorkspace.slnx --configuration Release --no-build
./scripts/validate-ai-context.sh
./scripts/validate-security.sh
./scripts/validate-evaluations.sh
./scripts/validate-template.sh
```

In the pull request, explain motivation, architecture/security effects, commands
actually run, results, and residual limitations. Do not claim an AI-client test
unless the client interaction and evidence were captured.

## Reporting defects and proposing changes

Use the issue forms for reproducible bugs or scoped feature proposals. For a
security vulnerability, follow [SECURITY.md](SECURITY.md) instead of opening a
public issue. By contributing, you agree that your contribution is licensed under
the repository's [MIT License](LICENSE).
