using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Helpers.Enums;
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
        if (!result.IsSuccess)
        {
            return NotFound(new { errors = result.Errors });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<ActionResult<FinancialTypeDto>> Create([FromBody] FinancialTypeRequest request)
    {
        try
        {
            var result = await _typeService.CreateAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] FinancialTypeRequest request)
    {
        try
        {
            var result = await _typeService.UpdateAsync(id, request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var result = await _typeService.DeleteAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }
}
