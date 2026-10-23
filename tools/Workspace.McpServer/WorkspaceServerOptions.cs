namespace Workspace.McpServer;

public sealed record WorkspaceServerOptions(string WorkspaceRoot)
{
    public static WorkspaceServerOptions Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0)
        {
            return new WorkspaceServerOptions(Environment.CurrentDirectory);
        }

        if (args.Count == 2 && args[0] == "--workspace" &&
            !string.IsNullOrWhiteSpace(args[1]))
        {
            return new WorkspaceServerOptions(args[1]);
        }

        throw new ArgumentException("Usage: Workspace.McpServer [--workspace <path>]");
    }
}
