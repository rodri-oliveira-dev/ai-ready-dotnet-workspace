using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Workspace.McpServer;

[McpServerToolType]
public sealed class WorkspaceTools(WorkspaceReader workspaceReader)
{
    [McpServerTool(
        Name = "list_dotnet_projects",
        Title = "List .NET projects",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Lists sanitized .NET project metadata inside the authorized workspace.")]
    public Task<ProjectCatalog> ListDotNetProjects(CancellationToken cancellationToken) =>
        ExecuteSafelyAsync(
            workspaceReader.ListDotNetProjectsAsync,
            "The workspace project catalog could not be read safely.",
            cancellationToken);

    [McpServerTool(
        Name = "get_architecture_rules",
        Title = "Get architecture rules",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Returns only the architecture responsibility section from the workspace AGENTS.md.")]
    public Task<ArchitectureRules> GetArchitectureRules(CancellationToken cancellationToken) =>
        ExecuteSafelyAsync(
            workspaceReader.GetArchitectureRulesAsync,
            "The workspace architecture rules could not be read safely.",
            cancellationToken);

    private static async Task<TResult> ExecuteSafelyAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        string safeErrorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            return await operation(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new InvalidOperationException(safeErrorMessage);
        }
    }
}
