using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System.ComponentModel.DataAnnotations;

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
    public UserRoles Role { get; set; } = UserRoles.User;

    [Required]
    public bool IsDeleted { get; set; } = false;

    public List<Wallet> Wallets { get; set; } = new();
}
