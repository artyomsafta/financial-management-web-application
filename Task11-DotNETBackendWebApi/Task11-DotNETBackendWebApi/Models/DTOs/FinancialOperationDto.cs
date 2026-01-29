using System.Text.Json.Serialization;

namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class FinancialOperationDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("typeId")]
    public string TypeId { get; set; }

    [JsonPropertyName("amount")]
    public float Amount { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; }
}
