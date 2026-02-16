using Task11_DotNETBackendWebApi.Helpers.Enums;

namespace Task11_DotNETBackendWebApi.Data.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = nameof(UserRoles.User);
    public bool IsDeleted { get; set; } = false;

    public List<Wallet> Wallets { get; set; } = new();
}
