using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly ILogger<WalletService> _logger;
    public WalletService(
        AppDbContext context, 
        IUserContext userContext, 
        ILogger<WalletService> logger
    )
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
            .Select(w => new WalletDto
            {
                Id = w.Id,
                Name = w.Name,
                BaseCurrency = new CurrencyListDto
                {
                    Id = w.CurrencyId,
                    Code = w.Currency.Code
                },
                User = new UserDto
                {
                    Id = w.UserId,
                    Username = w.User.Username,
                    Role = w.User.Role
                }
            })
            .ToListAsync();
    }

    public async Task<WalletDto> GetByIdAsync(Guid id)
    {
        var query = _context.Wallets
            .AsNoTracking()
            .Where(w => w.Id == id);

        if (!_userContext.IsAdmin)
        {
            query = query.Where(w => w.UserId == _userContext.UserId);
        }

        var wallet = await query
            .Select(w => new WalletDto
            {
                Id = w.Id,
                Name = w.Name,
                BaseCurrency = new CurrencyListDto
                {
                    Id = w.CurrencyId,
                    Code = w.Currency.Code
                },
                User = new UserDto
                {
                    Id = w.UserId,
                    Username = w.User.Username,
                    Role = w.User.Role
                }
            })
            .FirstOrDefaultAsync();

        if (wallet is null)
        {
            throw new KeyNotFoundException($"Wallet with ID {id} not found");
        }

        return wallet;
    }

    public async Task<Guid> CreateAsync(CreateWalletRequest request)
    {
        if (!_userContext.IsAdmin && request.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (!request.BaseCurrency.IsValidCurrencyCode())
            throw new ValidationException("The currency code is incorrect.");

        var baseCurrency = request.BaseCurrency.Trim().ToUpper();

        if (baseCurrency != "UAH")
        {
            throw new ValidationException("Currently, the base currency of the wallet can only be UAH");
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user is null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        }

        var currencyId = await GetOrCreateCurrencyIdAsync(baseCurrency);

        var newWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            UserId = user.Id,
            CurrencyId = currencyId,
            IsDeleted = false
        };

        try
        { 
            _context.Wallets.Add(newWallet);
            await _context.SaveChangesAsync();

            return newWallet.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating a new wallet with id {Id}.", newWallet.Id);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateWalletRequest request)
    {
        if (!request.BaseCurrency.IsValidCurrencyCode())
            throw new ValidationException("The currency code is incorrect.");

        var baseCurrency = request.BaseCurrency.Trim().ToUpper();

        if (baseCurrency != "UAH")
        {
            throw new ValidationException("Currently, the base currency of the wallet can only be UAH");
        }

        var wallet = await _context.Wallets.FindAsync(id);
        if (wallet is null)
        {
            throw new KeyNotFoundException("Wallet not found");
        }

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var currencyId = await GetOrCreateCurrencyIdAsync(baseCurrency);

        wallet.Name = request.Name.Trim();
        wallet.CurrencyId = currencyId;

        try 
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating the wallet with id {Id}.", wallet.Id);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(UpdateAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var wallet = await _context.Wallets
            .Include(w => w.FinancialOperations)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wallet is null)
        {
            throw new KeyNotFoundException("Wallet not found");
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
            _logger.LogError(ex, "Database error occurred while deleting the wallet with id {Id}.", wallet.Id);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(DeleteAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    private async Task<int> GetOrCreateCurrencyIdAsync(string currencyCode)
    {
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == currencyCode);

        if (currency is null)
        {
            currency = new Currency
            {
                Code = currencyCode
            };

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();
        }

        return currency.Id;
    }
}
