using Task11_DotNETBackendWebApi.Models;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IUserAuthService
{
    Task<UserLoginResponse?> LoginAsync(UserLoginRequest request);
}
