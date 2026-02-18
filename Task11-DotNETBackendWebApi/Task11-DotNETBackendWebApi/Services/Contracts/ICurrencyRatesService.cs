namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface ICurrencyRatesService
{
    Task<decimal> GetRateAsync(string currencyCode, DateTime date);
}
