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
    public void ListDotNetProjectsReturnsSelectedMetadataOnly()
    {
        var reader = CreateReader();

        var result = reader.ListDotNetProjects();
        var json = JsonSerializer.Serialize(result);

        var project = Assert.Single(result.Projects);
        Assert.Equal("src/Demo/Demo.csproj", project.RelativePath);
        Assert.Equal(["net10.0"], project.TargetFrameworks);
        Assert.Equal(["Example.Package"], project.PackageReferences);
        Assert.DoesNotContain("must-not-leak", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Password", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetArchitectureRulesReturnsOnlyRequestedSection()
    {
        var reader = CreateReader();

        var result = reader.GetArchitectureRules();

        Assert.Equal("AGENTS.md", result.Source);
        Assert.Equal(["- Keep HTTP at the boundary."], result.Rules);
        Assert.DoesNotContain(result.Rules, rule => rule.Contains("Secret", StringComparison.Ordinal));
    }

    [Fact]
    public void WorkspaceToolReturnsSanitizedErrorForInvalidProject()
    {
        var projectPath = Path.Combine(_workspace, "src", "Demo", "Demo.csproj");
        File.WriteAllText(projectPath, "<Project><invalid>");
        var tools = new WorkspaceTools(CreateReader());

        var exception = Assert.Throws<InvalidOperationException>(tools.ListDotNetProjects);

        Assert.Equal("The workspace project catalog could not be read safely.", exception.Message);
        Assert.Null(exception.InnerException);
        Assert.DoesNotContain(_workspace, exception.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        Directory.Delete(_workspace, recursive: true);
    }

    private WorkspaceReader CreateReader() => new(new WorkspacePathPolicy(_workspace));
}
