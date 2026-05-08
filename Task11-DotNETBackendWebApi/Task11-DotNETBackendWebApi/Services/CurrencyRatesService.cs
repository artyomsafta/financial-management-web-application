using System.Text.Json;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
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
        _baseUrl = _configuration["ExchangeRatesApi:BaseUrl"] ?? throw new ArgumentNullException("ExchangeRatesApi:BaseUrl"); //TODO: handle null case in a middleware
        _logger = logger;
    }
   
    public async Task<Result<CurrencyRateResult>> GetRatesAsync(string currencyCode, DateTime date)
    {
        try
        {
            var ratesListResult = await GetRatesListAsync(date);

            if (!ratesListResult.IsSuccess)
            {
                return Result<CurrencyRateResult>.Failure(ratesListResult.Errors);
            }

            var exchangeRates = ratesListResult.Data
                .FirstOrDefault(r => string.Equals(r.Currency, currencyCode, StringComparison.OrdinalIgnoreCase));

            if (exchangeRates is not null && exchangeRates.SaleRate > 0 && exchangeRates.PurchaseRate > 0)
            {
                return Result<CurrencyRateResult>.Success(new CurrencyRateResult
                {
                    SaleRate = Math.Round(exchangeRates.SaleRate, 2, MidpointRounding.AwayFromZero),
                    PurchaseRate = Math.Round(exchangeRates.PurchaseRate, 2, MidpointRounding.AwayFromZero)
                });
            }

            return Result<CurrencyRateResult>.Failure($"Exchange rates for '{currencyCode}' are currently unavailable.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Bank API is unavailable.");
            return Result<CurrencyRateResult>.Failure("External bank service is unavailable. Please try again later.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse bank response.");
            return Result<CurrencyRateResult>.Failure("Received invalid data from the bank. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving exchange rates.");
            return Result<CurrencyRateResult>.Failure("An unexpected error occurred. Please try again later.");
        }
    }

    private async Task<Result<List<ExchangeRate>>> GetRatesListAsync(DateTime date)
    {
        var dateString = date.ToString("dd.MM.yyyy");
        var url = $"{_baseUrl}{dateString}";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return Result<List<ExchangeRate>>.Failure($"Failed to retrieve exchange rates. Bank API returned status: {response.StatusCode}");
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonObj = JsonSerializer.Deserialize<ExchangeRatesDto>(jsonString);

        if (jsonObj?.ExchangeRates == null || jsonObj.ExchangeRates.Count == 0)
        {
            return Result<List<ExchangeRate>>.Failure($"No exchange rates available for this date: {dateString}");
        }

        return Result<List<ExchangeRate>>.Success(jsonObj.ExchangeRates);
    }
}
