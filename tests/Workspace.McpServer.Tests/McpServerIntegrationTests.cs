using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Workspace.McpServer.Tests;

public sealed class McpServerIntegrationTests
{
    [Fact]
    public async Task StdioClientListsAndInvokesReadOnlyTools()
    {
        var repositoryRoot = FindRepositoryRoot();
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "Workspace integration test",
            Command = "dotnet",
            Arguments =
            [
                "run",
                "--project",
                "tools/Workspace.McpServer/Workspace.McpServer.csproj",
                "--configuration",
                "Release",
                "--no-build",
                "--",
                "--workspace",
                "."
            ],
            WorkingDirectory = repositoryRoot
        });

        await using var client = await McpClient.CreateAsync(transport);
        var tools = await client.ListToolsAsync();

        Assert.Equal(
            ["get_architecture_rules", "list_dotnet_projects"],
            tools.Select(tool => tool.Name).Order(StringComparer.Ordinal).ToArray());
        Assert.All(tools, tool => Assert.True(tool.ProtocolTool.Annotations?.ReadOnlyHint));
        Assert.All(tools, tool => Assert.False(tool.ProtocolTool.Annotations?.DestructiveHint));
        Assert.All(tools, tool => Assert.False(tool.ProtocolTool.Annotations?.OpenWorldHint));

        var result = await client.CallToolAsync(
            "list_dotnet_projects",
            cancellationToken: CancellationToken.None);
        var text = string.Join(
            Environment.NewLine,
            result.Content.OfType<TextContentBlock>().Select(content => content.Text));

        Assert.NotEqual(true, result.IsError);
        Assert.Contains("Workspace.McpServer", text, StringComparison.Ordinal);

        var rulesResult = await client.CallToolAsync(
            "get_architecture_rules",
            cancellationToken: CancellationToken.None);
        var rulesText = string.Join(
            Environment.NewLine,
            rulesResult.Content.OfType<TextContentBlock>().Select(content => content.Text));

        Assert.NotEqual(true, rulesResult.IsError);
        Assert.Contains("API entry points own HTTP routing", rulesText, StringComparison.Ordinal);
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
