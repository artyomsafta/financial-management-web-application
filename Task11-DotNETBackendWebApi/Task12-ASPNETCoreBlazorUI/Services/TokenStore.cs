using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class TokenStore
{
    private ProtectedLocalStorage _localStorage;

    public TokenStore(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SetTokenAsync(string token)
    {
        await _localStorage.SetAsync("access_token", token);
    }

    public async Task<string?> GetTokenAsync()
    {
        var result = await _localStorage.GetAsync<string>("access_token");
        return result.Success ? result.Value : null;
    }

    public async Task ClearTokenAsync()
    {
        await _localStorage.DeleteAsync("access_token");
    }
}
