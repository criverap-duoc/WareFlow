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
        // Fase B.2: el listado devuelve un objeto paginado, no un array plano.
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
        payload.GetProperty("items").GetArrayLength().Should().Be(0);
        payload.GetProperty("page").GetInt32().Should().Be(1);
        payload.GetProperty("pageSize").GetInt32().Should().Be(20);
        payload.GetProperty("totalItems").GetInt32().Should().Be(0);
        payload.GetProperty("totalPages").GetInt32().Should().Be(0);
        payload.GetProperty("hasNext").GetBoolean().Should().BeFalse();
        payload.GetProperty("hasPrevious").GetBoolean().Should().BeFalse();
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

        // Se filtra por SKU para que el resultado no dependa de la paginación.
        var sku = created.GetProperty("sku").GetString();
        var response = await _client.GetAsync($"/api/products?search={sku}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("totalItems").GetInt32().Should().Be(0);
        payload.GetProperty("items").EnumerateArray()
            .Select(p => p.GetProperty("id").GetInt32())
            .Should().NotContain(productId);
    }

    // ---------------- Paginación server-side (Fase B.2) ----------------

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsRequestedPage()
    {
        using var isolatedFactory = new IsolatedWebApplicationFactory();
        using var client = await isolatedFactory.CreateAuthenticatedClientAsync();
        await SeedProductsAsync(client, 12);

        var payload = await GetProductsPayloadAsync(client, "/api/products?page=1&pageSize=5");

        payload.GetProperty("page").GetInt32().Should().Be(1);
        payload.GetProperty("pageSize").GetInt32().Should().Be(5);
        payload.GetProperty("totalItems").GetInt32().Should().Be(12);
        payload.GetProperty("totalPages").GetInt32().Should().Be(3);
        payload.GetProperty("hasPrevious").GetBoolean().Should().BeFalse();
        payload.GetProperty("hasNext").GetBoolean().Should().BeTrue();
        payload.GetProperty("items").GetArrayLength().Should().Be(5);
    }

    [Fact]
    public async Task GetProducts_WithSecondPage_ReturnsNextSliceWithoutRepeats()
    {
        using var isolatedFactory = new IsolatedWebApplicationFactory();
        using var client = await isolatedFactory.CreateAuthenticatedClientAsync();
        await SeedProductsAsync(client, 12);

        var firstPage = await GetProductsPayloadAsync(client, "/api/products?page=1&pageSize=5");
        var secondPage = await GetProductsPayloadAsync(client, "/api/products?page=2&pageSize=5");

        var firstIds = IdsOf(firstPage);
        var secondIds = IdsOf(secondPage);

        secondPage.GetProperty("page").GetInt32().Should().Be(2);
        secondPage.GetProperty("hasPrevious").GetBoolean().Should().BeTrue();
        secondIds.Should().HaveCount(5);
        secondIds.Intersect(firstIds).Should().BeEmpty();
        // Orden por nombre ascendente: la segunda página arranca en el producto 06.
        secondPage.GetProperty("items")[0].GetProperty("name").GetString()
            .Should().Be("Producto paginado 06");
    }

    [Fact]
    public async Task GetProducts_WithTooLargePageSize_ClampsToOneHundred()
    {
        using var isolatedFactory = new IsolatedWebApplicationFactory();
        using var client = await isolatedFactory.CreateAuthenticatedClientAsync();
        await SeedProductsAsync(client, 3);

        var payload = await GetProductsPayloadAsync(client, "/api/products?page=1&pageSize=500");

        payload.GetProperty("pageSize").GetInt32().Should().Be(100);
        payload.GetProperty("totalItems").GetInt32().Should().Be(3);
        payload.GetProperty("totalPages").GetInt32().Should().Be(1);
        payload.GetProperty("items").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task GetProducts_WithSearch_FiltersByName()
    {
        using var isolatedFactory = new IsolatedWebApplicationFactory();
        using var client = await isolatedFactory.CreateAuthenticatedClientAsync();
        await client.CreateProductAsync(name: "Laptop gamer 14 pulgadas");
        await client.CreateProductAsync(name: "Mouse inalámbrico");
        await client.CreateProductAsync(name: "Laptop ultrabook 15 pulgadas");

        var payload = await GetProductsPayloadAsync(client, "/api/products?search=laptop");

        payload.GetProperty("totalItems").GetInt32().Should().Be(2);
        var names = payload.GetProperty("items").EnumerateArray()
            .Select(p => p.GetProperty("name").GetString())
            .ToList();
        names.Should().HaveCount(2);
        names.Should().OnlyContain(name => name!.ToLowerInvariant().Contains("laptop"));
    }

    // ---------------- Payload parcial del Bodeguero (Fase B.2) ----------------

    [Fact]
    public async Task UpdateProduct_AsBodeguero_WithStockOnly_UpdatesStockAndKeepsCatalog()
    {
        using var isolatedFactory = new IsolatedWebApplicationFactory();
        using var admin = await isolatedFactory.CreateAuthenticatedClientAsync("Admin");
        var created = await admin.CreateProductAsync(stock: 5, price: 1000m);
        var productId = created.GetProperty("id").GetInt32();

        using var bodeguero = await isolatedFactory.CreateAuthenticatedClientAsync("Bodeguero");
        var response = await bodeguero.PutAsJsonAsync($"/api/products/{productId}", new
        {
            stock = 9,
            minimumStock = 3
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("stock").GetInt32().Should().Be(9);
        payload.GetProperty("minimumStock").GetInt32().Should().Be(3);
        // Un campo ausente del payload no sobrescribe el valor actual.
        payload.GetProperty("name").GetString().Should().Be("Producto de prueba");
        payload.GetProperty("price").GetDecimal().Should().Be(1000m);
    }

    [Fact]
    public async Task UpdateProduct_AsBodeguero_ChangingCatalogField_ReturnsForbidden()
    {
        using var isolatedFactory = new IsolatedWebApplicationFactory();
        using var admin = await isolatedFactory.CreateAuthenticatedClientAsync("Admin");
        var created = await admin.CreateProductAsync(stock: 5, price: 1000m);
        var productId = created.GetProperty("id").GetInt32();

        using var bodeguero = await isolatedFactory.CreateAuthenticatedClientAsync("Bodeguero");
        var response = await bodeguero.PutAsJsonAsync($"/api/products/{productId}", new
        {
            name = "Nombre cambiado por el bodeguero",
            stock = 9
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("message").GetString().Should().Contain("Solo Admin");

        // El producto quedó intacto: ni el stock ni el nombre cambiaron.
        var product = await isolatedFactory.FindProductInDatabaseAsync(productId);
        product.Should().NotBeNull();
        product!.Stock.Should().Be(5);
        product.Name.Should().Be("Producto de prueba");
    }

    // ---------------- Helpers ----------------

    private static async Task SeedProductsAsync(HttpClient client, int count)
    {
        for (var i = 1; i <= count; i++)
        {
            await client.CreateProductAsync(
                stock: i,
                price: 1000m * i,
                name: $"Producto paginado {i:00}");
        }
    }

    private static async Task<JsonElement> GetProductsPayloadAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static List<int> IdsOf(JsonElement payload) =>
        payload.GetProperty("items").EnumerateArray()
            .Select(p => p.GetProperty("id").GetInt32())
            .ToList();
}
