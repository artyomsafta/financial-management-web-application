using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Data.Entities;

[Index(nameof(Name), IsUnique = true)]
public class FinancialType
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public bool IsIncome { get; set; }

    [Required]
    public bool IsDeleted { get; set; } = false;

    public List<FinancialOperation> FinancialOperations { get; set; } = new();
}
