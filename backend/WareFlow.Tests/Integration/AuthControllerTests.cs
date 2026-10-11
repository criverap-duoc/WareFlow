using System.Net;
using System.Net.Http.Headers;
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

    /// <summary>
    /// Registra un usuario nuevo y hace login, devolviendo el par de tokens
    /// (accessToken de 15 min + refreshToken de 7 días) que emite la API.
    /// </summary>
    private async Task<(string AccessToken, string RefreshToken)> RegisterAndLoginAsync(string email)
    {
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
        return (
            payload.GetProperty("accessToken").GetString()!,
            payload.GetProperty("refreshToken").GetString()!);
    }

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
        var (accessToken, _) = await RegisterAndLoginAsync(UniqueEmail());

        accessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessAndRefreshToken()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync(UniqueEmail());

        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var (_, refreshToken) = await RegisterAndLoginAsync(UniqueEmail());

        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        // La rotación emite un refresh token nuevo y distinto del anterior
        payload.GetProperty("refreshToken").GetString().Should().NotBe(refreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = "token-que-no-existe"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithAlreadyRotatedToken_ReturnsUnauthorized()
    {
        var (_, refreshToken) = await RegisterAndLoginAsync(UniqueEmail());

        var first = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        // El token ya fue rotado (revocado): reutilizarlo debe fallar
        var second = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

        second.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesRefreshTokens()
    {
        var (accessToken, refreshToken) = await RegisterAndLoginAsync(UniqueEmail());

        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var logoutResponse = await _client.SendAsync(logoutRequest);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Tras el logout el refresh token queda revocado y no sirve para renovar
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var email = UniqueEmail();
        var payload = new { firstName = "Test", lastName = "User", email, password = ValidPassword };

        var first = await _client.PostAsJsonAsync("/api/auth/register", payload);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync("/api/auth/register", payload);

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMissingFields_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithNonExistingUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = UniqueEmail(),
            password = ValidPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("message").GetString().Should().Contain("Credenciales inválidas");
    }
}
