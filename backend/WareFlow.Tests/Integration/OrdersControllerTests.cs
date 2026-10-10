using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace WareFlow.Tests.Integration;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            items = new[] { new { productId = 1, quantity = 1 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithValidItems_CreatesOrderAndDecrementsStock()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 10, price: 1500m);
        var productId = created.GetProperty("id").GetInt32();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            items = new[] { new { productId, quantity = 3 } },
            notes = "Orden de prueba"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<JsonElement>();
        order.GetProperty("id").GetInt32().Should().BeGreaterThan(0);
        order.GetProperty("orderNumber").GetString().Should().StartWith("ORD-");
        order.GetProperty("totalAmount").GetDecimal().Should().Be(4500m);
        order.GetProperty("status").GetInt32().Should().Be(1); // Pending

        // El stock debe haber disminuido exactamente en la cantidad pedida.
        (await client.GetStockAsync(productId)).Should().Be(7);
    }

    [Fact]
    public async Task CreateOrder_WithInsufficientStock_ReturnsBadRequest()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 2);
        var productId = created.GetProperty("id").GetInt32();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            items = new[] { new { productId, quantity = 5 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("message").GetString().Should().Contain("Stock insuficiente");

        // El stock no debe haberse tocado.
        (await client.GetStockAsync(productId)).Should().Be(2);
    }

    [Fact]
    public async Task CreateOrder_WithNonExistingProduct_ReturnsBadRequest()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            items = new[] { new { productId = 999999, quantity = 1 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("message").GetString().Should().Contain("no encontrado");
    }

    [Fact]
    public async Task GetOrderById_ForOwnOrder_ReturnsOk()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 5);
        var productId = created.GetProperty("id").GetInt32();

        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            items = new[] { new { productId, quantity = 1 } }
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = createdOrder.GetProperty("id").GetInt32();

        var response = await client.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("id").GetInt32().Should().Be(orderId);
        payload.GetProperty("items").GetArrayLength().Should().Be(1);
        payload.GetProperty("items")[0].GetProperty("productId").GetInt32().Should().Be(productId);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidTransition_UpdatesStatus()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 5);
        var productId = created.GetProperty("id").GetInt32();

        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            items = new[] { new { productId, quantity = 2 } }
        });
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = createdOrder.GetProperty("id").GetInt32();

        // Pending (1) -> Processing (2)
        var response = await client.PutAsJsonAsync($"/api/orders/{orderId}/status", new
        {
            status = 2,
            notes = "En preparación"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("status").GetInt32().Should().Be(2);

        var getResponse = await client.GetAsync($"/api/orders/{orderId}");
        var order = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        order.GetProperty("status").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithInvalidTransition_ReturnsBadRequest()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        var created = await client.CreateProductAsync(stock: 5);
        var productId = created.GetProperty("id").GetInt32();

        var createResponse = await client.PostAsJsonAsync("/api/orders", new
        {
            items = new[] { new { productId, quantity = 1 } }
        });
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = createdOrder.GetProperty("id").GetInt32();

        // Pending (1) -> Delivered (4): transición no permitida
        var response = await client.PutAsJsonAsync($"/api/orders/{orderId}/status", new
        {
            status = 4
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("message").GetString().Should().Contain("No se puede cambiar el estado");

        // El estado debe seguir siendo Pending (1).
        var getResponse = await client.GetAsync($"/api/orders/{orderId}");
        var order = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        order.GetProperty("status").GetInt32().Should().Be(1);
    }

    // ---------------- Paginación server-side (Fase B.2) ----------------

    [Fact]
    public async Task GetOrders_WithPagination_ReturnsRequestedPageSize()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        await CreateOrdersAsync(client, 3);

        var response = await client.GetAsync("/api/orders?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Fase B.2: el listado devuelve un objeto paginado, no un array plano.
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.GetProperty("page").GetInt32().Should().Be(1);
        payload.GetProperty("pageSize").GetInt32().Should().Be(2);
        payload.GetProperty("items").GetArrayLength().Should().Be(2);
        payload.GetProperty("totalItems").GetInt32().Should().BeGreaterThan(2);
        payload.GetProperty("hasPrevious").GetBoolean().Should().BeFalse();
        payload.GetProperty("hasNext").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetOrders_WithSecondPage_ReturnsNextSliceWithoutRepeats()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync();
        await CreateOrdersAsync(client, 3);

        var firstPage = await GetOrdersPayloadAsync(client, "/api/orders?page=1&pageSize=2");
        var secondPage = await GetOrdersPayloadAsync(client, "/api/orders?page=2&pageSize=2");

        var firstIds = IdsOf(firstPage);
        var secondIds = IdsOf(secondPage);

        secondPage.GetProperty("page").GetInt32().Should().Be(2);
        secondPage.GetProperty("hasPrevious").GetBoolean().Should().BeTrue();
        secondIds.Should().NotBeEmpty();
        secondIds.Intersect(firstIds).Should().BeEmpty();
    }

    // ---------------- Helpers ----------------

    /// <summary>Crea <paramref name="count"/> órdenes de 1 unidad sobre un producto nuevo.</summary>
    private static async Task CreateOrdersAsync(HttpClient client, int count)
    {
        var product = await client.CreateProductAsync(stock: count * 2);
        var productId = product.GetProperty("id").GetInt32();

        for (var i = 0; i < count; i++)
        {
            var response = await client.PostAsJsonAsync("/api/orders", new
            {
                items = new[] { new { productId, quantity = 1 } }
            });
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    private static async Task<JsonElement> GetOrdersPayloadAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static List<int> IdsOf(JsonElement payload) =>
        payload.GetProperty("items").EnumerateArray()
            .Select(o => o.GetProperty("id").GetInt32())
            .ToList();
}
