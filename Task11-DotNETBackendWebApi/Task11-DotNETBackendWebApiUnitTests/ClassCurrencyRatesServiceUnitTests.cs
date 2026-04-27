using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Task11_DotNETBackendWebApi.Services;

namespace Task11_DotNETBackendWebApiUnitTests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}

[TestClass]
public class ClassCurrencyRatesServiceUnitTests
{
    private IConfiguration _configuration;
    private Mock<ILogger<CurrencyRatesService>> _loggerMock;

    [TestInitialize]
    public void Setup()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExchangeRatesApi:BaseUrl"] = "http://mock-api.com/"
            })
            .Build();

        _loggerMock = new Mock<ILogger<CurrencyRatesService>>();
    }

    [DataTestMethod]
    [DataRow("USD", 43.30, 42.70)]
    [DataRow("EUR", 51.30, 50.31)]
    [DataRow("CHF", 58.30, 55.75)]
    [DataRow("usd", 43.30, 42.70)]
    [DataRow("eur", 51.30, 50.31)]
    [DataRow("chf", 58.30, 55.75)]
    [DataRow("Usd", 43.30, 42.70)]
    [DataRow("Eur", 51.30, 50.31)]
    [DataRow("Chf", 58.30, 55.75)]
    public async Task Test_GetRatesAsync_PositiveCases(string currency, double saleRate, double purchaseRate)
    {
        var mockJson = """
        {
        "date": "01.03.2026",
        "bank": "PB",
        "baseCurrency": 980,
        "baseCurrencyLit": "UAH",
        "exchangeRate": [    
                { "baseCurrency": "UAH", "currency": "USD", "saleRateNB": 43.2081000, "purchaseRateNB": 43.2081000, "saleRate": 43.3000000, "purchaseRate": 42.7040000 },
                { "baseCurrency": "UAH", "currency": "EUR", "saleRateNB": 51.0244000, "purchaseRateNB": 51.0244000, "saleRate": 51.3000000, "purchaseRate": 50.3050000 },
                { "baseCurrency": "UAH", "currency": "CHF", "saleRateNB": 55.8028000, "purchaseRateNB": 55.8028000, "saleRate": 58.3000000, "purchaseRate": 55.7548000 }
            ]
        }
        """;

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
        };

        var mockHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHandler);
        var service = new CurrencyRatesService(httpClient, _configuration, _loggerMock.Object);

        var successResult = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));

        successResult.Data.SaleRate.Should().Be((decimal)saleRate);
        successResult.Data.PurchaseRate.Should().Be((decimal)purchaseRate);
    }

    [DataTestMethod]
    [DataRow("GBP")]
    [DataRow("gbp")]
    [DataRow("Gbp")]
    public async Task Test_GetRatesAsync_CurrencyNotFoundCase(string currency)
    {
        var mockJson = """
        {
        "date": "01.03.2026",
        "bank": "PB",
        "baseCurrency": 980,
        "baseCurrencyLit": "UAH",
        "exchangeRate": [    
                { "baseCurrency": "UAH", "currency": "USD", "saleRateNB": 43.2081000, "purchaseRateNB": 43.2081000, "saleRate": 43.3000000, "purchaseRate": 42.7040000 },
                { "baseCurrency": "UAH", "currency": "EUR", "saleRateNB": 51.0244000, "purchaseRateNB": 51.0244000, "saleRate": 51.3000000, "purchaseRate": 50.3050000 },
                { "baseCurrency": "UAH", "currency": "CHF", "saleRateNB": 55.8028000, "purchaseRateNB": 55.8028000, "saleRate": 58.3000000, "purchaseRate": 55.7548000 }
            ]
        }
        """;

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
        };

        var mockHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHandler);
        var service = new CurrencyRatesService(httpClient, _configuration, _loggerMock.Object);

        var failureResult = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));
        failureResult.IsSuccess.Should().BeFalse();

        var failureMessage = $"Exchange rates for '{currency}' are currently unavailable.";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [DataTestMethod]
    [DataRow("AUD")]
    [DataRow("UAH")]
    [DataRow("aud")]
    [DataRow("uah")]
    [DataRow("Aud")]
    [DataRow("Uah")]
    public async Task Test_GetRatesAsync_NoRateCase(string currency)
    {
        var mockJson = """
        {
        "date": "01.03.2026",
        "bank": "PB",
        "baseCurrency": 980,
        "baseCurrencyLit": "UAH",
        "exchangeRate": [    
                { "baseCurrency": "UAH", "currency": "AUD", "saleRateNB": 30.7620000, "purchaseRateNB": 30.7620000, "saleRate": 0, "purchaseRate": 0 },
                { "baseCurrency": "UAH", "currency": "UAH", "saleRateNB": 1.0000000, "purchaseRateNB": 1.0000000 }
            ]
        }
        """;

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
        };

        var mockHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHandler);
        var service = new CurrencyRatesService(httpClient, _configuration, _loggerMock.Object);

        var failureResult = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));
        failureResult.IsSuccess.Should().BeFalse();

        var failureMessage = $"Exchange rates for '{currency}' are currently unavailable.";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_GetRatesAsync_EmptyResponseCase()
    {
        var mockJson = """
        {
        "date": "01.03.2027",
        "bank": "PB",
        "baseCurrency": 980,
        "baseCurrencyLit": "UAH",
        "exchangeRate": []
        }
        """;

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
        };

        var mockHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHandler);
        var service = new CurrencyRatesService(httpClient, _configuration, _loggerMock.Object);

        var currency = "USD";
        var date = new DateTime(2026, 3, 1);

        var failureResult = await service.GetRatesAsync(currency, date);
        failureResult.IsSuccess.Should().BeFalse();

        var failureMessage = $"No exchange rates available for this date: {date.ToString("dd.MM.yyyy")}";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_GetRatesAsync_HttpRequestExceptionCase()
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        var mockHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHandler);
        var service = new CurrencyRatesService(httpClient, _configuration, _loggerMock.Object);
        var currency = "USD";

        var failureResult = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));
        failureResult.IsSuccess.Should().BeFalse();

        var failureMessage = $"Failed to retrieve exchange rates. Bank API returned status: {response.StatusCode}";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_GetRatesAsync_BrokenResponseCase()
    {
        var mockJson = """
        {
            "someBrokenJson": [
        }
        """;

        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(mockJson, Encoding.UTF8, "application/json")
        };

        var mockHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHandler);
        var service = new CurrencyRatesService(httpClient, _configuration, _loggerMock.Object);

        var failureResult = await service.GetRatesAsync("USD", new DateTime(2026, 3, 1));
        failureResult.IsSuccess.Should().BeFalse();

        var failureMessage = "Received invalid data from the bank. Please try again later.";
        failureResult.Errors.Should().Contain(failureMessage);
    }
}
