using Shared.Models.DTOs;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IUserService
{
    Task<List<UserDto>> GetListAsync();
}
