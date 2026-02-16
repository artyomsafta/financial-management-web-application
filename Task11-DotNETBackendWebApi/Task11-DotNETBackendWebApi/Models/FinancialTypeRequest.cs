using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Models;

public class FinancialTypeRequest
{
    [Required(ErrorMessage = "Type name is required")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "IsIncome flag is required")]
    public bool IsIncome { get; set; }
}
