using Bogus;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services.MockServices;

public class MockFinancialOperationService : IFinancialOperationService
{
    private readonly List<FinancialOperationDto> _financialOperations;

    public MockFinancialOperationService()
    {
        var currencies = new[]
        {
            new CurrencyListDto { Id = 0, Code = "USD", },
            new CurrencyListDto { Id = 1, Code = "EUR", },
            new CurrencyListDto { Id = 2, Code = "GBP", },
            new CurrencyListDto { Id = 3, Code = "JPY", },
            new CurrencyListDto { Id = 4, Code = "AUD", },
            new CurrencyListDto { Id = 5, Code = "CAD", },
        };

        var types = new[]
        {
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Services Payment" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Software Subscription" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Consulting Fees" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Marketing & Ads" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Office Rent" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Utilities" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Travel Expenses" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Employee Salaries" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Equipment Purchase" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Training & Development" },
            new FinancialTypeListDto { Id = Guid.NewGuid(), Name = "Insurance Premiums" },
        };

        var wallets = new[]
        {
            new WalletListDto { Id = Guid.NewGuid(), Name = "Main Corporate Account" },
            new WalletListDto { Id = Guid.NewGuid(), Name = "Business Credit Card" },
            new WalletListDto { Id = Guid.NewGuid(), Name = "Operating Cash" },
            new WalletListDto { Id = Guid.NewGuid(), Name = "Savings Account" },
            new WalletListDto { Id = Guid.NewGuid(), Name = "Investment Account" },
        };

        var finOpFaker = new Faker<FinancialOperationDto>("en")
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.Amount, f => Math.Round(f.Random.Decimal(-15000m, 35000m), 2))
            .RuleFor(o => o.Date, f => f.Date.Past())
            .RuleFor(o => o.Currency, f => f.PickRandom(currencies))
            .RuleFor(o => o.Comment, f => f.Lorem.Sentence(4))
            .RuleFor(o => o.Note, f => f.Lorem.Paragraph())
            .RuleFor(o => o.Type, f => f.PickRandom(types))
            .RuleFor(o => o.Wallet, f => f.PickRandom(wallets));

        _financialOperations = finOpFaker.Generate(200);
    }

    public Task<List<FinancialOperationDto>> GetListAsync()
    {
        return Task.FromResult(_financialOperations
            .OrderByDescending(f => f.Date)
            .ToList());
    }

    public Task<FinancialOperationDto> GetByIdAsync(Guid id)
    {
        var finOp = _financialOperations.FirstOrDefault(o => o.Id == id);
        return Task.FromResult(finOp ?? new FinancialOperationDto());
    }

    public Task<ApiResponseDto> CreateAsync(FinancialOperationDto model)
    {
        var newFinOp = new FinancialOperationDto
        {
            Id = Guid.NewGuid(),
            Amount = model.Amount,
            Date = model.Date,
            Currency = model.Currency,
            Comment = model.Comment,
            Note = model.Note,
            Type = model.Type,
            Wallet = model.Wallet
        };

        _financialOperations.Add(newFinOp);

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Financial operation successfully created (Mock)"
        });
    }

    public Task<ApiResponseDto> UpdateAsync(Guid id, FinancialOperationDto model)
    {
        var existingFinOp = _financialOperations.FirstOrDefault(o => o.Id == id);
        if (existingFinOp != null)
        {
            existingFinOp.Amount = model.Amount;
            existingFinOp.Date = model.Date;
            existingFinOp.Currency = model.Currency;
            existingFinOp.Comment = model.Comment;
            existingFinOp.Note = model.Note;
            existingFinOp.Type = model.Type;
            existingFinOp.Wallet = model.Wallet;
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Financial operation successfully updated (Mock)"
        });
    }

    public Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        var finOp = _financialOperations.FirstOrDefault(o => o.Id == id);
        if (finOp != null)
        {
            _financialOperations.Remove(finOp);
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Financial operation successfully deleted (Mock)"
        });
    }
}
