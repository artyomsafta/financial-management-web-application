using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Shared.Models;
using Shared.Models.DTOs;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassFinancialOperationServiceUnitTests;

[TestClass]
public class OperationCreateMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ICurrencyRatesService> _ratesServiceMock;
    private CurrencyRateResult _defaultRates = new CurrencyRateResult
    {
        PurchaseRate = 42.70m,
        SaleRate = 43.30m
    };
    private Mock<ILogger<FinancialOperationService>> _loggerMock;
    private FinancialOperationService _operationService;

    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();

    private static readonly Guid Wallet1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Id = Guid.NewGuid();

    private static readonly Guid Type1Id = Guid.NewGuid();
    private static readonly Guid Type2Id = Guid.NewGuid();
    private static readonly Guid Type3Id = Guid.NewGuid();

    private static readonly int CurrencyUahId = 1;
    private static readonly int CurrencyUsdId = 2;

    private static readonly Guid WalletNotFoundId = Guid.NewGuid();
    private static readonly Guid TypeNotFoundId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(_options);
        _userContextMock = new Mock<IUserContext>();

        _ratesServiceMock = new Mock<ICurrencyRatesService>();
        var successResult = _defaultRates;
        _ratesServiceMock
            .Setup(s => s.GetRatesAsync(It.Is<string>(c => c == "USD"), It.IsAny<DateTime>()))
            .ReturnsAsync(successResult);

        _loggerMock = new Mock<ILogger<FinancialOperationService>>();
        _operationService = new FinancialOperationService(_context, _userContextMock.Object, _ratesServiceMock.Object, _loggerMock.Object);

        this.SeedMockDb();
    }

    private void SeedMockDb()
    {
        using (var context = new AppDbContext(_options))
        {
            var user1 = new User { Id = User1Id, Username = "user1", Role = UserRoles.User, IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = UserRoles.User, IsDeleted = false };

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", IsDeleted = false, UserId = User1Id, CurrencyId = CurrencyUahId };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "DELETED wallet", IsDeleted = true, UserId = User2Id, CurrencyId = CurrencyUahId };

            var type1 = new FinancialType { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true, IsDeleted = false };
            var type2 = new FinancialType { Id = Type2Id, Name = "Rent", Description = "Monthly rent payment", IsIncome = false, IsDeleted = false };
            var type3 = new FinancialType { Id = Type3Id, Name = "DELETED", Description = "Soft-deleted type", IsIncome = false, IsDeleted = true };

            var currencyUah = new Currency { Id = CurrencyUahId, Code = "UAH" };
            var currencyUsd = new Currency { Id = CurrencyUsdId, Code = "USD" };

            context.Users.AddRange(user1, user2);
            context.Wallets.AddRange(wallet1, wallet2);
            context.FinancialTypes.AddRange(type1, type2, type3);
            context.Currencies.AddRange(currencyUah, currencyUsd);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Success test operation"
        };

        var successResult = await _operationService.CreateAsync(request);
        var actualOperation = await _operationService.GetByIdAsync(successResult);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 4, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 5),
            Currency = new CurrencyListDto { Id = CurrencyUsdId, Code = "USD" },
            Comment = $"The amount in the transaction currency is 10000,0000 USD",
            Note = "Success test operation",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 8_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Success test operation"
        };

        var successResult = await _operationService.CreateAsync(request);
        var actualOperation = await _operationService.GetByIdAsync(successResult);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 4, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 5),
            Currency = new CurrencyListDto { Id = CurrencyUsdId, Code = "USD" },
            Comment = $"The amount in the transaction currency is 8000,0000 USD",
            Note = "Success test operation",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_ExpenseOperationPositiveCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type2Id,
            WalletId = Wallet1Id,
            Amount = 200m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Success test expence operation"
        };

        var successResult = await _operationService.CreateAsync(request);
        var actualOperation = await _operationService.GetByIdAsync(successResult);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = Math.Round(request.Amount * _defaultRates.SaleRate, 4, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 5),
            Currency = new CurrencyListDto { Id = CurrencyUsdId, Code = "USD" },
            Comment = $"The amount in the transaction currency is 200,0000 USD",
            Note = "Success test expence operation",
            Type = new FinancialTypeListDto { Id = Type2Id, Name = "Rent" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };


        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_OperationInBaseWalletCurrencyPositiveCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "UAH",
            Note = "Success test operation in base wallet currency"
        };

        var successResult = await _operationService.CreateAsync(request);
        var actualOperation = await _operationService.GetByIdAsync(successResult);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = new CurrencyListDto { Id = CurrencyUahId, Code = "UAH" },
            Comment = "",
            Note = "Success test operation in base wallet currency",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }


    [DataTestMethod]
    [DataRow("EUR")]
    [DataRow("   EUR")]
    [DataRow("EUR   ")]
    [DataRow("  EUR  ")]
    [DataRow("Eur")]
    [DataRow("   eUR")]
    [DataRow("eur     ")]
    public async Task Test_CreateAsync_AddingNewCurrencyToDbCase(string newCurrencyCode)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        _defaultRates = new CurrencyRateResult
        {
            PurchaseRate = 50.25m,
            SaleRate = 51.25m
        };

        var successRatesResult = _defaultRates;
        _ratesServiceMock
            .Setup(s => s.GetRatesAsync(It.Is<string>(c => c == "EUR"), It.IsAny<DateTime>()))
            .ReturnsAsync(successRatesResult);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = newCurrencyCode,
            Note = "Success test operation with new currency."
        };

        var successResult = await _operationService.CreateAsync(request);
        var actualOperation = await _operationService.GetByIdAsync(successResult);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 4, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 5),
            Currency = new CurrencyListDto { Id = 3, Code = "EUR" },
            Comment = $"The amount in the transaction currency is 10000,0000 EUR",
            Note = "Success test operation with new currency.",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_InvalidCurrentCurrencyCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var failureCurrencyCode = "AAA";
        var failureMessage = $"Exchange rates for '{failureCurrencyCode}' are currently unavailable.";

        _ratesServiceMock
            .Setup(s => s.GetRatesAsync(failureCurrencyCode, It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException(failureMessage));

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = failureCurrencyCode,
            Note = "Test operation with wrong currency code."
        };

        Func<Task> action = () => _operationService.CreateAsync(request);
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(failureMessage);
    }

    [TestMethod]
    public async Task Test_CreateAsync_InvalidDateCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2035, 3, 5),
            Currency = "USD",
            Note = "Test operation with wrong date."
        };

        var operationId = await _operationService.CreateAsync(request);
        (await _context.FinancialOperations.FindAsync(operationId))!.Date.Should().Be(request.Date);
    }

    [TestMethod]
    public async Task Test_CreateAsync_TypeNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateFinOperationRequest
        {
            TypeId = TypeNotFoundId,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Test operation with wrong type."
        };

        Func<Task> action = () => _operationService.CreateAsync(request);
        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message.Should().Be("There is no such type of operation.");
    }

    [TestMethod]
    public async Task Test_CreateAsync_TypeIsDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type3Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Test operation with wrong type."
        };

        Func<Task> action = () => _operationService.CreateAsync(request);
        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message.Should().Be("There is no such type of operation.");
    }

    [TestMethod]
    public async Task Test_CreateAsync_WalletNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = WalletNotFoundId,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Test operation with wrong wallet."
        };

        Func<Task> action = () => _operationService.CreateAsync(request);
        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message.Should().Be("There is no such wallet.");
    }

    [TestMethod]
    public async Task Test_CreateAsync_WalletIsDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet2Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Test operation with wrong wallet."
        };

        Func<Task> action = () => _operationService.CreateAsync(request);
        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message.Should().Be("There is no such wallet.");
    }

    [TestMethod]
    public async Task Test_CreateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var request = new CreateFinOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = "USD",
            Note = "Test operation with wrong user."
        };

        Func<Task> action = () => _operationService.CreateAsync(request);
        (await action.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be("Access denied");
    }   
}


