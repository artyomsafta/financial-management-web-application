using Shared.Models;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly TokenStore _tokenStore;

    public AuthService(
        IHttpClientFactory clientFactory, 
        TokenStore tokenStore
    )
    {
        _clientFactory = clientFactory;
        _tokenStore = tokenStore;
    }

    public async Task<UserLoginResponse?> LoginAsync(UserLoginRequest request)
    {
        var client = _clientFactory.CreateClient("Api");

        var response = await client.PostAsJsonAsync("api/v1/UsersAuth/login", request);

        if (!response.IsSuccessStatusCode)
            return null;

        var loginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();

        if (loginResponse is null)
            return null;

        await _tokenStore.SetTokenAsync(loginResponse.Token);

        return loginResponse;
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.ClearTokenAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _clientFactory.CreateClient("Api");
        var jwtToken = await _tokenStore.GetTokenAsync();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        return client;
    }
}
