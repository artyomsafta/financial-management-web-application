using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersAuthController : ControllerBase
{
    private readonly IUserAuthService _userAuthService;

    public UsersAuthController(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    [HttpPost("login")]
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
