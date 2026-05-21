using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers;
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
        var query = _context.FinancialOperations
            .AsNoTracking()
            .Where(o => o.Id == id);

        if (!_userContext.IsAdmin)
        {
            query = query.Where(o => o.Wallet.UserId == _userContext.UserId);
        }

        var operation = await query
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
            .FirstOrDefaultAsync();

        if (operation is null)
        {
            return Result<FinancialOperationDto>.Failure($"Operation with ID {id} not found");
        }

        return Result<FinancialOperationDto>.Success(operation);
    }

    public async Task<Result<Guid>> CreateAsync(CreateFinOperationRequest request)
    {
        if (!request.Currency.IsValidCurrencyCode())
            return Result<Guid>.Failure("The currency code is incorrect.");

        var wallet = await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == request.WalletId);
        if (wallet is null)
            return Result<Guid>.Failure("There is no such wallet.");

        var type = await _context.FinancialTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TypeId);
        if (type is null)
            return Result<Guid>.Failure("There is no such type of operation.");

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var baseCurrency = wallet.Currency.Code;
        var currentCurrency = request.Currency.Trim().ToUpper();

        var calculationResult = await CalculateAmountAsync(
            request.Amount, 
            baseCurrency, 
            currentCurrency, 
            request.Date, 
            type.IsIncome
        );

        if (!calculationResult.IsSuccess)
        {
            return Result<Guid>.Failure(calculationResult.Errors);
        }

        var finalAmount = calculationResult.Data;

        var currencyId = await GetOrCreateCurrencyIdAsync(currentCurrency);

        var comment = (baseCurrency == currentCurrency) ? "" : $"The amount in the transaction currency is {request.Amount:F4} {currentCurrency}";

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = finalAmount,
            Date = request.Date,
            Comment = comment,
            Note = request.Note.Trim(),
            IsDeleted = false,
            FinancialTypeId = type.Id,
            WalletId = wallet.Id,
            CurrencyId = currencyId
        };

        try
        {
            _context.FinancialOperations.Add(newOperation);
            await _context.SaveChangesAsync();

            return Result<Guid>.Success(newOperation.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating the financial operation with id {Id}.", newOperation.Id);
            return Result<Guid>.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            return Result<Guid>.Failure("An unexpected system error occurred.");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateFinOperationRequest request)
    {
        if (!request.Currency.IsValidCurrencyCode())
            return Result.Failure("The currency code is incorrect.");

        var operation = await _context.FinancialOperations
            .Include(o => o.Wallet)
                .ThenInclude(w => w.Currency)
            .Include(o => o.Type)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (operation is null)
        {
            return Result.Failure("The financial operation does not exist.");
        }

        if (!_userContext.IsAdmin && operation.Wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var baseCurrency = operation.Wallet.Currency.Code;
        var currentCurrency = request.Currency.Trim().ToUpper();
        var currencyId = await GetOrCreateCurrencyIdAsync(currentCurrency);

        if (operation.FinancialTypeId != request.TypeId)
        {
            var type = await _context.FinancialTypes
                .FirstOrDefaultAsync(t => t.Id == request.TypeId);
            if (type is null)
                return Result.Failure("There is no such type of operation.");

            operation.Type = type;
        }

        var calculationResult = await CalculateAmountAsync(
            request.Amount, 
            baseCurrency, 
            currentCurrency, 
            request.Date, 
            operation.Type.IsIncome
        );

        if (!calculationResult.IsSuccess)
        {
            return Result.Failure(calculationResult.Errors);
        }

        var finalAmount = calculationResult.Data;

        var comment = (baseCurrency == currentCurrency) ? "" : $"The amount in the transaction currency is {request.Amount:F4} {currentCurrency}";

        operation.Amount = finalAmount;
        operation.Date = request.Date;
        operation.Comment = comment;
        operation.Note = request.Note.Trim();
        operation.CurrencyId = currencyId;

        try
        {            
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
