namespace Workspace.McpServer;

public sealed class WorkspacePathPolicy
{
    private const int MaximumRelativePathLength = 1024;

    private static readonly HashSet<string> SensitiveSegments = new(
        [".git", ".ssh", ".nuget"],
        StringComparer.OrdinalIgnoreCase);

    public WorkspacePathPolicy(string workspaceRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);

        RootPath = Path.GetFullPath(workspaceRoot);
        if (!Directory.Exists(RootPath))
        {
            throw new DirectoryNotFoundException("The authorized workspace does not exist.");
        }

        if ((File.GetAttributes(RootPath) & FileAttributes.ReparsePoint) != 0)
        {
            throw new UnauthorizedAccessException(
                "The authorized workspace root cannot be a symbolic link or reparse point.");
        }
    }

    public string RootPath { get; }

    public string ResolveReadableFile(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The requested workspace file does not exist.");
        }

        EnsureNoReparsePoint(fullPath);
        return fullPath;
    }

    public string ResolvePath(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        if (relativePath.Length > MaximumRelativePathLength || relativePath.Any(char.IsControl))
        {
            throw new ArgumentException("The workspace-relative path is invalid.", nameof(relativePath));
        }

        if (Path.IsPathRooted(relativePath))
        {
            throw new UnauthorizedAccessException("Only workspace-relative paths are allowed.");
        }

        string candidate;
        try
        {
            candidate = Path.GetFullPath(Path.Combine(RootPath, relativePath));
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException)
        {
            throw new ArgumentException(
                "The workspace-relative path is invalid.",
                nameof(relativePath));
        }
        var normalizedRelative = Path.GetRelativePath(RootPath, candidate);

        if (normalizedRelative == ".." ||
            normalizedRelative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
            Path.IsPathRooted(normalizedRelative))
        {
            throw new UnauthorizedAccessException("The path is outside the authorized workspace.");
        }

        var segments = normalizedRelative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);

        if (segments.Any(IsSensitiveSegment))
        {
            throw new UnauthorizedAccessException("Sensitive workspace paths are not readable.");
        }

        return candidate;
    }

    public string ToWorkspaceRelativePath(string fullPath)
    {
        var relativePath = Path.GetRelativePath(RootPath, Path.GetFullPath(fullPath));
        _ = ResolvePath(relativePath);
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static bool IsSensitiveSegment(string segment) =>
        SensitiveSegments.Contains(segment) ||
        segment.Equals(".env", StringComparison.OrdinalIgnoreCase) ||
        segment.StartsWith(".env.", StringComparison.OrdinalIgnoreCase);

    private void EnsureNoReparsePoint(string fullPath)
    {
        var relativePath = Path.GetRelativePath(RootPath, fullPath);
        var currentPath = RootPath;

        foreach (var segment in relativePath.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            currentPath = Path.Combine(currentPath, segment);
            if ((File.GetAttributes(currentPath) & FileAttributes.ReparsePoint) != 0)
            {
                throw new UnauthorizedAccessException("Symbolic links are not readable.");
            }
        }
    }
}
