using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models;
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
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<ActionResult> GetList()
    {
        var result = await _userService.GetListAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById([FromRoute] Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> Create([FromBody] UserRegisterRequest request)
    {
        var result = await _userService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] UserRegisterRequest request)
    {
        var result = await _userService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _userService.DeleteAsync(id);
        return NoContent();
    }
}
