using System.ComponentModel.DataAnnotations;
using Task11_DotNETBackendWebApi.Helpers.Enums;

namespace Task11_DotNETBackendWebApi.Data.Entities;

public class User
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = nameof(UserRoles.User);

    [Required]
    public bool IsDeleted { get; set; } = false;

    public List<Wallet> Wallets { get; set; } = new();
}
