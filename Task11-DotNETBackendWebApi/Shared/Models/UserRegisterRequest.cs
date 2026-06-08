using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class UserRegisterRequest
{
    [Required(ErrorMessage = "User name is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}
