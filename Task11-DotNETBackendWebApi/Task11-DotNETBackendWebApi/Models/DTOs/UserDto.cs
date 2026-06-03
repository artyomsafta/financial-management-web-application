using Task11_DotNETBackendWebApi.Data.Entities;

namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public UserRoles Role { get; set; }
}
