using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers;
using Task11_DotNETBackendWebApi.Helpers.Enums;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly ILogger<WalletService> _logger;
    public WalletService(AppDbContext context, IUserContext userContext, ILogger<WalletService> logger)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<IEnumerable<WalletDto>> GetListAsync()
    {
        var query = _context.Wallets.AsQueryable();

        if (!_userContext.IsAdmin)
        {
            query = query.Where(w => w.UserId == _userContext.UserId);
        }

        return await query
            .AsNoTracking()
            .Include(w => w.User)
            .Select(w => new WalletDto
            {
                Id = w.Id,
                Name = w.Name,
                BaseCurrency = w.BaseCurrency,
                UserId = w.UserId,
                Username = w.User.Username
            })
            .ToListAsync();
    }

    public async Task<WalletDto?> GetByIdAsync(Guid id)
    {
        var wallet = await _context.Wallets
            .AsNoTracking()
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wallet is null)
        {
            return null;
        }

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        return MapToDto(wallet);
    }

    public async Task<WalletDto> CreateAsync(WalletRequest request)
    {
        if (!_userContext.IsAdmin && request.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        request.BaseCurrency.EnsureCurrencyIsValid();

        if (request.BaseCurrency.Trim().ToUpper() != (nameof(Currencies.UAH)))
        {
            throw new InvalidOperationException("Currently, the base currency of the wallet can only be UAH");
        }

        await _context.Users.EnsureUserExistsAsync(request.UserId);

        var newWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            BaseCurrency = request.BaseCurrency.Trim().ToUpper(),
            IsDeleted = false,
            UserId = request.UserId
        };

        try
        { 
            _context.Wallets.Add(newWallet);
            await _context.SaveChangesAsync();

            var wallet = await _context.Wallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == newWallet.Id);

            return MapToDto(wallet);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new wallet.");
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> UpdateAsync(Guid id, WalletRequest request)
    {
        if (!_userContext.IsAdmin && request.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        request.BaseCurrency.EnsureCurrencyIsValid();

        if (request.BaseCurrency.Trim().ToUpper() != (nameof(Currencies.UAH)))
        {
            throw new InvalidOperationException("Currently, the base currency of the wallet can only be UAH");
        }

        var wallet = await _context.Wallets.FindAsync(id);
        if (wallet is null)
        {
            return false;
        }

        if (wallet.UserId != request.UserId)
        {
            await _context.Users.EnsureUserExistsAsync(request.UserId);
            wallet.UserId = request.UserId;
        }

        wallet.Name = request.Name.Trim();
        wallet.BaseCurrency = request.BaseCurrency.Trim().ToUpper();

        try 
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the wallet with id {Id}.", wallet.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var wallet = await _context.Wallets
            .Include(w => w.FinancialOperations)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wallet is null)
        {
            return false;
        }

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (wallet.FinancialOperations.Any(o => !o.IsDeleted))
        {
            throw new InvalidOperationException("Cannot delete a wallet that has associated financial operations.");
        }

        try 
        { 
            wallet.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while soft deleting the wallet with id {Id}.", wallet.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    private WalletDto MapToDto(Wallet wallet)
    {
        return new WalletDto
        {
            Id = wallet.Id,
            Name = wallet.Name,
            BaseCurrency = wallet.BaseCurrency,
            UserId = wallet.UserId,
            Username = wallet.User.Username
        };
    }
}
