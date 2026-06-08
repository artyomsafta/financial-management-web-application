using Shared.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class CurrencyRatesService : ICurrencyRatesService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly ILogger<CurrencyRatesService> _logger;

    public CurrencyRatesService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<CurrencyRatesService> logger
    )
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["ExchangeRatesApi:BaseUrl"] 
            ?? throw new InvalidOperationException("Base url for exchange rates API is missing.");
        _logger = logger;
    }
   
    public async Task<CurrencyRateResult> GetRatesAsync(string currencyCode, DateTime date)
    {
        try
        {
            var ratesListResult = await GetRatesListAsync(date);

            var exchangeRates = ratesListResult
                .FirstOrDefault(r => string.Equals(r.Currency, currencyCode, StringComparison.OrdinalIgnoreCase));

            if (exchangeRates is not null && exchangeRates.SaleRate > 0 && exchangeRates.PurchaseRate > 0)
            {
                return new CurrencyRateResult
                {
                    SaleRate = Math.Round(exchangeRates.SaleRate, 2, MidpointRounding.AwayFromZero),
                    PurchaseRate = Math.Round(exchangeRates.PurchaseRate, 2, MidpointRounding.AwayFromZero)
                };
            }

            throw new InvalidOperationException($"Exchange rates for '{currencyCode}' are currently unavailable.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HttpRequestException("External bank service is unavailable. Please try again later.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new InvalidOperationException(ex.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse bank response.");
            throw new ValidationException("Received invalid data from the bank. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving exchange rates.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    private async Task<List<ExchangeRate>> GetRatesListAsync(DateTime date)
    {
        var dateString = date.ToString("dd.MM.yyyy");
        var url = $"{_baseUrl}{dateString}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to retrieve exchange rates. Bank API returned status: {response.StatusCode}");
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonObj = JsonSerializer.Deserialize<ExchangeRatesDto>(jsonString);

        if (jsonObj?.ExchangeRates == null || jsonObj.ExchangeRates.Count == 0)
        {
            throw new InvalidOperationException($"No exchange rates available for this date: {dateString}");
        }

        return jsonObj.ExchangeRates;
    }
}
