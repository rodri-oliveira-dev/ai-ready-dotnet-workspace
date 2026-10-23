using System.Diagnostics;
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
    [InlineData("nested/../../outside.txt")]
    [InlineData(".git/config")]
    [InlineData(".SSH/id_rsa")]
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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid\0path")]
    public void ResolvePathRejectsInvalidInput(string path)
    {
        var policy = new WorkspacePathPolicy(_workspace);

        Assert.ThrowsAny<ArgumentException>(() => policy.ResolvePath(path));
    }

    [Fact]
    public void ResolvePathRejectsExcessivelyLongInput()
    {
        var policy = new WorkspacePathPolicy(_workspace);

        Assert.Throws<ArgumentException>(() => policy.ResolvePath(new string('a', 1025)));
    }

    [Fact]
    public void ResolveReadableFileRejectsSymbolicLinkThatEscapesWorkspace()
    {
        var outside = Path.Combine(
            Path.GetTempPath(),
            $"workspace-mcp-outside-{Guid.NewGuid():N}");
        var link = Path.Combine(_workspace, "external");
        Directory.CreateDirectory(outside);
        File.WriteAllText(Path.Combine(outside, "secret.txt"), "must-not-be-readable");

        try
        {
            CreateDirectoryLink(link, outside);
            var policy = new WorkspacePathPolicy(_workspace);

            Assert.Throws<UnauthorizedAccessException>(
                () => policy.ResolveReadableFile("external/secret.txt"));
        }
        finally
        {
            if (Directory.Exists(link))
            {
                Directory.Delete(link);
            }

            Directory.Delete(outside, recursive: true);
        }
    }

    public void Dispose()
    {
        Directory.Delete(_workspace, recursive: true);
    }

    private static void CreateDirectoryLink(string link, string target)
    {
        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateSymbolicLink(link, target);
            return;
        }

        var startInfo = new ProcessStartInfo("cmd.exe")
        {
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("mklink");
        startInfo.ArgumentList.Add("/J");
        startInfo.ArgumentList.Add(link);
        startInfo.ArgumentList.Add(target);

        using var process = Process.Start(startInfo) ??
            throw new InvalidOperationException("Could not start the junction test helper.");
        process.WaitForExit();

        Assert.True(
            process.ExitCode == 0,
            $"Could not create the test junction: {process.StandardError.ReadToEnd()}");
    }
}
