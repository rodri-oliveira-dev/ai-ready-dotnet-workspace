using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SampleApi;

namespace SampleApi.Tests;

public sealed class ApiEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthReturnsHealthyStatus()
    {
        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
    }

    [Theory]
    [InlineData("/api/greeting", "Hello, World!")]
    [InlineData("/api/greeting?name=Ada", "Hello, Ada!")]
    public async Task GreetingReturnsExpectedMessage(string requestUri, string expectedMessage)
    {
        var response = await _client.GetAsync(requestUri);
        var body = await response.Content.ReadFromJsonAsync<GreetingResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(expectedMessage, body.Message);
    }
}
