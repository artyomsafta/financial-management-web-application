using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    public WalletService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<WalletDto>> GetAllAsync()
    {
        return await _context.Wallets
            .Include(w => w.User)
            .Select(w => new WalletDto
            {
                Id = w.Id,
                Name = w.Name,
                Balance = w.Balance,
                BaseCurrency = w.BaseCurrency,
                UserId = w.UserId,
                Username = w.User.Username
            })
            .ToListAsync();
    }

    public async Task<WalletDto?> GetByIdAsync(Guid id)
    {
        var wallet = await _context.Wallets
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wallet is null)
        {
            return null;
        }

        return MapToDto(wallet);
    }

    public async Task<WalletDto> CreateAsync(WalletRequest request)
    {
        request.BaseCurrency.EnsureCurrencyIsValid();
        await _context.Users.EnsureUserExistsAsync(request.UserId);

        var newWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Balance = 0m,
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
        request.BaseCurrency.EnsureCurrencyIsValid();

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

        try 
        {
            wallet.Name = request.Name.Trim();
            wallet.BaseCurrency = request.BaseCurrency.Trim().ToUpper();
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the wallet with id {Id}.", wallet.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var wallet = await _context.Wallets
            .Include(w => w.FinancialOperations)
            .FirstOrDefaultAsync(w => w.Id == id);
        if (wallet is null)
        {
            return false;
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
            Balance = wallet.Balance,
            BaseCurrency = wallet.BaseCurrency,
            UserId = wallet.UserId,
            Username = wallet.User.Username
        };
    }
}
