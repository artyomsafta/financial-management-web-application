using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class FinancialTypeService : IFinancialTypeService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly ILogger<FinancialTypeService> _logger;

    public FinancialTypeService(
        AppDbContext context, 
        IUserContext userContext, 
        ILogger<FinancialTypeService> logger
    )
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<IEnumerable<FinancialTypeDto>> GetListAsync()
    {
        return await _context.FinancialTypes
            .AsNoTracking()
            .Select(t => new FinancialTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsIncome = t.IsIncome
            })
            .ToListAsync();
    }

    public async Task <FinancialTypeDto> GetByIdAsync(Guid id)
    {
        var type = await _context.FinancialTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type is null)
        {
            throw new KeyNotFoundException($"Type with ID {id} not found");
        }

        return type.MapToFinTypeDto();
    }

    public async Task<Guid> CreateAsync(FinancialTypeRequest request)
    {
        if (!_userContext.IsAdmin)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (await _context.FinancialTypes.AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower()))
        {
            throw new InvalidOperationException("A financial type with the same name already exists.");
        }

        var newType = new FinancialType
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            IsIncome = request.IsIncome
        };

        try
        {
            _context.FinancialTypes.Add(newType);
            await _context.SaveChangesAsync();

            return newType.Id;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating the financial type {Name}", request.Name);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }

    public async Task<bool> UpdateAsync(Guid id, FinancialTypeRequest request)
    {
        if (!_userContext.IsAdmin)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var type = await _context.FinancialTypes.FindAsync(id);
        if (type is null)
        {
            throw new KeyNotFoundException("The financial type does not exist.");
        }

        if (await _context.FinancialTypes.AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower()))
        {
            throw new InvalidOperationException("The financial type with the same name already exists.");
        }

        type.Name = request.Name.Trim();
        type.Description = request.Description.Trim();
        type.IsIncome = request.IsIncome;

        try
        {
            _context.FinancialTypes.Update(type);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating the financial type {Name}", request.Name);
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
        if (!_userContext.IsAdmin)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var type = await _context.FinancialTypes
            .Include(t => t.FinancialOperations)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (type is null)
        {
            throw new KeyNotFoundException("The financial type does not exist.");
        }

        if (type.FinancialOperations.Any(o => !o.IsDeleted))
        {
            throw new InvalidOperationException("You cannot delete a type that has financial operations.");
        }

        try
        {
            type.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting the financial type {Name}", type.Name);
            throw new DbUpdateException("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(DeleteAsync));
            throw new Exception("An unexpected system error occurred.");
        }
    }
}
