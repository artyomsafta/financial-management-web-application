using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetListAsync();
    Task<UserDto> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(UserRegisterRequest request);
    Task<bool> UpdateAsync(Guid id, UserRegisterRequest request);
    Task<bool> DeleteAsync(Guid id);
}
