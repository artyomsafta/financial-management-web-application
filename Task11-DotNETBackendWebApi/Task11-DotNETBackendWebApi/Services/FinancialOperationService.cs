using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class FinancialOperationService : IFinancialOperationService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly ICurrencyRatesService _currencyRatesService;
    private readonly ILogger<FinancialOperationService> _logger;

    public FinancialOperationService(AppDbContext context, IUserContext userContext, ICurrencyRatesService currencyRatesService, ILogger<FinancialOperationService> logger)
    {
        _context = context;
        _userContext = userContext;
        _currencyRatesService = currencyRatesService;
        _logger = logger;
    }

    public async Task<IEnumerable<FinancialOperationDto>> GetListAsync()
    {
        var query = _context.FinancialOperations.AsQueryable();

        if (!_userContext.IsAdmin)
        {
            query = query.Where(w => w.Wallet.UserId == _userContext.UserId);
        }

        return await query
            .AsNoTracking()
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .Include(o => o.Currency)
            .Select(o => new FinancialOperationDto
            {
                Id = o.Id,
                Amount = o.Amount,
                Date = o.Date,
                Currency = new CurrencyListDto
                {
                    Id = o.CurrencyId,
                    Code = o.Currency.Code
                },
                Comment = o.Comment,
                Note = o.Note,
                Type = new FinancialTypeListDto
                {
                    Id = o.FinancialTypeId,
                    Name = o.Type.Name
                },
                Wallet = new WalletListDto
                {
                    Id = o.WalletId,
                    Name = o.Wallet.Name
                }
            })
            .ToListAsync();
    }

    public async Task<Result<FinancialOperationDto>> GetByIdAsync(Guid id)
    {
        var operation = await _context.FinancialOperations
            .AsNoTracking()
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .Include(o => o.Currency)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (operation is null)
        {
            return Result<FinancialOperationDto>.Failure($"Operation with ID {id} not found");
        }

        if (!_userContext.IsAdmin && operation.Wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        return Result<FinancialOperationDto>.Success(MapToDto(operation));
    }

    public async Task<Result<FinancialOperationDto>> CreateAsync(FinancialOperationRequest request)
    {
        if (request.Date > DateTime.Now)
            return Result<FinancialOperationDto>.Failure("Cannot create operation in the future.");

        var currencyCode = request.Currency.Trim().ToUpper();
        if (string.IsNullOrEmpty(currencyCode) || currencyCode.Length != 3)
            return Result<FinancialOperationDto>.Failure("The currency code is incorrect.");

        var type = await _context.FinancialTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TypeId);
        if (type is null)
            return Result<FinancialOperationDto>.Failure("There is no such type of operation.");

        var wallet = await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == request.WalletId);
        if (wallet is null)
            return Result<FinancialOperationDto>.Failure("There is no such wallet.");

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var finalAmount = await CalculateAmountAsync(request.Amount, wallet.Currency.Code, currencyCode, request.Date, type.IsIncome);

        await AddCurrencyIfNotExistsAsync(currencyCode);

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = finalAmount,
            Date = request.Date,
            Comment = $"The amount in the transaction currency is {request.Amount:F2} {currencyCode}",
            Note = request.Note.Trim(),
            IsDeleted = false,
            FinancialTypeId = type.Id,
            WalletId = wallet.Id,
            CurrencyId = await _context.Currencies
                .Where(c => c.Code == currencyCode)
                .Select(c => c.Id)
                .FirstOrDefaultAsync()
        };

        try
        {
            _context.FinancialOperations.Add(newOperation);
            await _context.SaveChangesAsync();

            var operation = await _context.FinancialOperations
                .AsNoTracking()
                .Include(o => o.Type)
                .Include(o => o.Wallet)
                .FirstOrDefaultAsync(o => o.Id == newOperation.Id);

            return Result<FinancialOperationDto>.Success(MapToDto(operation));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new financial operation.");
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, FinancialOperationRequest request)
    {
        if (request.Date > DateTime.Now)
            return Result.Failure("Cannot create operation in the future.");

        var currencyCode = request.Currency.Trim().ToUpper();
        if (string.IsNullOrEmpty(currencyCode) || currencyCode.Length != 3)
            return Result.Failure("The currency code is incorrect.");

        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            return Result.Failure("The financial operation does not exist.");
        }

        var wallet = await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == request.WalletId);
        if (wallet is null)
            return Result.Failure("There is no such wallet.");

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (operation.WalletId != request.WalletId)
        {
            return Result.Failure("You have selected the wrong wallet.");
        }

        if (operation.FinancialTypeId != request.TypeId)
        {
            var type = await _context.FinancialTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TypeId);
            if (type is null)
                return Result.Failure("There is no such type of operation.");

            operation.FinancialTypeId = type.Id;
        }

        var isIncome = await _context.FinancialTypes
            .Where(t => t.Id == operation.FinancialTypeId)
            .Select(t => t.IsIncome)
            .FirstOrDefaultAsync();

        var finalAmount = await CalculateAmountAsync(request.Amount, wallet.Currency.Code, currencyCode, request.Date, isIncome);

        await AddCurrencyIfNotExistsAsync(currencyCode);

        operation.Amount = finalAmount;
        operation.Date = request.Date;
        operation.Comment = $"The amount in the transaction currency is {request.Amount:F2} {currencyCode}";
        operation.Note = request.Note.Trim();
        operation.CurrencyId = await _context.Currencies
            .Where(c => c.Code == currencyCode)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        try
        {            
            _context.FinancialOperations.Update(operation);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the financial operation with id {Id}.", operation.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            return Result.Failure("The financial operation does not exist.");
        }

        var wallet = await _context.Wallets.FindAsync(operation.WalletId);

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        operation.IsDeleted = true;

        try
        {
            _context.FinancialOperations.Update(operation);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while soft deleting the financial operation {Id}", operation.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    private async Task<decimal> CalculateAmountAsync(decimal requestAmount, string baseCurrency, string currentCurrency, DateTime requestDate, bool IsIncome)
    {
        if (baseCurrency != currentCurrency)
        {
            var currencyRates = await _currencyRatesService.GetRateAsync(currentCurrency, requestDate);
            var rate = IsIncome ? currencyRates.PurchaseRate : currencyRates.SaleRate;
            requestAmount *= rate;
        }

        return Math.Round(requestAmount, 4, MidpointRounding.AwayFromZero);
    }

    private async Task AddCurrencyIfNotExistsAsync(string currencyCode)
    {
        var exists = await _context.Currencies
            .AsNoTracking()
            .AnyAsync(c => c.Code == currencyCode);

        if (exists) return;

        try
        {
            _context.Currencies.Add(new Currency 
            { 
                Code = currencyCode 
            });
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Currency {Currency} already exists or failed to create.", currencyCode);
        }
    }

    private FinancialOperationDto MapToDto(FinancialOperation operation)
    {
        return new FinancialOperationDto
        {
            Id = operation.Id,
            Amount = operation.Amount,
            Date = operation.Date,
            Currency = new CurrencyListDto
            {
                Id = operation.CurrencyId,
                Code = operation.Currency.Code
            },
            Comment = operation.Comment,
            Note = operation.Note,
            Type = new FinancialTypeListDto
            {
                Id = operation.FinancialTypeId,
                Name = operation.Type.Name
            },
            Wallet = new WalletListDto
            {
                Id = operation.WalletId,
                Name = operation.Wallet.Name
            }
        };
    }
}
