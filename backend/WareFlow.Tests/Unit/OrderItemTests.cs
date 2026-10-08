using FluentAssertions;
using WareFlow.Core.Models;

namespace WareFlow.Tests.Unit;

public class OrderItemTests
{
    [Fact]
    public void Subtotal_IsQuantityTimesUnitPrice()
    {
        var item = new OrderItem
        {
            Quantity = 3,
            UnitPrice = 19.99m
        };

        item.Subtotal.Should().Be(59.97m);
    }

    [Fact]
    public void Defaults_AreInitializedAsExpected()
    {
        var product = new Product();

        product.IsActive.Should().BeTrue();
        product.OrderItems.Should().BeEmpty();
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
