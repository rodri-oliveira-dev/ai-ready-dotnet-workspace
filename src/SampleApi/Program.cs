using SampleApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new HealthResponse("Healthy")))
    .WithName("Health");

app.MapGet("/api/greeting", (string? name) =>
    {
        var recipient = string.IsNullOrWhiteSpace(name) ? "World" : name.Trim();
        return Results.Ok(new GreetingResponse($"Hello, {recipient}!"));
    })
    .WithName("Greeting");

app.Run();

public partial class Program;
