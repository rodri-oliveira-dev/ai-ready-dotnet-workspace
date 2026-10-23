using System.Text.Json;
using Workspace.McpServer;

namespace Workspace.McpServer.Tests;

public sealed class WorkspaceReaderTests : IDisposable
{
    private readonly string _workspace = Path.Combine(
        Path.GetTempPath(),
        $"workspace-mcp-reader-tests-{Guid.NewGuid():N}");

    public WorkspaceReaderTests()
    {
        Directory.CreateDirectory(Path.Combine(_workspace, "src", "Demo"));
        File.WriteAllText(
            Path.Combine(_workspace, "src", "Demo", "Demo.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Password>must-not-leak</Password>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Example.Package" />
              </ItemGroup>
            </Project>
            """);
        File.WriteAllText(
            Path.Combine(_workspace, "AGENTS.md"),
            """
            # Guidance

            ## Layer responsibilities

            - Keep HTTP at the boundary.

            ## Security boundaries

            - Secret rule must not be returned.
            """);
    }

    [Fact]
    public async Task ListDotNetProjectsReturnsSelectedMetadataOnly()
    {
        var reader = CreateReader();

        var result = await reader.ListDotNetProjectsAsync(CancellationToken.None);
        var json = JsonSerializer.Serialize(result);

        var project = Assert.Single(result.Projects);
        Assert.Equal("src/Demo/Demo.csproj", project.RelativePath);
        Assert.Equal(["net10.0"], project.TargetFrameworks);
        Assert.Equal(["Example.Package"], project.PackageReferences);
        Assert.DoesNotContain("must-not-leak", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetArchitectureRulesReturnsOnlyRequestedSection()
    {
        var reader = CreateReader();

        var result = await reader.GetArchitectureRulesAsync(CancellationToken.None);

        Assert.Equal("AGENTS.md", result.Source);
        Assert.Equal(["- Keep HTTP at the boundary."], result.Rules);
        Assert.DoesNotContain(result.Rules, rule => rule.Contains("Secret", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WorkspaceToolReturnsSanitizedErrorForInvalidProject()
    {
        var projectPath = Path.Combine(_workspace, "src", "Demo", "Demo.csproj");
        File.WriteAllText(projectPath, "<Project><invalid>");
        var tools = new WorkspaceTools(CreateReader());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => tools.ListDotNetProjects(CancellationToken.None));

        Assert.Equal("The workspace project catalog could not be read safely.", exception.Message);
        Assert.Null(exception.InnerException);
        Assert.DoesNotContain(_workspace, exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListDotNetProjectsPropagatesCancellation()
    {
        var reader = CreateReader();
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => reader.ListDotNetProjectsAsync(cancellationSource.Token));
    }

    [Fact]
    public async Task ListDotNetProjectsRejectsTooManyMetadataItems()
    {
        var references = string.Join(
            Environment.NewLine,
            Enumerable.Range(0, 51)
                .Select(index => $"<PackageReference Include=\"Example.Package.{index}\" />"));
        WriteDemoProject(
            $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                {references}
              </ItemGroup>
            </Project>
            """);

        await Assert.ThrowsAsync<InvalidDataException>(
            () => CreateReader().ListDotNetProjectsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ListDotNetProjectsRejectsOversizedMetadataValue()
    {
        WriteDemoProject($"<Project Sdk=\"{new string('x', 201)}\" />");

        await Assert.ThrowsAsync<InvalidDataException>(
            () => CreateReader().ListDotNetProjectsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ListDotNetProjectsRejectsOversizedAggregateCatalog()
    {
        var projectDirectory = Path.Combine(_workspace, "src", "Demo");
        var references = string.Join(
            Environment.NewLine,
            Enumerable.Range(0, 50)
                .Select(index =>
                    $"<PackageReference Include=\"Package.{index}.{new string('x', 170)}\" />"));

        for (var index = 0; index < 30; index++)
        {
            File.WriteAllText(
                Path.Combine(projectDirectory, $"Bulk{index}.csproj"),
                $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <ItemGroup>
                    {references}
                  </ItemGroup>
                </Project>
                """);
        }

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateReader().ListDotNetProjectsAsync(CancellationToken.None));
    }

    public void Dispose()
    {
        Directory.Delete(_workspace, recursive: true);
    }

    private WorkspaceReader CreateReader() => new(new WorkspacePathPolicy(_workspace));

    private void WriteDemoProject(string content) => File.WriteAllText(
        Path.Combine(_workspace, "src", "Demo", "Demo.csproj"),
        content);
}
