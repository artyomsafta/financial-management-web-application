using System.Text.Json.Serialization;

namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class FinancialTypeDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}
