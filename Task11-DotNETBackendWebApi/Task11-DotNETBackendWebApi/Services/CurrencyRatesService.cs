using System.Text.Json;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class CurrencyRatesService : ICurrencyRatesService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public CurrencyRatesService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<decimal> GetRateAsync(string currencyCode, DateTime date)
    {
        try
        {
            var ratesList = await GetRatesListAsync(date);
            var exchangeRates = ratesList
                .FirstOrDefault(r => string.Equals(r.Currency, currencyCode, StringComparison.OrdinalIgnoreCase));

            if (exchangeRates is not null && exchangeRates.PurchaseRate > 0)
            {
                return Math.Round(exchangeRates.PurchaseRate, 2, MidpointRounding.AwayFromZero);
            }

            throw new InvalidOperationException($"No exchange rates available for this currency: {currencyCode}");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Failed to retrieve exchange rates. Please try again later.", ex);
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
