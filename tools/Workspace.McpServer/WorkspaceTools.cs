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
    public ProjectCatalog ListDotNetProjects() => workspaceReader.ListDotNetProjects();

    [McpServerTool(
        Name = "get_architecture_rules",
        Title = "Get architecture rules",
        ReadOnly = true,
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        UseStructuredContent = true)]
    [Description("Returns only the architecture responsibility section from the workspace AGENTS.md.")]
    public ArchitectureRules GetArchitectureRules() => workspaceReader.GetArchitectureRules();
}
