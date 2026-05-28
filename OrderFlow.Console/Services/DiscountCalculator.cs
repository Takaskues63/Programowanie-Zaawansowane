using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class DiscountCalculator
{
    private const decimal VipDiscountRate       = 0.10m;
    private const decimal HighValueDiscountRate = 0.05m;
    private const decimal VipHighValueRate      = 0.05m;
    private const decimal HighValueThreshold    = 1000m;
    private const decimal VipHighValueThreshold = 5000m;
    private const decimal MaxDiscountRate       = 0.25m;

    public decimal Calculate(Order order)
    {
        var total = order.TotalAmount;
        var rate  = ComputeRate(order, total);
        return Math.Round(total * Math.Min(rate, MaxDiscountRate), 2);
    }

    private decimal ComputeRate(Order order, decimal total)
    {
        var rate = 0m;

        if (order.Customer.IsVip)
            rate += VipDiscountRate;

        if (total > HighValueThreshold)
            rate += HighValueDiscountRate;

        if (order.Customer.IsVip && total > VipHighValueThreshold)
            rate += VipHighValueRate;

        return rate;
    }
}
