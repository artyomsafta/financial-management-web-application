namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0m;
    public string BaseCurrency { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}
