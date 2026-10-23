using System.Text.Json;

if (args.Length != 2)
{
    return Fail("usage: validate-json.cs <path> <report-template|devcontainer>");
}

try
{
    using var document = JsonDocument.Parse(
        File.ReadAllText(args[0]),
        new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

    return args[1] switch
    {
        "report-template" => ValidateReportTemplate(document.RootElement),
        "devcontainer" => ValidateDevContainer(document.RootElement),
        _ => Fail($"unsupported validation target: {args[1]}")
    };
}
catch (Exception exception) when (exception is IOException or JsonException or InvalidOperationException)
{
    return Fail(exception.Message);
}

static int ValidateReportTemplate(JsonElement root)
{
    if (!root.TryGetProperty("status", out var status) ||
        status.ValueKind != JsonValueKind.String ||
        status.GetString() != "not_run")
    {
        return Fail("the report template top-level status must be not_run");
    }

    return 0;
}

static int ValidateDevContainer(JsonElement root)
{
    if (root.TryGetProperty("privileged", out var privileged) &&
        privileged.ValueKind != JsonValueKind.False)
    {
        return Fail("privileged mode is forbidden");
    }

    if (!root.TryGetProperty("runArgs", out var runArguments) ||
        runArguments.ValueKind != JsonValueKind.Array)
    {
        return Fail("runArgs must be an array");
    }

    string[] expectedArguments =
    [
        "--cap-drop=ALL",
        "--security-opt=no-new-privileges:true"
    ];
    var actualArguments = runArguments
        .EnumerateArray()
        .Select(argument => argument.ValueKind == JsonValueKind.String
            ? argument.GetString()
            : null)
        .ToArray();

    if (actualArguments.Length != expectedArguments.Length ||
        actualArguments.Any(argument => argument is null) ||
        !actualArguments.ToHashSet(StringComparer.Ordinal).SetEquals(expectedArguments))
    {
        return Fail("runArgs contains an argument outside the permitted isolation policy");
    }

    if (root.TryGetProperty("mounts", out var mounts) &&
        (mounts.ValueKind != JsonValueKind.Array || mounts.GetArrayLength() != 0))
    {
        return Fail("additional mounts are forbidden");
    }

    if (root.TryGetProperty("capAdd", out var addedCapabilities) &&
        (addedCapabilities.ValueKind != JsonValueKind.Array ||
         addedCapabilities.GetArrayLength() != 0))
    {
        return Fail("added capabilities are forbidden");
    }

    if (root.TryGetProperty("containerEnv", out _) ||
        root.TryGetProperty("remoteEnv", out _))
    {
        return Fail("container environment injection is forbidden");
    }

    return 0;
}

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
