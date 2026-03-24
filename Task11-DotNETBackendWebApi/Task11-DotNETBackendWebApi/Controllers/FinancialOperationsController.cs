using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class FinancialOperationsController : ControllerBase
{
    private readonly IFinancialOperationService _operationService;

    public FinancialOperationsController(IFinancialOperationService operationService)
    {
        _operationService = operationService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<FinancialOperationDto>>> GetList()
    {
        var operations = await _operationService.GetListAsync();
        return Ok(operations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FinancialOperationDto>> GetById([FromRoute] Guid id)
    {
        try
        {
            var operation = await _operationService.GetByIdAsync(id);
            if (operation is null)
            {
                return NotFound(new { message = $"Operation with ID {id} not found" });
            }

            return Ok(operation);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<FinancialOperationDto>> Create([FromBody] FinancialOperationRequest request)
    {
        try
        {
            var createdOperation = await _operationService.CreateAsync(request);
            if (createdOperation is null)
            {
                return NotFound(new { message = "The transaction was not created. Please verify that the input data is correct." });
            }

            return CreatedAtAction(nameof(GetById), new { id = createdOperation.Id }, createdOperation);
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
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] FinancialOperationRequest request)
    {
        try
        {
            var isUpdated = await _operationService.UpdateAsync(id, request);
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
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var isDeleted = await _operationService.DleteAsync(id);
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
