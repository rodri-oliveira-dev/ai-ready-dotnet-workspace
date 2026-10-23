using Workspace.McpServer;

namespace Workspace.McpServer.Tests;

public sealed class WorkspaceServerOptionsTests
{
    [Fact]
    public void ParseAcceptsExplicitWorkspace()
    {
        var result = WorkspaceServerOptions.Parse(["--workspace", "."]);

        Assert.Equal(".", result.WorkspaceRoot);
    }

    [Theory]
    [InlineData("--workspace")]
    [InlineData("--unknown")]
    [InlineData("--workspace", "")]
    [InlineData("--workspace", " ")]
    [InlineData("--workspace", ".", "extra")]
    public void ParseRejectsInvalidArguments(params string[] arguments)
    {
        Assert.Throws<ArgumentException>(() => WorkspaceServerOptions.Parse(arguments));
    }
}
