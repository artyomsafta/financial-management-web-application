using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class FinancialTypeRequest
{
    [Required(ErrorMessage = "Type name is required")]
    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    [Required(ErrorMessage = "IsIncome flag is required")]
    public bool IsIncome { get; set; }
}
