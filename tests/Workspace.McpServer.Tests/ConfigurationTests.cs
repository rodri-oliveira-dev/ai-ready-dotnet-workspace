using System.Text.Json;

namespace Workspace.McpServer.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void PortableMcpConfigurationStartsOnlyWorkspaceServer()
    {
        var repositoryRoot = FindRepositoryRoot();
        using var document = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(repositoryRoot, ".mcp.json")));

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
        var repositoryRoot = FindRepositoryRoot();
        var configuration = File.ReadAllText(
            Path.Combine(repositoryRoot, ".codex", "config.toml"));

        Assert.Contains("[mcp_servers.workspace]", configuration, StringComparison.Ordinal);
        Assert.Contains(
            "enabled_tools = [\"list_dotnet_projects\", \"get_architecture_rules\"]",
            configuration,
            StringComparison.Ordinal);
        Assert.Contains(
            "default_tools_approval_mode = \"prompt\"",
            configuration,
            StringComparison.Ordinal);
        Assert.DoesNotContain("danger-full-access", configuration, StringComparison.Ordinal);
        Assert.DoesNotContain("approval_policy = \"never\"", configuration, StringComparison.Ordinal);
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
