using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetListAsync();
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(UserRegisterRequest request);
    Task<Result> UpdateAsync(Guid id, UserRegisterRequest request);
    Task<Result> DeleteAsync(Guid id);
}
