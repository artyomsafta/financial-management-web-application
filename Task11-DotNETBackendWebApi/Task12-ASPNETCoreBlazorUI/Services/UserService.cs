using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class UserService : IUserService
{
    private readonly IHttpService _httpService;
    private static readonly string _usersUri = "Users";
    private static readonly string _usersListUri = "Users/list";

    public UserService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<List<UserDto>> GetListAsync()
    {
        return await _httpService.GetListAsync<UserDto>($"{_usersListUri}");
    }
}
