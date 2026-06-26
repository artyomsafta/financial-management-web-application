using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]                                                  // TODO : Uncomment this line when authentication is implemented!!!
//[AllowAnonymous]                                                //TODO : Remove this line when authentication is implemented!!!
public class FinancialTypesController : ControllerBase
{
    private readonly IFinancialTypeService _typeService;

    public FinancialTypesController(IFinancialTypeService typeService)
    {
        _typeService = typeService;
    }

    [HttpGet("list")]
    public async Task<ActionResult> GetListAsync()
    {
        var result = await _typeService.GetListAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var result = await _typeService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<ActionResult> CreateAsync([FromBody] FinancialTypeRequest request)
    {
        var result = await _typeService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<ActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] FinancialTypeRequest request)
    {
        var result = await _typeService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        var result = await _typeService.DeleteAsync(id);
        return NoContent();
    }
}
