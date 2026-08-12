using Shared.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IAuthService
{
    Task<UserLoginResponse?> LoginAsync(UserLoginRequest request);
    Task LogoutAsync();
    Task<HttpClient> CreateAuthenticatedClientAsync();
}
