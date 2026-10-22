using System.Xml;
using System.Xml.Linq;

namespace Workspace.McpServer;

public sealed class WorkspaceReader(WorkspacePathPolicy pathPolicy)
{
    private const int MaximumProjects = 200;
    private const long MaximumInstructionBytes = 128 * 1024;

    public ProjectCatalog ListDotNetProjects()
    {
        var enumerationOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System |
                               FileAttributes.ReparsePoint,
            MatchCasing = MatchCasing.PlatformDefault
        };

        var projectPaths = Directory
            .EnumerateFiles(pathPolicy.RootPath, "*.csproj", enumerationOptions)
            .Take(MaximumProjects + 1)
            .ToArray();

        if (projectPaths.Length > MaximumProjects)
        {
            throw new InvalidOperationException(
                $"The workspace contains more than {MaximumProjects} projects.");
        }

        var projects = projectPaths
            .Select(ReadProject)
            .OrderBy(project => project.RelativePath, StringComparer.Ordinal)
            .ToArray();

        return new ProjectCatalog(projects);
    }

    public ArchitectureRules GetArchitectureRules()
    {
        var agentsPath = pathPolicy.ResolveReadableFile("AGENTS.md");
        if (new FileInfo(agentsPath).Length > MaximumInstructionBytes)
        {
            throw new InvalidOperationException("AGENTS.md exceeds the safe read limit.");
        }

        var rules = new List<string>();
        var inSection = false;

        foreach (var line in File.ReadLines(agentsPath))
        {
            if (line.Equals("## Layer responsibilities", StringComparison.Ordinal))
            {
                inSection = true;
                continue;
            }

            if (inSection && line.StartsWith("## ", StringComparison.Ordinal))
            {
                break;
            }

            if (inSection && !string.IsNullOrWhiteSpace(line))
            {
                rules.Add(line.Trim());
            }
        }

        return new ArchitectureRules("AGENTS.md", rules);
    }

    private ProjectInfo ReadProject(string projectPath)
    {
        var safeProjectPath = pathPolicy.ResolveReadableFile(
            pathPolicy.ToWorkspaceRelativePath(projectPath));
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 1024 * 1024
        };

        using var xmlReader = XmlReader.Create(safeProjectPath, settings);
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        var root = document.Root ?? throw new InvalidDataException("Project XML has no root element.");

        return new ProjectInfo(
            Path.GetFileNameWithoutExtension(safeProjectPath),
            pathPolicy.ToWorkspaceRelativePath(safeProjectPath),
            root.Attribute("Sdk")?.Value ?? string.Empty,
            ReadValues(document, "TargetFramework", "TargetFrameworks"),
            ReadIncludes(document, "PackageReference"),
            ReadIncludes(document, "ProjectReference")
                .Select(Path.GetFileName)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Cast<string>()
                .ToArray());
    }

    private static string[] ReadValues(XDocument document, params string[] names) =>
        document
            .Descendants()
            .Where(element => names.Contains(element.Name.LocalName, StringComparer.Ordinal))
            .SelectMany(element => element.Value.Split(';', StringSplitOptions.RemoveEmptyEntries |
                                                               StringSplitOptions.TrimEntries))
            .Where(value => value.Length <= 100)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string[] ReadIncludes(XDocument document, string itemName) =>
        document
            .Descendants()
            .Where(element => element.Name.LocalName == itemName)
            .Select(element => element.Attribute("Include")?.Value ??
                               element.Attribute("Update")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value) && value.Length <= 200)
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}

public sealed record ProjectCatalog(IReadOnlyList<ProjectInfo> Projects);

public sealed record ProjectInfo(
    string Name,
    string RelativePath,
    string Sdk,
    IReadOnlyList<string> TargetFrameworks,
    IReadOnlyList<string> PackageReferences,
    IReadOnlyList<string> ProjectReferences);

public sealed record ArchitectureRules(string Source, IReadOnlyList<string> Rules);
