using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Workspace.McpServer;

var options = WorkspaceServerOptions.Parse(args);
var builder = Host.CreateApplicationBuilder();

builder.Logging.AddConsole(consoleOptions =>
{
    consoleOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(new WorkspacePathPolicy(options.WorkspaceRoot));
builder.Services.AddSingleton<WorkspaceReader>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
