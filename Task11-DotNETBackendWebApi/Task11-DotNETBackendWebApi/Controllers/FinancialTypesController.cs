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
        var type = await _typeService.GetByIdAsync(id);
        if (type is null)
        {
            return NotFound(new { message = $"Type with ID {id} not found" });
        }

        return Ok(type);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<ActionResult<FinancialTypeDto>> Create([FromBody] FinancialTypeRequest request)
    {
        try
        {
            var createdType = await _typeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdType.Id }, createdType);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] FinancialTypeRequest request)
    {
        try
        {
            var isUpdated = await _typeService.UpdateAsync(id, request);
            if (!isUpdated)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var isDeleted = await _typeService.DeleteAsync(id);
            if (!isDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
