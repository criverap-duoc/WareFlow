using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace WareFlow.Tests.Integration;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string ValidPassword = "Test123!";

    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Email único por test para evitar colisiones en la base InMemory compartida.
    private static string UniqueEmail() => $"user_{Guid.NewGuid():N}@wareflow.test";

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        var email = UniqueEmail();

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = ValidPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("email").GetString().Should().Be(email);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email = UniqueEmail(),
            password = "weak"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var email = UniqueEmail();

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = ValidPassword
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = ValidPassword
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = UniqueEmail();

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = ValidPassword
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Wrong123!"
        });

        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
