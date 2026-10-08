using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace WareFlow.Tests.Integration;

public class HealthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsOkStatusCode()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_ReturnsOkStatusPayload()
    {
        var response = await _client.GetAsync("/api/health");
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        payload.GetProperty("status").GetString().Should().Be("OK");
        payload.GetProperty("message").GetString().Should().Contain("running");
        payload.TryGetProperty("timestamp", out _).Should().BeTrue();
    }
}
