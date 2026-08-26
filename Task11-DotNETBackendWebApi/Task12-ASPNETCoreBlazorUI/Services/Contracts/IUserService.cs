using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IUserService
{
    Task<List<UserDto>> GetListAsync();
    Task<UserDto> GetByIdAsync(Guid id);
    Task<ApiResponseDto> CreateAsync(UserRegisterRequest request);
    Task<ApiResponseDto> UpdateAsync(Guid id, UserRegisterRequest request);
    Task<ApiResponseDto> DeleteAsync(Guid id);
}
