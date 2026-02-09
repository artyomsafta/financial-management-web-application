using System.ComponentModel.DataAnnotations;

namespace Task11_DotNETBackendWebApi.Models;

public class UserRegisterRequest
{
    [Required(ErrorMessage = "User name is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}
