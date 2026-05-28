using System.Net;
using System.Text;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string         _content;
    public int CallCount { get; private set; }

    public TestHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content    = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        });
    }
}

public class CallbackHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public CallbackHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        => _handler = handler;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_handler(request));
}

public class CurrencyServiceTests
{
    private static CurrencyService MakeService(HttpStatusCode code, string json)
    {
        var handler = new TestHttpMessageHandler(code, json);
        var client  = new HttpClient(handler) { BaseAddress = new Uri("https://api.nbp.pl") };
        return new CurrencyService(client);
    }

    private static string NbpJson(decimal rate) =>
        $"{{\"table\":\"A\",\"currency\":\"dolar\",\"code\":\"USD\",\"rates\":[{{\"no\":\"1\",\"effectiveDate\":\"2026-01-01\",\"mid\":{rate.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}]}}";

    [Fact]
    public async Task GetRateAsync_ValidCurrency_ReturnsRate()
    {
        var service = MakeService(HttpStatusCode.OK, NbpJson(4.05m));

        var rate = await service.GetRateAsync("USD");

        Assert.Equal(4.05m, rate);
    }

    [Fact]
    public async Task GetRateAsync_PLN_Returns1WithoutCallingApi()
    {
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, NbpJson(1m));
        var client  = new HttpClient(handler) { BaseAddress = new Uri("https://api.nbp.pl") };
        var service = new CurrencyService(client);

        var rate = await service.GetRateAsync("PLN");

        Assert.Equal(1.0m, rate);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetRateAsync_UnknownCurrency_ReturnsNull()
    {
        var service = MakeService(HttpStatusCode.NotFound, "");

        var rate = await service.GetRateAsync("XYZ");

        Assert.Null(rate);
    }

    [Fact]
    public async Task GetRateAsync_ServerError_ThrowsCurrencyServiceException()
    {
        var service = MakeService(HttpStatusCode.InternalServerError, "");

        await Assert.ThrowsAsync<CurrencyServiceException>(
            () => service.GetRateAsync("USD"));
    }

    [Fact]
    public async Task ConvertAsync_UsdToEur_ReturnsCorrectAmount()
    {
        var usdRate = 4.0m;
        var eurRate = 4.2m;

        var mockHandler = new CallbackHttpMessageHandler(req =>
        {
            var rate = req.RequestUri!.ToString().Contains("USD") ? usdRate : eurRate;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(NbpJson(rate), Encoding.UTF8, "application/json")
            };
        });

        var client  = new HttpClient(mockHandler) { BaseAddress = new Uri("https://api.nbp.pl") };
        var service = new CurrencyService(client);

        var result = await service.ConvertAsync(100m, "USD", "EUR");

        Assert.Equal(Math.Round(100m * usdRate / eurRate, 2), result);
    }

    [Fact]
    public async Task GetRateAsync_CalledTwice_ApiCalledOnlyOnce()
    {
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, NbpJson(4.05m));
        var client  = new HttpClient(handler) { BaseAddress = new Uri("https://api.nbp.pl") };
        var service = new CurrencyService(client);

        await service.GetRateAsync("USD");
        await service.GetRateAsync("USD");

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetRateAsync_RequestGoesToCorrectUrl()
    {
        string? capturedUrl = null;
        var mockHandler = new CallbackHttpMessageHandler(req =>
        {
            capturedUrl = req.RequestUri!.PathAndQuery;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(NbpJson(4.0m), Encoding.UTF8, "application/json")
            };
        });

        var client  = new HttpClient(mockHandler) { BaseAddress = new Uri("https://api.nbp.pl") };
        var service = new CurrencyService(client);

        await service.GetRateAsync("USD");

        Assert.Contains("/api/exchangerates/rates/A/USD/", capturedUrl);
    }
}
