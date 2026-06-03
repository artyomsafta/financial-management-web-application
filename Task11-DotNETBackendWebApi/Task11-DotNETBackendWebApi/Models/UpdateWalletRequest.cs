using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Models;

public class UpdateWalletRequest
{
    [Required(ErrorMessage = "Wallet name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Set the base currency for your wallet")]
    public string BaseCurrency { get; set; }
}
