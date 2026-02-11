using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(UserRegisterRequest request);
    Task<bool> UpdateAsync(Guid id, UserRegisterRequest request);
    Task<bool> SoftDeleteAsync(Guid id);
}
