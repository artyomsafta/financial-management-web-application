using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Task11_DotNETBackendWebApi.Helpers.Enums;

namespace Task11_DotNETBackendWebApi.Data.Entities;

[Index(nameof(Username), IsUnique = true)]
public class User
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required]
    public string Role { get; set; } = nameof(UserRoles.User);

    [Required]
    public bool IsDeleted { get; set; } = false;

    public List<Wallet> Wallets { get; set; } = new();
}
