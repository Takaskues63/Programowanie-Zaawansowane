using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class OrderValidatorTests
{
    private readonly OrderValidator _validator = new();

    private static Order MakeOrder(
        int id = 1,
        OrderStatus status = OrderStatus.New,
        DateTime? date = null,
        List<OrderItem>? items = null)
    {
        var customer = new Customer { Id = 1, Name = "Test", City = "Warszawa", IsVip = false };
        return new Order
        {
            Id        = id,
            Customer  = customer,
            OrderDate = date ?? DateTime.Now.AddDays(-1),
            Status    = status,
            Items     = items ?? new List<OrderItem>
            {
                new() { Product = new Product { Id = 1, Name = "P", Price = 100m, Category = "X" },
                        Quantity = 1, UnitPrice = 100m }
            }
        };
    }

    [Fact]
    public void HasItems_OrderWithNoItems_ReturnsError()
    {
        var order = MakeOrder(items: new List<OrderItem>());

        var result = _validator.ValidateAll(order, out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains("pozycji"));
    }

    [Fact]
    public void AmountWithinLimit_OrderExceedsLimit_ReturnsError()
    {
        var items = new List<OrderItem>
        {
            new() { Product = new Product { Id = 1, Name = "Drogi", Price = 25000m, Category = "X" },
                    Quantity = 1, UnitPrice = 25000m }
        };
        var order = MakeOrder(items: items);

        var result = _validator.ValidateAll(order, out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains("limit"));
    }

    [Fact]
    public void PositiveQuantities_ZeroQuantity_ReturnsError()
    {
        var items = new List<OrderItem>
        {
            new() { Product = new Product { Id = 1, Name = "P", Price = 100m, Category = "X" },
                    Quantity = 0, UnitPrice = 100m }
        };
        var order = MakeOrder(items: items);

        var result = _validator.ValidateAll(order, out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains("ilość"));
    }

    [Fact]
    public void FutureDate_OrderDateInFuture_ReturnsError()
    {
        var order = MakeOrder(date: DateTime.Now.AddDays(1));

        var result = _validator.ValidateAll(order, out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains("przyszłości"));
    }

    [Fact]
    public void CancelledStatus_OrderIsCancelled_ReturnsError()
    {
        var order = MakeOrder(status: OrderStatus.Cancelled);

        var result = _validator.ValidateAll(order, out var errors);

        Assert.False(result);
        Assert.Contains(errors, e => e.Contains("anulowane"));
    }

    [Fact]
    public void ValidateAll_ValidOrder_ReturnsTrue()
    {
        var order = MakeOrder();

        var result = _validator.ValidateAll(order, out var errors);

        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAll_MultipleViolations_ReturnsAllErrors()
    {
        var order = MakeOrder(
            status: OrderStatus.Cancelled,
            date: DateTime.Now.AddDays(1),
            items: new List<OrderItem>());

        var result = _validator.ValidateAll(order, out var errors);

        Assert.False(result);
        Assert.True(errors.Count >= 3);
    }

    [Theory]
    [InlineData(OrderStatus.New,        true)]
    [InlineData(OrderStatus.Validated,  true)]
    [InlineData(OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Completed,  true)]
    [InlineData(OrderStatus.Cancelled,  false)]
    public void ValidateAll_VariousStatuses_ReturnsExpected(OrderStatus status, bool expected)
    {
        var order = MakeOrder(status: status);

        var result = _validator.ValidateAll(order, out _);

        Assert.Equal(expected, result);
    }
}
