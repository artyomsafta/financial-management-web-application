using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var username = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authUser");

            if (string.IsNullOrWhiteSpace(username))
                return new AuthenticationState(_anonymous);

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "custom");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkUserAsAuthenticated(string username)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authUser", username);

        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "custom");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authUser");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
