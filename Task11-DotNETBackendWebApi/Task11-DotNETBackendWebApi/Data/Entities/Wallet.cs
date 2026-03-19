using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Task11_DotNETBackendWebApi.Helpers.Enums;

namespace Task11_DotNETBackendWebApi.Data.Entities;

public class Wallet
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string BaseCurrency { get; set; } = nameof(Currencies.UAH);

    [Required]
    public bool IsDeleted { get; set; } = false;

    [Required]
    public Guid UserId { get; set; }

    public User User { get; set; }
    public List<FinancialOperation> FinancialOperations { get; set; } = new();
}
