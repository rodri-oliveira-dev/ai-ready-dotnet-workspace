using Workspace.McpServer;

namespace Workspace.McpServer.Tests;

public sealed class WorkspacePathPolicyTests : IDisposable
{
    private readonly string _workspace = Path.Combine(
        Path.GetTempPath(),
        $"workspace-mcp-tests-{Guid.NewGuid():N}");

    public WorkspacePathPolicyTests()
    {
        Directory.CreateDirectory(_workspace);
        File.WriteAllText(Path.Combine(_workspace, "AGENTS.md"), "# Test");
    }

    [Fact]
    public void ResolveReadableFileAllowsWorkspaceFile()
    {
        var policy = new WorkspacePathPolicy(_workspace);

        var result = policy.ResolveReadableFile("AGENTS.md");

        Assert.Equal(Path.Combine(_workspace, "AGENTS.md"), result);
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData(".git/config")]
    [InlineData(".env")]
    [InlineData("config/.env.local")]
    public void ResolvePathRejectsUnauthorizedPaths(string path)
    {
        var policy = new WorkspacePathPolicy(_workspace);

        Assert.Throws<UnauthorizedAccessException>(() => policy.ResolvePath(path));
    }

    [Fact]
    public void ResolvePathRejectsAbsolutePath()
    {
        var policy = new WorkspacePathPolicy(_workspace);

        Assert.Throws<UnauthorizedAccessException>(() => policy.ResolvePath(_workspace));
    }

    public void Dispose()
    {
        Directory.Delete(_workspace, recursive: true);
    }
}
