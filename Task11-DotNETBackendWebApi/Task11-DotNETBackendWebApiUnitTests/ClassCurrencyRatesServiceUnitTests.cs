using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Task11_DotNETBackendWebApi.Services;

namespace Task11_DotNETBackendWebApiUnitTests;

[TestClass]
public class ClassCurrencyRatesServiceUnitTests
{
    private IConfiguration _configuration = null!;

    [TestInitialize]
    public void Setup() => _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ExchangeRatesApi:BaseUrl"] = "http://mock-api.com/" }).Build();

    [DataTestMethod]
    [DataRow("USD", 43.30, 42.70)]
    [DataRow("EUR", 51.30, 50.31)]
    [DataRow("CHF", 58.30, 55.75)]
    [DataRow("usd", 43.30, 42.70)]
    public async Task Test_GetRatesAsync_PositiveCases(string currency, double saleRate, double purchaseRate)
    {
        var result = await Service(HttpStatusCode.OK, Rates()).GetRatesAsync(currency, new DateTime(2026, 3, 1));
        result.SaleRate.Should().Be((decimal)saleRate);
        result.PurchaseRate.Should().Be((decimal)purchaseRate);
    }

    [DataTestMethod]
    [DataRow("GBP")]
    [DataRow("gbp")]
    public async Task Test_GetRatesAsync_CurrencyNotFoundCase(string currency)
    {
        Func<Task> action = () => Service(HttpStatusCode.OK, Rates()).GetRatesAsync(currency, new DateTime(2026, 3, 1));
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be($"Exchange rates for '{currency}' are currently unavailable.");
    }

    [DataTestMethod]
    [DataRow("AUD")]
    [DataRow("UAH")]
    public async Task Test_GetRatesAsync_NoRateCase(string currency)
    {
        Func<Task> action = () => Service(HttpStatusCode.OK, """{"exchangeRate":[{"currency":"AUD","saleRate":0,"purchaseRate":0},{"currency":"UAH"}]}""").GetRatesAsync(currency, new DateTime(2026, 3, 1));
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be($"Exchange rates for '{currency}' are currently unavailable.");
    }

    [TestMethod]
    public async Task Test_GetRatesAsync_EmptyResponseCase()
    {
        Func<Task> action = () => Service(HttpStatusCode.OK, """{"exchangeRate":[]}""").GetRatesAsync("USD", new DateTime(2026, 3, 1));
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("No exchange rates available for this date: 01.03.2026");
    }

    [TestMethod]
    public async Task Test_GetRatesAsync_HttpRequestExceptionCase()
    {
        Func<Task> action = () => Service(HttpStatusCode.InternalServerError, "").GetRatesAsync("USD", new DateTime(2026, 3, 1));
        (await action.Should().ThrowAsync<HttpRequestException>()).Which.Message.Should().Be("External bank service is unavailable. Please try again later.");
    }

    [TestMethod]
    public async Task Test_GetRatesAsync_BrokenResponseCase()
    {
        Func<Task> action = () => Service(HttpStatusCode.OK, "{broken").GetRatesAsync("USD", new DateTime(2026, 3, 1));
        (await action.Should().ThrowAsync<ValidationException>()).Which.Message.Should().Be("Received invalid data from the bank. Please try again later.");
    }

    private CurrencyRatesService Service(HttpStatusCode status, string json) => new(new HttpClient(new Handler(new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") })), _configuration, Mock.Of<ILogger<CurrencyRatesService>>());
    private static string Rates() => """{"exchangeRate":[{"currency":"USD","saleRate":43.3,"purchaseRate":42.704},{"currency":"EUR","saleRate":51.3,"purchaseRate":50.305},{"currency":"CHF","saleRate":58.3,"purchaseRate":55.7548}]}""";
    private sealed class Handler(HttpResponseMessage response) : HttpMessageHandler { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(response); }
}
