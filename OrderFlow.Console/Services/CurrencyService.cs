using System.Text.Json;

namespace OrderFlow.Console.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, decimal> _cache = new();

    public CurrencyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal?> GetRateAsync(string currencyCode)
    {
        if (currencyCode.Equals("PLN", StringComparison.OrdinalIgnoreCase))
            return 1.0m;

        if (_cache.TryGetValue(currencyCode.ToUpper(), out var cached))
            return cached;

        var url      = $"/api/exchangerates/rates/A/{currencyCode}/?format=json";
        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new CurrencyServiceException(
                $"NBP API error: {(int)response.StatusCode} {response.ReasonPhrase}");

        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json);
        var rate = doc.RootElement
            .GetProperty("rates")[0]
            .GetProperty("mid")
            .GetDecimal();

        _cache[currencyCode.ToUpper()] = rate;
        return rate;
    }

    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        var fromRate = await GetRateAsync(fromCurrency)
            ?? throw new CurrencyServiceException($"Nieznana waluta: {fromCurrency}");
        var toRate = await GetRateAsync(toCurrency)
            ?? throw new CurrencyServiceException($"Nieznana waluta: {toCurrency}");

        return Math.Round(amount * fromRate / toRate, 2);
    }
}
