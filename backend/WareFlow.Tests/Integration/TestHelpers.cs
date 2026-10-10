using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WareFlow.Core.Models;
using WareFlow.Infrastructure.Data;

namespace WareFlow.Tests.Integration;

/// <summary>
/// Utilidades compartidas por los tests de integración.
/// La base InMemory se comparte entre clases de test, por eso los emails y SKU
/// se generan con un Guid para evitar colisiones entre tests.
/// </summary>
internal static class TestHelpers
{
    public const string ValidPassword = "Test123!";

    public static string UniqueEmail() => $"user_{Guid.NewGuid():N}@wareflow.test";

    public static string UniqueSku(string prefix) => $"{prefix}-{Guid.NewGuid():N}".ToUpperInvariant();

    /// <summary>
    /// Registra un usuario nuevo, hace login y devuelve un HttpClient con el
    /// header Authorization (Bearer token) ya configurado.
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(this CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var email = UniqueEmail();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = ValidPassword
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = ValidPassword
        });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = payload.GetProperty("token").GetString();
        token.Should().NotBeNullOrEmpty();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>Crea un producto de prueba (requiere un cliente autenticado) y devuelve el JSON creado.</summary>
    public static async Task<JsonElement> CreateProductAsync(
        this HttpClient client,
        int stock = 10,
        decimal price = 1000m,
        string? sku = null,
        string name = "Producto de prueba")
    {
        var response = await client.PostAsJsonAsync("/api/products", new
        {
            name,
            description = "Creado por tests de integración",
            sku = sku ?? UniqueSku("WF"),
            price,
            stock,
            minimumStock = 0,
            category = "Testing"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    /// <summary>Lee el stock actual de un producto vía API (endpoint público).</summary>
    public static async Task<int> GetStockAsync(this HttpClient client, int productId)
    {
        var response = await client.GetAsync($"/api/products/{productId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("stock").GetInt32();
    }

    /// <summary>
    /// Consulta el producto directamente en el DbContext de la factory, para
    /// verificar cambios que la API no expone (p. ej. el soft delete).
    /// </summary>
    public static async Task<Product?> FindProductInDatabaseAsync(
        this CustomWebApplicationFactory factory,
        int productId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
    }
}
