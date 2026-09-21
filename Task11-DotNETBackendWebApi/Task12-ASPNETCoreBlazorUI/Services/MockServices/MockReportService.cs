using Bogus;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services.MockServices;

public class MockReportService : IReportService
{
    private readonly Faker<CurrencyListDto> _currencyFaker;
    private readonly Faker<FinancialTypeListDto> _typeFaker;
    private readonly Faker<WalletListDto> _walletFaker;

    public MockReportService()
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

        _currencyFaker = new Faker<CurrencyListDto>().CustomInstantiator(f => f.PickRandom(currencies));
        _typeFaker = new Faker<FinancialTypeListDto>().CustomInstantiator(f => f.PickRandom(types));
        _walletFaker = new Faker<WalletListDto>().CustomInstantiator(f => f.PickRandom(wallets));

    }

    public Task<ReportDto> GetDailyReportAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        var report = GenerateReport(startOfDay, endOfDay, count: 100);
        return Task.FromResult(report);
    }

    public Task<ReportDto> GetPeriodReportAsync(DateTime startDate, DateTime endDate)
    {
        var report = GenerateReport(startDate, endDate, count: 300);
        return Task.FromResult(report);
    }

    private ReportDto GenerateReport(DateTime from, DateTime to, int count)
    {
        var operationFaker = new Faker<FinancialOperationDto>("en")
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.Amount, f => Math.Round(f.Random.Decimal(-15000m, 35000m), 2))
            .RuleFor(o => o.Date, f => f.Date.Between(from, to))
            .RuleFor(o => o.Currency, _ => _currencyFaker.Generate())
            .RuleFor(o => o.Type, _ => _typeFaker.Generate())
            .RuleFor(o => o.Wallet, _ => _walletFaker.Generate())
            .RuleFor(o => o.Comment, f => f.Commerce.ProductName())
            .RuleFor(o => o.Note, f => f.Lorem.Sentence(4));

        var operations = operationFaker.Generate(count)
            .OrderByDescending(o => o.Date)
            .ToList();

        var totalIncome = operations.Where(o => o.Amount > 0).Sum(o => o.Amount);
        var totalExpenses = Math.Abs(operations.Where(o => o.Amount < 0).Sum(o => o.Amount));

        return new ReportDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetResult = totalIncome - totalExpenses,
            Operations = operations
        };
    }
}
