using System.Text.Json.Serialization;

namespace Shared.Models.DTOs;

public class ExchangeRatesDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("bank")]
    public string Bank { get; set; }

    [JsonPropertyName("baseCurrency")]
    public int BaseCurrency { get; set; }

    [JsonPropertyName("baseCurrencyLit")]
    public string BaseCurrencyLit { get; set; }

    [JsonPropertyName("exchangeRate")]
    public List<ExchangeRate> ExchangeRates { get; set; } = new();
}

public class ExchangeRate
{
    [JsonPropertyName("baseCurrency")]
    public string BaseCurrency { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("saleRateNB")]
    public decimal SaleRateNB { get; set; }

    [JsonPropertyName("purchaseRateNB")]
    public decimal PurchaseRateNB { get; set; }

    [JsonPropertyName("saleRate")]
    public decimal SaleRate { get; set; } = 0;

    [JsonPropertyName("purchaseRate")]
    public decimal PurchaseRate { get; set; } = 0;
}
