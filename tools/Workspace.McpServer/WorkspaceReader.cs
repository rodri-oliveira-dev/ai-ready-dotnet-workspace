using System.Xml;
using System.Xml.Linq;

namespace Workspace.McpServer;

public sealed class WorkspaceReader(WorkspacePathPolicy pathPolicy)
{
    private const int MaximumProjects = 200;
    private const int MaximumMetadataItems = 50;
    private const int MaximumMetadataValueLength = 200;
    private const long MaximumCatalogCharacters = 256 * 1024;
    private const long MaximumInstructionBytes = 128 * 1024;
    private const long MaximumProjectBytes = 256 * 1024;

    public async Task<ProjectCatalog> ListDotNetProjectsAsync(
        CancellationToken cancellationToken)
    {
        var enumerationOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            AttributesToSkip = FileAttributes.Hidden | FileAttributes.System |
                               FileAttributes.ReparsePoint,
            MatchCasing = MatchCasing.PlatformDefault
        };

        var projectPaths = new List<string>(MaximumProjects);
        foreach (var projectPath in Directory.EnumerateFiles(
                     pathPolicy.RootPath,
                     "*.csproj",
                     enumerationOptions))
        {
            cancellationToken.ThrowIfCancellationRequested();
            projectPaths.Add(projectPath);
            if (projectPaths.Count > MaximumProjects)
            {
                throw new InvalidOperationException(
                    $"The workspace contains more than {MaximumProjects} projects.");
            }
        }

        long catalogCharacters = 0;
        var projects = new List<ProjectInfo>(projectPaths.Count);
        foreach (var projectPath in projectPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var project = await ReadProjectAsync(projectPath, cancellationToken);
            catalogCharacters = checked(catalogCharacters + MeasureCharacters(project));
            if (catalogCharacters > MaximumCatalogCharacters)
            {
                throw new InvalidOperationException("The workspace project catalog is too large.");
            }

            projects.Add(project);
        }

        var orderedProjects = projects
            .OrderBy(project => project.RelativePath, StringComparer.Ordinal)
            .ToArray();

        return new ProjectCatalog(orderedProjects);
    }

    public async Task<ArchitectureRules> GetArchitectureRulesAsync(
        CancellationToken cancellationToken)
    {
        var agentsPath = pathPolicy.ResolveReadableFile("AGENTS.md");
        if (new FileInfo(agentsPath).Length > MaximumInstructionBytes)
        {
            throw new InvalidOperationException("AGENTS.md exceeds the safe read limit.");
        }

        var rules = new List<string>();
        var inSection = false;

        await using var stream = new FileStream(
            agentsPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
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

    private async Task<ProjectInfo> ReadProjectAsync(
        string projectPath,
        CancellationToken cancellationToken)
    {
        var safeProjectPath = pathPolicy.ResolveReadableFile(
            pathPolicy.ToWorkspaceRelativePath(projectPath));
        if (new FileInfo(safeProjectPath).Length > MaximumProjectBytes)
        {
            throw new InvalidDataException("The project file exceeds the safe read limit.");
        }

        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = MaximumProjectBytes
        };

        using var xmlReader = XmlReader.Create(safeProjectPath, settings);
        var document = await XDocument.LoadAsync(
            xmlReader,
            LoadOptions.None,
            cancellationToken);
        var root = document.Root ?? throw new InvalidDataException("Project XML has no root element.");
        var sdk = root.Attribute("Sdk")?.Value ?? string.Empty;
        EnsureMetadataValueIsBounded(sdk);

        return new ProjectInfo(
            Path.GetFileNameWithoutExtension(safeProjectPath),
            pathPolicy.ToWorkspaceRelativePath(safeProjectPath),
            sdk,
            ReadValues(document, "TargetFramework", "TargetFrameworks"),
            ReadIncludes(document, "PackageReference"),
            ReadIncludes(document, "ProjectReference")
                .Select(Path.GetFileName)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Cast<string>()
                .ToArray());
    }

    private static string[] ReadValues(XDocument document, params string[] names) =>
        ReadBoundedMetadata(document
            .Descendants()
            .Where(element => names.Contains(element.Name.LocalName, StringComparer.Ordinal))
            .SelectMany(element => element.Value.Split(';', StringSplitOptions.RemoveEmptyEntries |
                                                               StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal), StringComparer.Ordinal);

    private static string[] ReadIncludes(XDocument document, string itemName) =>
        ReadBoundedMetadata(document
            .Descendants()
            .Where(element => element.Name.LocalName == itemName)
            .Select(element => element.Attribute("Include")?.Value ??
                               element.Attribute("Update")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

    private static string[] ReadBoundedMetadata(
        IEnumerable<string> values,
        StringComparer comparer)
    {
        var boundedValues = values.Take(MaximumMetadataItems + 1).ToArray();
        if (boundedValues.Length > MaximumMetadataItems)
        {
            throw new InvalidDataException("Project metadata contains too many values.");
        }

        foreach (var value in boundedValues)
        {
            EnsureMetadataValueIsBounded(value);
        }

        return boundedValues.Order(comparer).ToArray();
    }

    private static void EnsureMetadataValueIsBounded(string value)
    {
        if (value.Length > MaximumMetadataValueLength)
        {
            throw new InvalidDataException("Project metadata exceeds the safe value limit.");
        }
    }

    private static long MeasureCharacters(ProjectInfo project) =>
        project.Name.Length +
        project.RelativePath.Length +
        project.Sdk.Length +
        project.TargetFrameworks.Sum(value => value.Length) +
        project.PackageReferences.Sum(value => value.Length) +
        project.ProjectReferences.Sum(value => value.Length);
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
