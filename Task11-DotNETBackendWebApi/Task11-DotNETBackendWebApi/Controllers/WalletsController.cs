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
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("list")]
    public async Task<ActionResult> GetListAsync()
    {
        var result = await _walletService.GetListAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var result = await _walletService.GetByIdAsync(id);
        return Ok(result);        
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync([FromBody] CreateWalletRequest request)
    {
        var result = await _walletService.CreateAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateWalletRequest request)
    { 
        var result = await _walletService.UpdateAsync(id, request); 
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        var result = await _walletService.DeleteAsync(id);
        return NoContent();
    }
}
