# GitHub repository settings

These settings are applied on GitHub and are intentionally not automated by the
template. Do not weaken security or change visibility while enabling template use.

## Enable Template repository after merge

The complete implementation becomes available from the default branch only after
the feature pull request is reviewed and merged. Then, with repository admin access:

1. Open **Settings** for `rodri-oliveira-dev/ai-ready-dotnet-workspace`.
2. In **General**, locate the repository features section.
3. Select **Template repository**.
4. Return to the repository page and verify **Use this template** is available.
5. Create a disposable repository from the template and verify that it starts from
   `main`, then follow the initializer instructions in the README.

See GitHub's official guides for [creating a template repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-template-repository)
and [creating a repository from a template](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template).

Enabling template mode does not copy branch protection, secrets, Actions variables,
environments, webhooks, apps, or organization policies into repositories created
from it. Configure those controls separately. Do not enable the setting before the
completed version is on `main`, because GitHub creates template repositories from
the default branch.

## Suggested public metadata

- Description: `Reproducible AI-ready .NET workspace template with Dev Containers, Copilot/Codex context, a least-privilege MCP server, and deterministic validation.`
- Website: leave empty unless a maintained documentation site exists.
- Topics: `dotnet`, `dotnet-template`, `devcontainer`, `github-copilot`, `codex`,
  `mcp`, `ai-agents`, `developer-experience`, `security`, `architecture`.

Before changing metadata, verify it remains accurate for the default branch. These
topics are descriptive suggestions, not compatibility guarantees. GitHub documents
that topics are public and limits repositories to 20 lowercase topics of at most
50 characters each; see [Classifying your repository with topics](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics).
