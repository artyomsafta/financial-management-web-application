using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Helpers.Enums;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialTypesController : ControllerBase
{
    private readonly IFinancialTypeService _typeService;

    public FinancialTypesController(IFinancialTypeService typeService)
    {
        _typeService = typeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinancialTypeDto>>> GetAll()
    {
        var types = await _typeService.GetAllAsync();
        return Ok(types);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FinancialTypeDto>> GetById(Guid id)
    {
        var type = await _typeService.GetByIdAsync(id);
        if (type is null)
        {
            return NotFound(new { message = $"Type with ID {id} not found" });
        }

        return Ok(type);
    }

    [HttpPost]
    public async Task<ActionResult<FinancialTypeDto>> Create([FromBody] FinancialTypeRequest request)
    {
        try
        {
            var createdType = await _typeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdType.Id }, createdType);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FinancialTypeRequest request)
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRoles.Admin))]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var isDeleted = await _typeService.SoftDeleteAsync(id);
            if (!isDeleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
