using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("all-wallets")]
    public async Task<ActionResult<IEnumerable<WalletDto>>> GetAll()
    {
        var wallets = await _walletService.GetAllAsync();
        return Ok(wallets);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WalletDto>> GetById(Guid id)
    {
        try
        {
            var wallet = await _walletService.GetByIdAsync(id);
            if (wallet is null)
            {
                return NotFound(new { message = $"Wallet with ID {id} not found" });
            }

            return Ok(wallet);
        }
        catch (UnauthorizedAccessException ex) 
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<WalletDto>> Create([FromBody] WalletRequest request)
    {
        try
        {
            var createdWallet = await _walletService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdWallet.Id }, createdWallet);
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
    public async Task<IActionResult> Update(Guid id, [FromBody] WalletRequest request)
    {
        try
        {
            var isUpdated = await _walletService.UpdateAsync(id, request);
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
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var isDeleted = await _walletService.SoftDeleteAsync(id);
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
