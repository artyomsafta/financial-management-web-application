using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinancialOperationsController : ControllerBase
{
    private readonly IFinancialOperationService _operationService;

    public FinancialOperationsController(IFinancialOperationService operationService)
    {
        _operationService = operationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FinancialOperationDto>>> GetAll()
    {
        var operations = await _operationService.GetAllAsync();
        return Ok(operations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FinancialOperationDto>> GetById(Guid id)
    {
        var operation = await _operationService.GetByIdAsync(id);
        if (operation is null)
        {
            return NotFound(new { message = $"Operation with ID {id} not found" });
        }

        return Ok(operation);
    }

    [HttpPost]
    public async Task<ActionResult<FinancialOperationDto>> Create([FromBody] FinancialOperationRequest request)
    {
        try
        {
            var createdOperation = await _operationService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdOperation.Id }, createdOperation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FinancialOperationRequest request)
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _operationService.SoftDeleteAsync(id);
        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
