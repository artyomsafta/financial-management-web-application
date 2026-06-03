using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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

    public async Task<FinancialOperationDto> GetByIdAsync(Guid id)
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
            throw new KeyNotFoundException($"Operation with ID {id} not found");
        }

        return operation;
    }

    public async Task<Guid> CreateAsync(CreateFinOperationRequest request)
    {
        if (!request.Currency.IsValidCurrencyCode())
            throw new ValidationException("The currency code is incorrect.");

        var wallet = await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == request.WalletId);
        if (wallet is null)
            throw new KeyNotFoundException("There is no such wallet.");

        var type = await _context.FinancialTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TypeId);
        if (type is null)
            throw new KeyNotFoundException("There is no such type of operation.");

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var baseCurrency = wallet.Currency.Code;
        var currentCurrency = request.Currency.Trim().ToUpper();

        var calculatedAmount = await CalculateAmountAsync(
            request.Amount, 
            baseCurrency, 
            currentCurrency, 
            request.Date, 
            type.IsIncome
        );

        var currencyId = await GetOrCreateCurrencyIdAsync(currentCurrency);

        var comment = (baseCurrency == currentCurrency) ? "" : $"The amount in the transaction currency is {request.Amount:F4} {currentCurrency}";

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = calculatedAmount,
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

            return newOperation.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating the financial operation with id {Id}.", newOperation.Id);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateFinOperationRequest request)
    {
        if (!request.Currency.IsValidCurrencyCode())
            throw new ValidationException("The currency code is incorrect.");

        var operation = await _context.FinancialOperations
            .Include(o => o.Wallet)
                .ThenInclude(w => w.Currency)
            .Include(o => o.Type)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (operation is null)
        {
            throw new KeyNotFoundException("The financial operation does not exist.");
        }

        if (!_userContext.IsAdmin && operation.Wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (operation.FinancialTypeId != request.TypeId)
        {
            var type = await _context.FinancialTypes
                .FirstOrDefaultAsync(t => t.Id == request.TypeId);
            if (type is null)
                throw new KeyNotFoundException("There is no such type of operation.");

            operation.Type = type;
        }

        var baseCurrency = operation.Wallet.Currency.Code;
        var currentCurrency = request.Currency.Trim().ToUpper();

        var calculatedAmount = await CalculateAmountAsync(
            request.Amount, 
            baseCurrency, 
            currentCurrency, 
            request.Date, 
            operation.Type.IsIncome
        );

        var currencyId = await GetOrCreateCurrencyIdAsync(currentCurrency);

        var comment = (baseCurrency == currentCurrency) ? "" : $"The amount in the transaction currency is {request.Amount:F4} {currentCurrency}";

        operation.Amount = calculatedAmount;
        operation.Date = request.Date;
        operation.Comment = comment;
        operation.Note = request.Note.Trim();
        operation.CurrencyId = currencyId;

        try
        {            
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating the financial operation with id {Id}.", operation.Id);
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
        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            throw new KeyNotFoundException("The financial operation does not exist.");
        }

        var wallet = await _context.Wallets.FindAsync(operation.WalletId);

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        try
        {
            operation.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting the financial operation with id {Id}.", operation.Id);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(DeleteAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    private async Task<decimal> CalculateAmountAsync(
        decimal requestAmount, 
        string baseCurrency, 
        string currentCurrency, 
        DateTime requestDate, 
        bool isIncome
    )
    {
        if (baseCurrency == currentCurrency)
        {
            return Math.Round(requestAmount, 4, MidpointRounding.AwayFromZero);
        }

        var ratesResult = await _currencyRatesService.GetRatesAsync(currentCurrency, requestDate);

        var rate = isIncome ? ratesResult.PurchaseRate : ratesResult.SaleRate;
        var finalAmount = requestAmount * rate;

        return Math.Round(finalAmount, 4, MidpointRounding.AwayFromZero);
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
