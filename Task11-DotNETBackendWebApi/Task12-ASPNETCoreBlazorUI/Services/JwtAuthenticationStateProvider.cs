using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class JwtAuthenticationStateProvider //: AuthenticationStateProvider
{
    //private readonly TokenStore _tokenStore;

    //public JwtAuthenticationStateProvider(TokenStore tokenStore)
    //{
    //    _tokenStore = tokenStore;
    //}

    //public override Task<AuthenticationState> GetAuthenticationStateAsync()
    //{
    //    var token = _tokenStore.AccessToken;

    //    if (string.IsNullOrWhiteSpace(token))
    //    {
    //        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    //    }

    //    var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");

    //    var user = new ClaimsPrincipal(identity);

    //    return Task.FromResult(new AuthenticationState(user));
    //}

    //private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    //{
    //    var handler = new JwtSecurityTokenHandler();
    //    var token = handler.ReadJwtToken(jwt);

    //    return token.Claims;
    //}
}
