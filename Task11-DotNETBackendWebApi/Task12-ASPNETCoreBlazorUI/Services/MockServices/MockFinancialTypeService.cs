using Bogus;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services.MockServices;

public class MockFinancialTypeService : IFinancialTypeService
{
    private readonly List<FinancialTypeDto> _financialTypes;

    public MockFinancialTypeService()
    {
        var finTypeFaker = new Faker<FinancialTypeDto>("en")
            .RuleFor(t => t.Id, f => Guid.NewGuid())
            .RuleFor(t => t.Name, f => f.Finance.TransactionType())
            .RuleFor(t => t.Description, f => f.Lorem.Sentence(3))
            .RuleFor(t => t.IsIncome, f => f.Random.Bool());

        _financialTypes = finTypeFaker.Generate(30);
    }

    public Task<List<FinancialTypeDto>> GetListAsync()
    {
        return Task.FromResult(_financialTypes
            .OrderBy(t => t.Name)
            .ToList());
    }

    public Task<FinancialTypeDto> GetByIdAsync(Guid id)
    {
        var financialType = _financialTypes.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(financialType ?? new FinancialTypeDto());
    }

    public Task<ApiResponseDto> CreateAsync(FinancialTypeDto model)
    {
        var newFinancialType = new FinancialTypeDto
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            IsIncome = model.IsIncome
        };
        _financialTypes.Add(newFinancialType);

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Financial type successfully created (Mock)"
        });
    }

    public Task<ApiResponseDto> UpdateAsync(Guid id, FinancialTypeDto model)
    {
        var existingFinancialType = _financialTypes.FirstOrDefault(t => t.Id == id);
        if (existingFinancialType != null)
        {
            existingFinancialType.Name = model.Name;
            existingFinancialType.Description = model.Description;
            existingFinancialType.IsIncome = model.IsIncome;
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Financial type successfully updated (Mock)"
        });
    }

    public Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        var financialType = _financialTypes.FirstOrDefault(t => t.Id == id);
        if (financialType != null)
        {
            _financialTypes.Remove(financialType);
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Financial type successfully deleted (Mock)"
        });
    }
}
