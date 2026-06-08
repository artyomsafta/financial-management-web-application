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
public class FinancialOperationsController : ControllerBase
{
    private readonly IFinancialOperationService _operationService;

    public FinancialOperationsController(IFinancialOperationService operationService)
    {
        _operationService = operationService;
    }

    [HttpGet("list")]
    public async Task<ActionResult> GetListAsync()
    {
        var result = await _operationService.GetListAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var result = await _operationService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] CreateFinOperationRequest request)
    {
        var result = await _operationService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateFinOperationRequest request)
    {
        var result = await _operationService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        var result = await _operationService.DeleteAsync(id);
        return NoContent();
    }
}
