using Shared.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface ICurrencyRatesService
{
    Task<CurrencyRateResult> GetRatesAsync(string currencyCode, DateTime date);
}
