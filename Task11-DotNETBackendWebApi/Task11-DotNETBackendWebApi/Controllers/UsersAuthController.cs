using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersAuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;

    public UsersAuthController(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<UserLoginResponse>> Login([FromBody] UserLoginRequest request)
    {
        var response = await _userAuthService.LoginAsync(request);

        if (response is null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(response);
    }
}
