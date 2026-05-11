namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public CurrencyListDto BaseCurrency { get; set; }
    public UserDto User { get; set; }
}
