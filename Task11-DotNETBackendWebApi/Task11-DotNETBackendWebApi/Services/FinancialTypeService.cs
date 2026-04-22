using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class FinancialTypeService : IFinancialTypeService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly ILogger<FinancialTypeService> _logger;

    public FinancialTypeService(AppDbContext context, IUserContext userContext, ILogger<FinancialTypeService> logger)
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

    public async Task<Result<FinancialTypeDto>> GetByIdAsync(Guid id)
    {
        var type = await _context.FinancialTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (type is null)
        {
            return Result<FinancialTypeDto>.Failure($"Type with ID {id} not found");
        }

        return Result<FinancialTypeDto>.Success(MapToDto(type));
    }

    public async Task<Result<FinancialTypeDto>> CreateAsync(FinancialTypeRequest request)
    {
        if (!_userContext.IsAdmin)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        if (await _context.FinancialTypes.AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower()))
        {
            return Result<FinancialTypeDto>.Failure("A financial type with the same name already exists.");
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

            return Result<FinancialTypeDto>.Success(MapToDto(newType));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while creating the financial type {Name}", request.Name);
            return Result<FinancialTypeDto>.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(CreateAsync));
            return Result<FinancialTypeDto>.Failure("An unexpected system error occurred.");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, FinancialTypeRequest request)
    {
        if (!_userContext.IsAdmin)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var type = await _context.FinancialTypes.FindAsync(id);
        if (type is null)
        {
            return Result.Failure("The financial type does not exist.");
        }

        if (await _context.FinancialTypes.AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower()))
        {
            return Result.Failure("The financial type with the same name already exists.");
        }

        type.Name = request.Name.Trim();
        type.Description = request.Description.Trim();
        type.IsIncome = request.IsIncome;

        try
        {
            _context.FinancialTypes.Update(type);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while updating the financial type {Name}", request.Name);
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
        if (!_userContext.IsAdmin)
        {
            throw new UnauthorizedAccessException("Access denied");
        }

        var type = await _context.FinancialTypes
            .Include(t => t.FinancialOperations)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (type is null)
        {
            return Result.Failure("The financial type does not exist.");
        }

        if (type.FinancialOperations.Any(o => !o.IsDeleted))
        {
            return Result.Failure("You cannot delete a type that has financial operations.");
        }

        try
        {
            type.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while deleting the financial type {Name}", type.Name);
            return Result.Failure("Operation aborted due to database connection error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled error occurred in {MethodName} logic.", nameof(DeleteAsync));
            return Result.Failure("An unexpected system error occurred.");
        }
    }

    private FinancialTypeDto MapToDto(FinancialType type)
    {
        return new FinancialTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Description = type.Description,
            IsIncome = type.IsIncome
        };
    }
}
