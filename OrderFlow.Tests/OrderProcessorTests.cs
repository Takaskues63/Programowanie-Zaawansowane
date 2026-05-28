using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class OrderProcessorTests
{
    private static List<Order> MakeOrders() => new()
    {
        new() { Id = 1, Customer = new() { Id = 1, Name = "VIP",    City = "W", IsVip = true  },
                OrderDate = DateTime.Now, Status = OrderStatus.New,
                Items = { new() { Product = new() { Id=1, Name="A", Price=500m,  Category="X" },
                                  Quantity = 1, UnitPrice = 500m } } },
        new() { Id = 2, Customer = new() { Id = 2, Name = "Normal", City = "K", IsVip = false },
                OrderDate = DateTime.Now, Status = OrderStatus.New,
                Items = { new() { Product = new() { Id=2, Name="B", Price=2000m, Category="Y" },
                                  Quantity = 1, UnitPrice = 2000m } } },
        new() { Id = 3, Customer = new() { Id = 3, Name = "Done",   City = "P", IsVip = false },
                OrderDate = DateTime.Now, Status = OrderStatus.Completed,
                Items = { new() { Product = new() { Id=3, Name="C", Price=300m,  Category="X" },
                                  Quantity = 2, UnitPrice = 300m } } },
    };

    [Fact]
    public void FilterOrders_VipPredicate_ReturnsOnlyVip()
    {
        var processor = new OrderProcessor(MakeOrders());

        var result = processor.FilterOrders(o => o.Customer.IsVip);

        Assert.Single(result);
        Assert.All(result, o => Assert.True(o.Customer.IsVip));
    }

    [Fact]
    public void AggregateOrders_SumFunc_ReturnsTotalAmount()
    {
        var processor = new OrderProcessor(MakeOrders());

        var result = processor.AggregateOrders(list => list.Sum(o => o.TotalAmount));

        Assert.Equal(500m + 2000m + 600m, result);
    }
}
