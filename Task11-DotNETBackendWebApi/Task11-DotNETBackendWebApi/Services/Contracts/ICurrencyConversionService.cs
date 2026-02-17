using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface ICurrencyConversionService
{
    Task<string> GetResultAsync(string currencyCode, DateTime date); // Think about decimal type for result

    Task<List<ExchangeRatesDto>> GetExchangeRatesAsync(DateTime date); // May be implement this method as private one
}
