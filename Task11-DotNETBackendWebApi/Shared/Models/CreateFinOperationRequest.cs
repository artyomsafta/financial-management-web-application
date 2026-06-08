using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class CreateFinOperationRequest
{
    [Required]
    public Guid TypeId { get; set; }

    [Required]
    public Guid WalletId { get; set; }

    [Required]
    [Range(0.00, double.MaxValue, ErrorMessage = "The amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Currency { get; set; }

    public string Note { get; set; } = "";
}
