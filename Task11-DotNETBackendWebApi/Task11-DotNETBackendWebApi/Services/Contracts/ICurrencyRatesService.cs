using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface ICurrencyRatesService
{
    Task<Result<CurrencyRateResult>> GetRatesAsync(string currencyCode, DateTime date);
}
