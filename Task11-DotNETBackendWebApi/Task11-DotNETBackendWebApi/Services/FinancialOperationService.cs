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

    public async Task<IEnumerable<FinancialOperationDto>> GetAllAsync()
    {
        var query = _context.FinancialOperations.AsQueryable();

        if (!_userContext.IsAdmin)
        {
            query = query.Where(w => w.Wallet.UserId == _userContext.UserId);
        }

        return await query
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .Select(o => new FinancialOperationDto
            {
                Id = o.Id,
                Amount = o.Amount,
                Date = o.Date,
                CurrentCurrency = o.CurrentCurrency,
                TransactionComment = o.TransactionComment,
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
        request.CurrentCurrency.EnsureCurrencyIsValid();
        request.Date.EnsureDateIsAcceptable();
        await _context.FinancialTypes.EnsureTypeExistsAsync(request.TypeId);
        await _context.Wallets.EnsureWalletExistsAsync(request.WalletId);

        var wallet = await _context.Wallets.FindAsync(request.WalletId);

        if (!_userContext.IsAdmin && wallet.UserId != _userContext.UserId)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var currentCurrency = request.CurrentCurrency.Trim().ToUpper();

        var isIncomeOperation = await _context.FinancialTypes
            .Where(t => t.Id == request.TypeId)
            .Select(t => t.IsIncome)
            .FirstOrDefaultAsync();

        var finalAmount = await CalculateAmount(request.Amount, wallet.BaseCurrency, currentCurrency, request.Date, isIncomeOperation);

        if (!isIncomeOperation)
        {
            if (wallet.Balance < finalAmount)
            {
                throw new InvalidOperationException("Operation aborted due to insufficient balance in the wallet");
            }
        }

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = finalAmount,
            Date = request.Date,
            CurrentCurrency = currentCurrency,
            TransactionComment = $"The amount in the transaction currency is {request.Amount:F2} {currentCurrency}",
            Note = request.Note.Trim(),
            IsDeleted = false,
            FinancialTypeId = request.TypeId,
            WalletId = request.WalletId
        };

        wallet.Balance += (isIncomeOperation ? finalAmount : -finalAmount);

        try
        {
            _context.FinancialOperations.Add(newOperation);
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            var operation = await _context.FinancialOperations
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
        request.CurrentCurrency.EnsureCurrencyIsValid();
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

        var currentCurrency = request.CurrentCurrency.Trim().ToUpper();

        var isIncomeOperation = await _context.FinancialTypes
            .Where(t => t.Id == operation.FinancialTypeId)
            .Select(t => t.IsIncome)
            .FirstOrDefaultAsync();

        var finalAmount = await CalculateAmount(request.Amount, wallet.BaseCurrency, currentCurrency, request.Date, isIncomeOperation);

        if (isIncomeOperation)
        {
            wallet.Balance -= operation.Amount;

            if ((wallet.Balance + finalAmount) < 0)
            {
                throw new InvalidOperationException("Operation aborted due to insufficient balance in the wallet");
            }
        }
        else
        {
            wallet.Balance += operation.Amount;
            
            if (wallet.Balance < finalAmount)
            {
                throw new InvalidOperationException("Operation aborted due to insufficient balance in the wallet");
            }
        }

        operation.Amount = finalAmount;
        operation.Date = request.Date;
        operation.CurrentCurrency = currentCurrency;
        operation.TransactionComment = $"The amount in the transaction currency is {request.Amount:F2} {currentCurrency}";
        operation.Note = request.Note.Trim();

        wallet.Balance += (isIncomeOperation ? finalAmount : -finalAmount);

        try
        {            
            _context.FinancialOperations.Update(operation);
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the financial operation with id {Id}.", operation.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
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

        var isIncomeOperation = await _context.FinancialTypes
            .Where(t => t.Id == operation.FinancialTypeId)
            .Select(t => t.IsIncome)
            .FirstOrDefaultAsync();

        if (isIncomeOperation)
        {
            if (wallet.Balance < operation.Amount)
            {
                throw new InvalidOperationException("Operation aborted due to insufficient balance in the wallet");
            }
        }

        wallet.Balance += (isIncomeOperation ? -operation.Amount : operation.Amount);
        operation.IsDeleted = true;

        try
        {
            _context.FinancialOperations.Update(operation);
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while soft deleting the financial operation {Id}", operation.Id);
            throw new InvalidOperationException("Operation aborted due to database connection error");
        }
    }

    public async Task<ReportDto> GetDailyReportAsync(DateTime date)
    {
        return await GetPeriodReportAsync(date, date);
    }

    public async Task<ReportDto> GetPeriodReportAsync(DateTime start, DateTime end)
    {
        if (start > end)
        {
            throw new ArgumentException("The start date must be earlier or equal to the end date.");
        }

        var startDate = start.Date;
        var endDate = end.Date.AddDays(1).AddTicks(-1);

        var query = _context.FinancialOperations.AsQueryable();

        if (!_userContext.IsAdmin)
        {
            query = query.Where(w => w.Wallet.UserId == _userContext.UserId);
        }

        var operations = await query
                .Include(o => o.Type)
                .Include(o => o.Wallet)
                .Where(o => o.Date >= startDate && o.Date <= endDate)
                .ToListAsync();

        var totalIncome = operations
                .Where(o => o.Type.IsIncome is true)
                .Sum(o => o.Amount);

        var totalExpenses = operations
                .Where(o => o.Type.IsIncome is false)
                .Sum(o => o.Amount);

        return new ReportDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetResult = totalIncome - totalExpenses,
            Operations = operations
                .Select(o => new FinancialOperationDto
                {
                    Id = o.Id,
                    Amount = o.Amount,
                    Date = o.Date,
                    CurrentCurrency = o.CurrentCurrency,
                    TransactionComment = o.TransactionComment,
                    Note = o.Note,
                    TypeId = o.FinancialTypeId,
                    TypeName = o.Type.Name,
                    WalletId = o.WalletId,
                    WalletName = o.Wallet.Name
                })
                .OrderByDescending(o => o.Date)
                .ToList()
        };
    }

    private async Task<decimal> CalculateAmount(decimal requestAmount, string baseCurrency, string currentCurrency, DateTime requestDate, bool IsIncome)
    {
        if (baseCurrency != currentCurrency)
        {
            var currencyRates = await _currencyRatesService.GetRateAsync(currentCurrency, requestDate);
            var rate = IsIncome ? currencyRates.PurchaseRate : currencyRates.SaleRate;
            requestAmount *= rate;
        }

        return Math.Round(requestAmount, 2, MidpointRounding.AwayFromZero);
    }

    private FinancialOperationDto MapToDto(FinancialOperation operation)
    {
        return new FinancialOperationDto
        {
            Id = operation.Id,
            Amount = operation.Amount,
            Date = operation.Date,
            CurrentCurrency = operation.CurrentCurrency,
            TransactionComment = operation.TransactionComment,
            Note = operation.Note,
            TypeId = operation.FinancialTypeId,
            TypeName = operation.Type.Name,
            WalletId = operation.WalletId,
            WalletName = operation.Wallet.Name
        };
    }
}
