using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers.Enums;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

//TODO: inject ICurrencyConversionService and implement logics for currency conversion in CreateAsync and UpdateAsync methods

public class FinancialOperationService : IFinancialOperationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FinancialOperationService> _logger;

    public FinancialOperationService(AppDbContext context, ILogger<FinancialOperationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<FinancialOperationDto>> GetAllAsync()
    {
        return await _context.FinancialOperations
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

        return MapToDto(operation);
    }

    public async Task<FinancialOperationDto> CreateAsync(FinancialOperationRequest request)
    {      
        await EnsureTypeExistsAsync(request.TypeId);
        await EnsureWalletExistsAsync(request.WalletId);
        await EnsureCurrencyIsValidAsync(request.CurrentCurrency);

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount, // TODO: return amount in base currency and implement logics for currency conversion
            Date = request.Date,
            CurrentCurrency = request.CurrentCurrency.Trim().ToUpper(),
            TransactionComment = $"The amount in the transaction currency is {request.Amount} {request.CurrentCurrency.Trim().ToUpper()}",
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
                .Include(o => o.Type)
                .Include(o => o.Wallet)
                .FirstOrDefaultAsync(o => o.Id == newOperation.Id);

            return MapToDto(operation);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new financial operation.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, FinancialOperationRequest request)
    {        
        await EnsureCurrencyIsValidAsync(request.CurrentCurrency);

        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            return false;
        }

        if (operation.FinancialTypeId != request.TypeId)
        {
            await EnsureTypeExistsAsync(request.TypeId);
            operation.FinancialTypeId = request.TypeId;
        }
        if (operation.WalletId != request.WalletId)
        {
            await EnsureWalletExistsAsync(request.WalletId);
            operation.WalletId = request.WalletId;
        }

        try
        {
            operation.Amount = request.Amount; // TODO: return amount in base currency and implement logics for currency conversion
            operation.Date = request.Date;
            operation.CurrentCurrency = request.CurrentCurrency.Trim().ToUpper();
            operation.TransactionComment = $"The amount in the transaction currency is {request.Amount} {request.CurrentCurrency.Trim().ToUpper()}";
            operation.Note = request.Note.Trim();
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the financial operation with id {Id}.", operation.Id);
            throw;
        }
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            return false;
        }

        try
        {
            operation.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while soft deleting the financial operation {Id}", operation.Id);
            throw;
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

        var operations = await _context.FinancialOperations
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

    //TODO: make these methods as extension methods

    private async Task EnsureTypeExistsAsync(Guid typeId)
    {
        var typeExists = await _context.FinancialTypes.AnyAsync(t => t.Id == typeId);
        if (!typeExists)
        {
            throw new InvalidOperationException("The specified type of operation does not exist.");
        }
    }

    private async Task EnsureWalletExistsAsync(Guid walletId)
    {
        var walletExists = await _context.Wallets.AnyAsync(w => w.Id == walletId);
        if (!walletExists)
        {
            throw new InvalidOperationException("The specified wallet does not exist.");
        }   
    }

    private async Task EnsureCurrencyIsValidAsync(string currencyCode)
    {
        if (!Enum.TryParse<Currencies>(currencyCode.Trim().ToUpper(), out _))
        {
            throw new ArgumentException($"The specified currency code: {currencyCode} was not found.");
        }
    }
}
