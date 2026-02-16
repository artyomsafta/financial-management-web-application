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
    private readonly ILogger<FinancialTypeService> _logger;

    public FinancialTypeService(AppDbContext context, ILogger<FinancialTypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<FinancialTypeDto>> GetAllAsync()
    {
        return await _context.FinancialTypes
            .Select(t => new FinancialTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                IsIncome = t.IsIncome
            })
            .ToListAsync();
    }

    public async Task<FinancialTypeDto?> GetByIdAsync(Guid id)
    {
        var type = await _context.FinancialTypes.FindAsync(id);
        if (type is null)
        {
            return null;
        }

        return MapToDto(type);
    }

    public async Task<FinancialTypeDto> CreateAsync(FinancialTypeRequest request)
    {
        await EnsureTypeNameNotTakenAync(request.Name);

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

            return MapToDto(newType);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while creating the financial type {Name}", request.Name);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, FinancialTypeRequest request)
    {
        await EnsureTypeNameNotTakenAync(request.Name);

        var existingType = await _context.FinancialTypes.FindAsync(id);
        if (existingType is null)
        {
            return false;
        }

        try
        {
            existingType.Name = request.Name.Trim();
            existingType.Description = request.Description.Trim();
            existingType.IsIncome = request.IsIncome;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "An error occurred while updating the financial type {Name}", request.Name);
            throw;
        }
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var type = await _context.FinancialTypes
            .Include(t => t.FinancialOperations)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (type is null)
        {
            return false;
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
            _logger.LogError(ex, "An error occurred while soft deleting the financial type {Name}", type.Name);
            throw;
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

    private async Task EnsureTypeNameNotTakenAync(string typeName)
    {
        if (await _context.FinancialTypes.AnyAsync(t => t.Name.ToLower() == typeName.Trim().ToLower()))
        {
            throw new InvalidOperationException("A financial type with the same name already exists.");
        }
    }
}
