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

    public FinancialOperationService(
        AppDbContext context, 
        IUserContext userContext, 
        ICurrencyRatesService currencyRatesService, 
        ILogger<FinancialOperationService> logger
    )
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
            //.Include(o => o.Type)
            //.Include(o => o.Wallet)
            //.Include(o => o.Currency)
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

        var calculationResult = await CalculateAmountAsync(request.Amount, wallet.Currency.Code, currencyCode, request.Date, type.IsIncome);
        if (!calculationResult.IsSuccess)
        {
            return Result<FinancialOperationDto>.Failure(calculationResult.Errors);
        }
        var finalAmount = calculationResult.Data;

        await AddCurrencyIfNotExistsAsync(currencyCode);

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = finalAmount,
            Date = request.Date,
            Comment = $"The amount in the transaction currency is {request.Amount:F4} {currencyCode}",
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
                .Include(o => o.Currency)
                .FirstOrDefaultAsync(o => o.Id == newOperation.Id);

            return Result<FinancialOperationDto>.Success(MapToDto(operation));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating the financial operation with id {Id}.", newOperation.Id);
            return Result<FinancialOperationDto>.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            return Result<FinancialOperationDto>.Failure("An unexpected system error occurred.");
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

        var calculationResult = await CalculateAmountAsync(request.Amount, wallet.Currency.Code, currencyCode, request.Date, isIncome);
        if (!calculationResult.IsSuccess)
        {
            return Result.Failure(calculationResult.Errors);
        }
        var finalAmount = calculationResult.Data;

        await AddCurrencyIfNotExistsAsync(currencyCode);

        operation.Amount = finalAmount;
        operation.Date = request.Date;
        operation.Comment = $"The amount in the transaction currency is {request.Amount:F4} {currencyCode}";
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
            _logger.LogError(ex, "Database error occurred while updating the financial operation with id {Id}.", operation.Id);
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
            _logger.LogError(ex, "Database error occurred while deleting the financial operation with id {Id}.", operation.Id);
            return Result.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(DeleteAsync));
            return Result.Failure("An unexpected system error occurred.");
        }
    }

    private async Task<Result<decimal>> CalculateAmountAsync(
        decimal requestAmount, 
        string baseCurrency, 
        string currentCurrency, 
        DateTime requestDate, 
        bool isIncome
    )
    {
        if (baseCurrency == currentCurrency)
        {
            return Result<decimal>.Success(Math.Round(requestAmount, 4, MidpointRounding.AwayFromZero));
        }

        var ratesResult = await _currencyRatesService.GetRatesAsync(currentCurrency, requestDate);

        if (!ratesResult.IsSuccess)
        {
            return Result<decimal>.Failure(ratesResult.Errors);
        }

        var rate = isIncome ? ratesResult.Data.PurchaseRate : ratesResult.Data.SaleRate;
        var finalAmount = requestAmount * rate;

        return Result<decimal>.Success(Math.Round(finalAmount, 4, MidpointRounding.AwayFromZero));
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred while adding currency {Currency}.", currencyCode);
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
