namespace Shared.Models.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public UserRoles Role { get; set; }
}
