using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
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

    public async Task<Result<WalletDto>> GetByIdAsync(Guid id)
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
            return Result<WalletDto>.Failure($"Wallet with ID {id} not found");
        }

        return Result<WalletDto>.Success(wallet);
    }

    public async Task<Result<Guid>> CreateAsync(WalletRequest request)
    {
        if (!_userContext.IsAdmin && request.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var currencyCode = request.BaseCurrency.Trim().ToUpper();
        if (string.IsNullOrEmpty(currencyCode) || currencyCode.Length != 3)
            return Result<Guid>.Failure("The currency code is incorrect.");

        if (currencyCode != "UAH")
        {
            return Result<Guid>.Failure("Currently, the base currency of the wallet can only be UAH");
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user is null)
        {
            return Result<Guid>.Failure($"User with ID {request.UserId} not found");
        }

        var newWallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            IsDeleted = false,
            UserId = user.Id,
            CurrencyId = await _context.Currencies
                .Where(c => c.Code == currencyCode)
                .Select(c => c.Id)
                .FirstOrDefaultAsync()
        };

        try
        { 
            _context.Wallets.Add(newWallet);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(newWallet.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating a new wallet with id {Id}.", newWallet.Id);
            return Result<Guid>.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            return Result<Guid>.Failure("An unexpected system error occurred.");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, WalletRequest request)
    {
        if (!_userContext.IsAdmin && request.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var currencyCode = request.BaseCurrency.Trim().ToUpper();
        if (string.IsNullOrEmpty(currencyCode) || currencyCode.Length != 3)
            return Result.Failure("The currency code is incorrect.");

        if (currencyCode != "UAH")
        {
            return Result.Failure("Currently, the base currency of the wallet can only be UAH");
        }

        var wallet = await _context.Wallets.FindAsync(id);
        if (wallet is null)
        {
            return Result.Failure("Wallet not found");
        }

        if (wallet.UserId != request.UserId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user is null)
            {
                return Result.Failure($"User with ID {request.UserId} not found");
            }
            wallet.UserId = user.Id;
        }

        wallet.Name = request.Name.Trim();
        wallet.CurrencyId = await _context.Currencies
            .Where(c => c.Code == currencyCode)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        try 
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating the wallet with id {Id}.", wallet.Id);
            return Result.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(UpdateAsync));
            return Result.Failure("An unexpected system error occurred.");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var wallet = await _context.Wallets
            .Include(w => w.FinancialOperations)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wallet is null)
        {
            return Result.Failure("Wallet not found");
        }

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (wallet.FinancialOperations.Any(o => !o.IsDeleted))
        {
            return Result.Failure("Cannot delete a wallet that has associated financial operations.");
        }

        try 
        { 
            wallet.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting the wallet with id {Id}.", wallet.Id);
            return Result.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(DeleteAsync));
            return Result.Failure("An unexpected system error occurred.");
        }
    }
}
