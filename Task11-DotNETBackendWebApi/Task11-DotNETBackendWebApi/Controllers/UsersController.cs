using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("list")]
    public async Task<ActionResult> GetListAsync()
    {
        var result = await _userService.GetListAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateAsync([FromBody] UserRegisterRequest request)
    {
        var result = await _userService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UserRegisterRequest request)
    {
        var result = await _userService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        var result = await _userService.DeleteAsync(id);
        return NoContent();
    }
}
