using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Models;

public class FinancialOperationRequest
{
    [Required]
    public Guid TypeId { get; set; }
    [Range(0.00, double.MaxValue, ErrorMessage = "The amount must be greater than zero.")]
    public decimal Amount { get; set; }
    [Required]
    public DateTime Date { get; set; }
    public string Note { get; set; } = string.Empty;
}
