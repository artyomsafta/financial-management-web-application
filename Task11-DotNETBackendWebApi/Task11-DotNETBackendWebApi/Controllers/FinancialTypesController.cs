using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FinancialTypesController : ControllerBase
{
    private readonly IFinancialTypeService _typeService;

    public FinancialTypesController(IFinancialTypeService typeService)
    {
        _typeService = typeService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<FinancialTypeDto>>> GetList()
    {
        var types = await _typeService.GetListAsync();
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FinancialTypeDto>> GetById([FromRoute] Guid id)
    {
        var result = await _typeService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<ActionResult<FinancialTypeDto>> Create([FromBody] FinancialTypeRequest request)
    {
        var result = await _typeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] FinancialTypeRequest request)
    {
        var result = await _typeService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _typeService.DeleteAsync(id);
        return NoContent();
    }
}
