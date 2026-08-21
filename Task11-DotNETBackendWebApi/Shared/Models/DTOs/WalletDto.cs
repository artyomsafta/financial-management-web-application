namespace Shared.Models.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public CurrencyListDto BaseCurrency { get; set; } = new();
    public UserDto User { get; set; } = new();
}
