using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

public class OrderCurrencyConverter
{
    private readonly ICurrencyService _currencyService;

    public OrderCurrencyConverter(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    public async Task<decimal> ConvertOrderTotalAsync(Order order, string targetCurrency)
    {
        return await _currencyService.ConvertAsync(order.TotalAmount, "PLN", targetCurrency);
    }
}
