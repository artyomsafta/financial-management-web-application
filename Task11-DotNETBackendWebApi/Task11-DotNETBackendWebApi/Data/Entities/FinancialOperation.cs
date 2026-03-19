using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Task11_DotNETBackendWebApi.Data.Entities;

public class FinancialOperation
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [Precision(20, 4)]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime Date { get; set; }

    [Required]
    public string Currency { get; set; } = null!;

    [MaxLength(255)]
    public string Comment { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    [Required]
    public bool IsDeleted { get; set; } = false;

    [Required]
    public Guid FinancialTypeId { get; set; }

    [Required]
    public Guid WalletId { get; set; }

    public FinancialType Type { get; set; }
    public Wallet Wallet { get; set; }
}
