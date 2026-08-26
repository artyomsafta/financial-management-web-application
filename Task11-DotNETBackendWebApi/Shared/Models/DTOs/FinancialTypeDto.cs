namespace Shared.Models.DTOs;

public class FinancialTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsIncome { get; set; }
}
