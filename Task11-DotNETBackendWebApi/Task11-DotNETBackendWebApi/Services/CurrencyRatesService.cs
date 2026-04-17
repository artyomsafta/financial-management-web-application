using System.Text.Json;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class CurrencyRatesService : ICurrencyRatesService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CurrencyRatesService> _logger;

    public CurrencyRatesService(HttpClient httpClient, IConfiguration configuration, ILogger<CurrencyRatesService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<CurrencyRateResult> GetRateAsync(string currencyCode, DateTime date)
    {
        try
        {
            var ratesList = await GetRatesListAsync(date);
            var exchangeRates = ratesList
                .FirstOrDefault(r => string.Equals(r.Currency, currencyCode, StringComparison.OrdinalIgnoreCase));

            if (exchangeRates is not null && exchangeRates.SaleRate > 0 && exchangeRates.PurchaseRate > 0)
            {
                return new CurrencyRateResult
                {
                    SaleRate = Math.Round(exchangeRates.SaleRate, 2, MidpointRounding.AwayFromZero),
                    PurchaseRate = Math.Round(exchangeRates.PurchaseRate, 2, MidpointRounding.AwayFromZero)
                };
            }

            throw new InvalidOperationException($"No exchange rates available for this currency: {currencyCode}");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while retrieving exchange rates for currency: {CurrencyCode} on date: {Date}", currencyCode, date);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error while retrieving exchange rates for currency: {CurrencyCode} on date: {Date}", currencyCode, date);
            throw new InvalidOperationException("Failed to retrieve exchange rates. Please try again later.", ex);
        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "An unexpected error.");
            throw new InvalidOperationException("An unexpected error occurred while retrieving exchange rates.", ex);
        }
    }

    private async Task<List<ExchangeRate>> GetRatesListAsync(DateTime date)
    {
        var baseUrl = _configuration["ExchangeRatesApi:BaseUrl"];
        var dateString = date.ToString("dd.MM.yyyy");
        var exchangeRatesApiUrl = $"{baseUrl}{dateString}";

        var request = new HttpRequestMessage(HttpMethod.Get, exchangeRatesApiUrl);
        var response = await _httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonObj = JsonSerializer.Deserialize<ExchangeRatesDto>(jsonString);
        var exchangeRates = jsonObj.ExchangeRates;

        if (exchangeRates.Count is 0)
        {
            throw new InvalidOperationException($"No exchange rates available for this date: {dateString}");
        }

        return exchangeRates;
    }
}
