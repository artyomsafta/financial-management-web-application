using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Models;

public class WalletRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Wallet name is required")]
    public string Name { get; set; }

    [Range(0.00, double.MaxValue, ErrorMessage = "The wallet balance must be greater than zero.")]
    public decimal Balance { get; set; }

    [Required(ErrorMessage = "Set the base currency for your wallet")]
    public string BaseCurrency { get; set; }
}
