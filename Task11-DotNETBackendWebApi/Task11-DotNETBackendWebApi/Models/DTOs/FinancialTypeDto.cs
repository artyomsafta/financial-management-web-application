namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class FinancialTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsIncome { get; set; }
}
