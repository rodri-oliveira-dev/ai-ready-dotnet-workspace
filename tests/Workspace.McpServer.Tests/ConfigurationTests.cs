using System.Text.Json;

namespace Workspace.McpServer.Tests;

public sealed class ConfigurationTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact]
    public void PortableMcpConfigurationStartsOnlyWorkspaceServer()
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(RepositoryRoot, ".mcp.json")));

        var servers = document.RootElement.GetProperty("mcpServers");
        var server = servers.GetProperty("workspace");
        var arguments = server.GetProperty("args")
            .EnumerateArray()
            .Select(value => value.GetString())
            .ToArray();

        Assert.Single(servers.EnumerateObject());
        Assert.Equal("dotnet", server.GetProperty("command").GetString());
        Assert.Contains("tools/Workspace.McpServer/Workspace.McpServer.csproj", arguments);
        Assert.DoesNotContain("--interactive", arguments);
        Assert.False(server.TryGetProperty("env", out _));
    }

    [Fact]
    public void CodexConfigurationAllowlistsReadOnlyWorkspaceTools()
    {
        var configuration = File.ReadAllText(
            Path.Combine(RepositoryRoot, ".codex", "config.toml"));

        Assert.Contains("[mcp_servers.workspace]", configuration, StringComparison.Ordinal);
        Assert.Contains(
            "enabled_tools = [\"list_dotnet_projects\", \"get_architecture_rules\"]",
            configuration,
            StringComparison.Ordinal);
        Assert.Contains(
            "default_tools_approval_mode = \"prompt\"",
            configuration,
            StringComparison.Ordinal);
        Assert.Contains("approval_policy = \"on-request\"", configuration, StringComparison.Ordinal);
        Assert.Contains("approvals_reviewer = \"user\"", configuration, StringComparison.Ordinal);
        Assert.Contains("sandbox_mode = \"workspace-write\"", configuration, StringComparison.Ordinal);
        Assert.Contains("network_access = false", configuration, StringComparison.Ordinal);
        Assert.Contains("allow_login_shell = false", configuration, StringComparison.Ordinal);
        Assert.DoesNotContain("danger-full-access", configuration, StringComparison.Ordinal);
        Assert.DoesNotContain("approval_policy = \"never\"", configuration, StringComparison.Ordinal);
    }

    [Fact]
    public void DevContainerUsesLeastPrivilegeDefaults()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(
            Path.Combine(RepositoryRoot, ".devcontainer", "devcontainer.json")));
        var root = document.RootElement;
        var runArguments = root.GetProperty("runArgs")
            .EnumerateArray()
            .Select(value => value.GetString())
            .ToArray();
        var serialized = root.GetRawText();

        Assert.Equal("vscode", root.GetProperty("containerUser").GetString());
        Assert.Equal("vscode", root.GetProperty("remoteUser").GetString());
        Assert.Contains("--cap-drop=ALL", runArguments);
        Assert.Contains("--security-opt=no-new-privileges:true", runArguments);
        Assert.DoesNotContain("privileged", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("docker.sock", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("containerEnv", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("remoteEnv", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("mounts", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkflowPinsActionsAndKeepsTokenReadOnly()
    {
        var workflow = File.ReadAllText(
            Path.Combine(RepositoryRoot, ".github", "workflows", "ci.yml"));

        var normalizedWorkflow = workflow.ReplaceLineEndings("\n");

        Assert.Contains("permissions:\n  contents: read", normalizedWorkflow, StringComparison.Ordinal);
        Assert.Matches(
            @"uses: actions/checkout@[0-9a-f]{40} # v7",
            workflow);
        Assert.Matches(
            @"uses: actions/setup-dotnet@[0-9a-f]{40} # v6",
            workflow);
        Assert.Contains("persist-credentials: false", workflow, StringComparison.Ordinal);
        Assert.Contains("./scripts/validate-security.sh", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("permissions: write", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("${{ secrets.", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("pull_request_target", workflow, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "AiReadyWorkspace.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ??
               throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
