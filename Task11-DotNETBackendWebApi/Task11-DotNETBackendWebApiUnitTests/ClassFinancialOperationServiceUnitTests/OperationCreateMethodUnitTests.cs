using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers.Enums;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
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
        _ratesServiceMock
            .Setup(s => s.GetRateAsync(It.Is<string>(c => c == "USD"), It.IsAny<DateTime>()))
            .ReturnsAsync(_defaultRates);

        _loggerMock = new Mock<ILogger<FinancialOperationService>>();
        _operationService = new FinancialOperationService(_context, _userContextMock.Object, _ratesServiceMock.Object, _loggerMock.Object);

        this.SeedMockDb();
    }

    private void SeedMockDb()
    {
        using (var context = new AppDbContext(_options))
        {
            var user1 = new User { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User), IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = nameof(UserRoles.User), IsDeleted = false };

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", Balance = 15_000m, BaseCurrency = nameof(Currencies.UAH), IsDeleted = false, UserId = User1Id };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "DELETED wallet", Balance = 20_000m, BaseCurrency = nameof(Currencies.UAH), IsDeleted = true, UserId = User2Id };

            var type1 = new FinancialType { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true, IsDeleted = false };
            var type2 = new FinancialType { Id = Type2Id, Name = "Rent", Description = "Monthly rent payment", IsIncome = false, IsDeleted = false };
            var type3 = new FinancialType { Id = Type3Id, Name = "DELETED", Description = "Soft-deleted type", IsIncome = false, IsDeleted = true };

            context.Users.AddRange(user1, user2);
            context.Wallets.AddRange(wallet1, wallet2);
            context.FinancialTypes.AddRange(type1, type2, type3);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Success test operation"
        };
        var actualOperation = await _operationService.CreateAsync(request);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 2, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 5),
            Currency = nameof(Currencies.USD),
            Comment = $"The amount in the transaction currency is 10000,00 USD",
            Note = "Success test operation",
            TypeId = Type1Id,
            TypeName = "Salary",
            WalletId = Wallet1Id,
            WalletName = "user1 wallet"
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 8_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Success test operation"
        };
        var actualOperation = await _operationService.CreateAsync(request);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 2, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 5),
            Currency = nameof(Currencies.USD),
            Comment = $"The amount in the transaction currency is 8000,00 USD",
            Note = "Success test operation",
            TypeId = Type1Id,
            TypeName = "Salary",
            WalletId = Wallet1Id,
            WalletName = "user1 wallet"
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_ExpenseOperationPositiveCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new FinancialOperationRequest
        {
            TypeId = Type2Id,
            WalletId = Wallet1Id,
            Amount = 200m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Success test expence operation"
        };
        var actualOperation = await _operationService.CreateAsync(request);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = Math.Round(request.Amount * _defaultRates.SaleRate, 2, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 5),
            Currency = nameof(Currencies.USD),
            Comment = $"The amount in the transaction currency is 200,00 USD",
            Note = "Success test expence operation",
            TypeId = Type2Id,
            TypeName = "Rent",
            WalletId = Wallet1Id,
            WalletName = "user1 wallet"
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_OperationInBaseWalletCurrencyPositiveCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var baseWalletCurrency = await _context.Wallets
            .Where(w => w.Id == Wallet1Id)
            .Select(w => w.BaseCurrency)
            .FirstOrDefaultAsync();

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = baseWalletCurrency,
            Note = "Success test operation in base wallet currency"
        };
        var actualOperation = await _operationService.CreateAsync(request);

        var expectedOperation = new FinancialOperationDto
        {
            Id = actualOperation.Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            Currency = nameof(Currencies.UAH),
            Comment = $"The amount in the transaction currency is 10000,00 UAH",
            Note = "Success test operation in base wallet currency",
            TypeId = Type1Id,
            TypeName = "Salary",
            WalletId = Wallet1Id,
            WalletName = "user1 wallet"
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_CreateAsync_InvalidCurrentCurrencyCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = "AAA",
            Note = "Test operation with wrong currency code."
        };

        var expectedErrorMessage = $"The specified currency code: {request.CurrentCurrency} was not found.";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_InvalidDateCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2035, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong date."
        };

        var expectedErrorMessage = "The specified date cannot be in the future.";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_TypeNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = TypeNotFoundId,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong type."
        };

        var expectedErrorMessage = "The specified type of operation does not exist.";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_TypeIsDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type3Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong type."
        };

        var expectedErrorMessage = "The specified type of operation does not exist.";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_WalletNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = WalletNotFoundId,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong wallet."
        };

        var expectedErrorMessage = "The specified wallet does not exist.";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_WalletIsDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet2Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong wallet."
        };

        var expectedErrorMessage = "The specified wallet does not exist.";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong user."
        };

        var expectedErrorMessage = "Access denied";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_NoExchangeRatesCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with missing exchange rates."
        };

        var expectedErrorMessage = $"No exchange rates available for this currency: {request.CurrentCurrency}";

        _ratesServiceMock
            .Setup(s => s.GetRateAsync(It.Is<string>(c => c == "USD"), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException(expectedErrorMessage));

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_NegativeWalletBalanceCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialOperationRequest
        {
            TypeId = Type2Id,
            WalletId = Wallet1Id,
            Amount = 10_000m,
            Date = new DateTime(2026, 3, 5),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation that causes a negative wallet balance."
        };

        var expectedErrorMessage = "Operation aborted due to insufficient balance in the wallet";

        try
        {
            var newOperation = await _operationService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
