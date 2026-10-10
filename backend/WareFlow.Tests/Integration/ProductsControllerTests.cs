using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace WareFlow.Tests.Integration;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_WithoutAuth_ReturnsOkWithEmptyList()
    {
        // Factory aislada: almacén InMemory nuevo, sin datos de otras clases de test.
        using var isolatedFactory = new IsolatedWebApplicationFactory();
        var client = isolatedFactory.CreateClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.ValueKind.Should().Be(JsonValueKind.Array);
        payload.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Sin auth",
            description = "No debería crearse",
            sku = TestHelpers.UniqueSku("WF"),
            price = 1000m,
            stock = 5,
            minimumStock = 0,
            category = "Testing"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreated()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var sku = TestHelpers.UniqueSku("WF");

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            name = "Teclado mecánico",
            description = "Producto de prueba",
            sku,
            price = 49990m,
            stock = 7,
            minimumStock = 2,
            category = "Teclados"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("id").GetInt32().Should().BeGreaterThan(0);
        payload.GetProperty("sku").GetString().Should().Be(sku);
        payload.GetProperty("stock").GetInt32().Should().Be(7);
        payload.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_ReturnsBadRequest()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var sku = TestHelpers.UniqueSku("WF");

        var body = new
        {
            name = "Producto duplicado",
            description = "Mismo SKU",
            sku,
            price = 1000m,
            stock = 1,
            minimumStock = 0,
            category = "Testing"
        };

        var first = await client.PostAsJsonAsync("/api/products", body);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/products", body);

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await second.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("message").GetString().Should().Contain("SKU");
    }

    [Fact]
    public async Task CreateProduct_WithNonPositivePrice_ReturnsBadRequest()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            name = "Producto sin precio",
            description = "Precio inválido",
            sku = TestHelpers.UniqueSku("WF"),
            price = 0m,
            stock = 3,
            minimumStock = 0,
            category = "Testing"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ReturnsOk()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 4, price: 2500m);
        var productId = created.GetProperty("id").GetInt32();

        var response = await _client.GetAsync($"/api/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("id").GetInt32().Should().Be(productId);
        payload.GetProperty("price").GetDecimal().Should().Be(2500m);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/products/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("message").GetString().Should().Contain("no encontrado");
    }

    [Fact]
    public async Task UpdateProduct_WithAuth_UpdatesProduct()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 5, price: 1000m);
        var productId = created.GetProperty("id").GetInt32();

        var response = await client.PutAsJsonAsync($"/api/products/{productId}", new
        {
            name = "Nombre actualizado",
            description = "Descripción actualizada",
            price = 1990m,
            minimumStock = 3,
            category = "Actualizados"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/products/{productId}");
        var payload = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("name").GetString().Should().Be("Nombre actualizado");
        payload.GetProperty("price").GetDecimal().Should().Be(1990m);
        payload.GetProperty("minimumStock").GetInt32().Should().Be(3);
        payload.GetProperty("updatedAt").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task DeleteProduct_WithAuth_PerformsSoftDelete()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 2);
        var productId = created.GetProperty("id").GetInt32();

        var response = await client.DeleteAsync($"/api/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await _factory.FindProductInDatabaseAsync(productId);
        product.Should().NotBeNull();
        product!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetProducts_DoesNotReturnInactiveProducts()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 2);
        var productId = created.GetProperty("id").GetInt32();

        var deleteResponse = await client.DeleteAsync($"/api/products/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.EnumerateArray()
            .Select(p => p.GetProperty("id").GetInt32())
            .Should().NotContain(productId);
    }
}
