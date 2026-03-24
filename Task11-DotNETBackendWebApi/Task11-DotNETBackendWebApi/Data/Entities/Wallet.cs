using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Data.Entities;

public class Wallet
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public bool IsDeleted { get; set; } = false;

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int CurrencyId { get; set; }

    public User User { get; set; }
    public Currency Currency { get; set; }
    public List<FinancialOperation> FinancialOperations { get; set; } = new();
}
