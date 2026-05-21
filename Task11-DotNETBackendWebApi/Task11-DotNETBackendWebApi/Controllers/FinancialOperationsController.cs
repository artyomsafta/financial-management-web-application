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
            var result = await _operationService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { errors = result.Errors });
            }

            return Ok(result.Data);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<FinancialOperationDto>> Create([FromBody] CreateFinOperationRequest request)
    {
        try
        {
            var result = await _operationService.CreateAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetById), new { id = result }, result.Data);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFinOperationRequest request)
    {
        try
        {
            var result = await _operationService.UpdateAsync(id, request);
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
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var result = await _operationService.DeleteAsync(id);
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
