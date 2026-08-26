namespace Shared.Models.DTOs;

public class ReportDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetResult { get; set; }

    public List<FinancialOperationDto> Operations { get; set; } = new();
}
