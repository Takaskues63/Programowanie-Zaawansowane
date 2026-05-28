using Moq;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class OrderCurrencyConverterTests
{
    private static Order MakeOrder(decimal unitPrice) => new()
    {
        Customer  = new() { Id = 1, Name = "T", City = "W", IsVip = false },
        OrderDate = DateTime.Now,
        Items     = { new() { Product = new() { Id=1, Name="P", Price=unitPrice, Category="X" },
                              Quantity = 1, UnitPrice = unitPrice } }
    };

    [Fact]
    public async Task ConvertOrderTotalAsync_CallsConvertWithCorrectArguments()
    {
        var mock      = new Mock<ICurrencyService>();
        mock.Setup(s => s.ConvertAsync(500m, "PLN", "USD")).ReturnsAsync(125m);
        var converter = new OrderCurrencyConverter(mock.Object);
        var order     = MakeOrder(500m);

        var result = await converter.ConvertOrderTotalAsync(order, "USD");

        Assert.Equal(125m, result);
        mock.Verify(s => s.ConvertAsync(500m, "PLN", "USD"), Times.Once);
    }

    [Fact]
    public async Task ConvertOrderTotalAsync_DifferentCurrency_UsesCorrectTarget()
    {
        var mock      = new Mock<ICurrencyService>();
        mock.Setup(s => s.ConvertAsync(1000m, "PLN", "EUR")).ReturnsAsync(238m);
        var converter = new OrderCurrencyConverter(mock.Object);
        var order     = MakeOrder(1000m);

        var result = await converter.ConvertOrderTotalAsync(order, "EUR");

        Assert.Equal(238m, result);
    }
}
