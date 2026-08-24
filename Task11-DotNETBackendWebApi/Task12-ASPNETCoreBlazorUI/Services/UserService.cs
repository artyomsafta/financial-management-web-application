using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
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

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        return await _httpService.GetByIdAsync<UserDto>($"{_usersUri}/{id}");
    }

    public async Task<ApiResponseDto> CreateAsync(UserRegisterRequest request)
    {
        return await _httpService.PostAsync(_usersUri, request);
    }

    public async Task<ApiResponseDto> UpdateAsync(Guid id, UserRegisterRequest request)
    {
        return await _httpService.PutAsync($"{_usersUri}/{id}", request);
    }

    public async Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        return await _httpService.DeleteAsync($"{_usersUri}/{id}");
    }
}
