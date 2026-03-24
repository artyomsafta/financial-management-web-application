using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Data.Entities;

[Index(nameof(Code), IsUnique = true)]
public class Currency
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(3)]
    public string Code { get; set; } = string.Empty;

    public List<Wallet> Wallets { get; set; } = new();
    public List<FinancialOperation> FinancialOperations { get; set; } = new();
}
