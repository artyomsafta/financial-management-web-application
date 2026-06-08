using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class CreateWalletRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Wallet name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Set the base currency for your wallet")]
    public string BaseCurrency { get; set; }
}
