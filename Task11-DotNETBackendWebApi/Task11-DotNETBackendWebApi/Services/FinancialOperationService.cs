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
            .Select(o => new FinancialOperationDto
            {
                Id = o.Id,
                Amount = o.Amount,
                Date = o.Date,
                Currency = o.Currency,
                Comment = o.Comment,
                Note = o.Note,
                TypeId = o.FinancialTypeId,
                TypeName = o.Type.Name,
                WalletId = o.WalletId,
                WalletName = o.Wallet.Name
            })
            .ToListAsync();
    }

    public async Task<FinancialOperationDto?> GetByIdAsync(Guid id)
    {
        var operation = await _context.FinancialOperations
            .AsNoTracking()
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (operation is null)
        {
            return null;
        }

        if (!_userContext.IsAdmin && operation.Wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        return MapToDto(operation);
    }

    public async Task<FinancialOperationDto> CreateAsync(FinancialOperationRequest request)
    {
        request.Currency.EnsureCurrencyIsValid();
        request.Date.EnsureDateIsAcceptable();
        await _context.FinancialTypes.EnsureTypeExistsAsync(request.TypeId);
        await _context.Wallets.EnsureWalletExistsAsync(request.WalletId);

        var wallet = await _context.Wallets.FindAsync(request.WalletId);

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var currentCurrency = request.Currency.Trim().ToUpper();

        var isIncomeOperation = await _context.FinancialTypes
            .Where(t => t.Id == request.TypeId)
            .Select(t => t.IsIncome)
            .FirstOrDefaultAsync();

        var finalAmount = await CalculateAmountAsync(request.Amount, wallet.BaseCurrency, currentCurrency, request.Date, isIncomeOperation);

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = finalAmount,
            Date = request.Date,
            Currency = currentCurrency,
            Comment = $"The amount in the transaction currency is {request.Amount:F2} {currentCurrency}",
            Note = request.Note.Trim(),
            IsDeleted = false,
            FinancialTypeId = request.TypeId,
            WalletId = request.WalletId
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

            return MapToDto(operation);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new financial operation.");
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> UpdateAsync(Guid id, FinancialOperationRequest request)
    {        
        request.Currency.EnsureCurrencyIsValid();
        request.Date.EnsureDateIsAcceptable();

        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            return false;
        }

        await _context.Wallets.EnsureWalletExistsAsync(request.WalletId);
        var wallet = await _context.Wallets.FindAsync(operation.WalletId);

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (operation.WalletId != request.WalletId)
        {
            throw new InvalidOperationException("You have selected the wrong wallet.");
        }

        if (operation.FinancialTypeId != request.TypeId)
        {
            await _context.FinancialTypes.EnsureTypeExistsAsync(request.TypeId);
            operation.FinancialTypeId = request.TypeId;
        }

        var currentCurrency = request.Currency.Trim().ToUpper();

        var isIncomeOperation = await _context.FinancialTypes
            .Where(t => t.Id == operation.FinancialTypeId)
            .Select(t => t.IsIncome)
            .FirstOrDefaultAsync();

        var finalAmount = await CalculateAmountAsync(request.Amount, wallet.BaseCurrency, currentCurrency, request.Date, isIncomeOperation);

        operation.Amount = finalAmount;
        operation.Date = request.Date;
        operation.Currency = currentCurrency;
        operation.Comment = $"The amount in the transaction currency is {request.Amount:F2} {currentCurrency}";
        operation.Note = request.Note.Trim();

        try
        {            
            _context.FinancialOperations.Update(operation);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the financial operation with id {Id}.", operation.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> DleteAsync(Guid id)
    {
        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            return false;
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

            return true;
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

    private FinancialOperationDto MapToDto(FinancialOperation operation)
    {
        return new FinancialOperationDto
        {
            Id = operation.Id,
            Amount = operation.Amount,
            Date = operation.Date,
            Currency = operation.Currency,
            Comment = operation.Comment,
            Note = operation.Note,
            TypeId = operation.FinancialTypeId,
            TypeName = operation.Type.Name,
            WalletId = operation.WalletId,
            WalletName = operation.Wallet.Name
        };
    }
}
