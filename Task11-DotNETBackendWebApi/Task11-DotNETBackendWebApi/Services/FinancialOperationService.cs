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
            .Select(o => new FinancialOperationDto
            {
                Id = o.Id,
                Amount = o.Amount,
                Date = o.Date,
                Note = o.Note,
                TypeId = o.FinancialTypeId,
                TypeName = o.Type.Name
            })
            .ToListAsync();
    }

    public async Task<FinancialOperationDto?> GetByIdAsync(Guid id)
    {
        var operation = await _context.FinancialOperations
            .Include(o => o.Type)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (operation is null)
        {
            return null;
        }

        return new FinancialOperationDto
        {
            Id = operation.Id,
            Amount = operation.Amount,
            Date = operation.Date,
            Note = operation.Note,
            TypeId = operation.FinancialTypeId,
            TypeName = operation.Type.Name
        };
    }

    public async Task<FinancialOperationDto> CreateAsync(FinancialOperationRequest request)
    {
        var typeExists = await _context.FinancialTypes.AnyAsync(t => t.Id == request.TypeId);
        if (!typeExists)
        {
            throw new InvalidOperationException("The specified type of operation does not exist.");
        }

        var newOperation = new FinancialOperation
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Date = request.Date,
            Note = request.Note,
            FinancialTypeId = request.TypeId,
            IsDeleted = false
        };

        try
        {
            _context.FinancialOperations.Add(newOperation);
            await _context.SaveChangesAsync();

            return new FinancialOperationDto
            {
                Id = newOperation.Id,
                Amount = newOperation.Amount,
                Date = newOperation.Date,
                Note = newOperation.Note,
                TypeId = newOperation.FinancialTypeId,
                TypeName = (await _context.FinancialTypes.FindAsync(newOperation.FinancialTypeId))?.Name ?? string.Empty
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new financial operation.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, FinancialOperationRequest request)
    {
        var operation = await _context.FinancialOperations.FindAsync(id);
        if (operation is null)
        {
            return false;
        }

        if (operation.FinancialTypeId != request.TypeId)
        {
            var typeExists = await _context.FinancialTypes.AnyAsync(t => t.Id == request.TypeId);
            if (!typeExists)
            {
                throw new InvalidOperationException("The specified type of operation does not exist.");
            }

            operation.FinancialTypeId = request.TypeId;
        }

        try
        {
            operation.Amount = request.Amount;
            operation.Date = request.Date;
            operation.Note = request.Note;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the financial operation.");
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
            _logger.LogError(ex, "An error occurred while soft deleting the financial operation.");
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
                    Note = o.Note,
                    TypeId = o.FinancialTypeId,
                    TypeName = o.Type.Name
                })
                .OrderByDescending(o => o.Date)
                .ToList()
        };
    }
}
