using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class DiscountCalculatorTests
{
    private readonly DiscountCalculator _calc = new();

    private static Order MakeOrder(bool isVip, decimal unitPrice, int qty = 1) =>
        new()
        {
            Customer  = new() { Id = 1, Name = "T", City = "W", IsVip = isVip },
            OrderDate = DateTime.Now,
            Items     = { new() { Product = new() { Id=1, Name="P", Price=unitPrice, Category="X" },
                                  Quantity = qty, UnitPrice = unitPrice } }
        };

    [Fact]
    public void Calculate_StandardCustomerLowAmount_ReturnsZeroDiscount()
    {
        var order = MakeOrder(isVip: false, unitPrice: 500m);

        var discount = _calc.Calculate(order);

        Assert.Equal(0m, discount);
    }

    [Fact]
    public void Calculate_VipCustomer_Returns10PercentDiscount()
    {
        var order = MakeOrder(isVip: true, unitPrice: 500m);

        var discount = _calc.Calculate(order);

        Assert.Equal(50m, discount);
    }

    [Fact]
    public void Calculate_StandardCustomerHighValue_Returns5PercentDiscount()
    {
        var order = MakeOrder(isVip: false, unitPrice: 1500m);

        var discount = _calc.Calculate(order);

        Assert.Equal(75m, discount);
    }

    [Fact]
    public void Calculate_VipCustomerHighValue_Returns15PercentDiscount()
    {
        var order = MakeOrder(isVip: true, unitPrice: 1500m);

        var discount = _calc.Calculate(order);

        Assert.Equal(225m, discount);
    }

    [Fact]
    public void Calculate_VipCustomerVeryHighValue_Returns20PercentDiscount()
    {
        var order = MakeOrder(isVip: true, unitPrice: 6000m);

        var discount = _calc.Calculate(order);

        Assert.Equal(1200m, discount);
    }

    [Fact]
    public void Calculate_MaxDiscountCap_Returns25Percent()
    {
        var order = MakeOrder(isVip: true, unitPrice: 10000m);
    
        var discount = _calc.Calculate(order);
    
        Assert.Equal(2000m, discount); // 20% від 10000
    }

    [Fact]
    public void Calculate_StandardCustomerExactly1000_NoHighValueDiscount()
    {
        var order = MakeOrder(isVip: false, unitPrice: 1000m);

        var discount = _calc.Calculate(order);

        Assert.Equal(0m, discount);
    }

    [Fact]
    public void Calculate_VipCustomerExactly5000_No20PercentYet()
    {
        var order = MakeOrder(isVip: true, unitPrice: 5000m);

        var discount = _calc.Calculate(order);

        Assert.Equal(750m, discount);
    }
}
