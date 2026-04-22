using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Text;
using Task11_DotNETBackendWebApi.Helpers.Enums;
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

    [TestInitialize]
    public void Setup()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExchangeRatesApi:BaseUrl"] = "http://mock-api.com/"
            })
            .Build();
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
    public async Task Test_GetRateAsync_PositiveCases(string currency, double saleRate, double purchaseRate)
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
        var service = new CurrencyRatesService(httpClient, _configuration);

        var result = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));

        result.SaleRate.Should().Be((decimal)saleRate);
        result.PurchaseRate.Should().Be((decimal)purchaseRate);
    }

    [DataTestMethod]
    [DataRow("GBP")]
    [DataRow("gbp")]
    [DataRow("Gbp")]
    public async Task Test_GetRateAsync_CurrencyNotFoundCase(string currency)
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
        var service = new CurrencyRatesService(httpClient, _configuration);

        var expectedErrorMessage = $"No exchange rates available for this currency: {currency}";

        try
        {
            var result = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [DataTestMethod]
    [DataRow("AUD")]
    [DataRow("UAH")]
    [DataRow("aud")]
    [DataRow("uah")]
    [DataRow("Aud")]
    [DataRow("Uah")]
    public async Task Test_GetRateAsync_NoRateCase(string currency)
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
        var service = new CurrencyRatesService(httpClient, _configuration);

        var expectedErrorMessage = $"No exchange rates available for this currency: {currency}";

        try
        {
            var result = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_GetRateAsync_EmptyResponseCase()
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
        var service = new CurrencyRatesService(httpClient, _configuration);
        var currency = nameof(Currencies.USD);

        var expectedErrorMessage = "No exchange rates available for this date: 01.03.2027";

        try
        {
            var result = await service.GetRatesAsync(currency, new DateTime(2027, 3, 1));
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_GetRateAsync_HttpRequestExceptionCase()
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
        var mockHandler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(mockHandler);
        var service = new CurrencyRatesService(httpClient, _configuration);
        var currency = nameof(Currencies.USD);

        var expectedErrorMessage = "Failed to retrieve exchange rates. Please try again later.";

        try
        {
            var result = await service.GetRatesAsync(currency, new DateTime(2026, 3, 1));
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.Contains(expectedErrorMessage, actualError.Message);
            Assert.IsInstanceOfType(actualError.InnerException, typeof(HttpRequestException));
        }
    }
}
