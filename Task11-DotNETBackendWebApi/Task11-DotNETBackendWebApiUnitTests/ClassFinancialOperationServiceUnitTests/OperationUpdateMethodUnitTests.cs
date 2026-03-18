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
public class OperationUpdateMethodUnitTests
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
    private static readonly Guid Wallet3Id = Guid.NewGuid();

    private static readonly Guid Type1Id = Guid.NewGuid();
    private static readonly Guid Type2Id = Guid.NewGuid();
    private static readonly Guid Type3Id = Guid.NewGuid();
    private static readonly Guid Type4Id = Guid.NewGuid();

    private static readonly Guid Wallet1OperationId = Guid.NewGuid();
    private static readonly Guid Wallet2Operation1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Operation2Id = Guid.NewGuid();
    private static readonly Guid Wallet2Operation3Id = Guid.NewGuid();

    private static readonly Guid WalletNotFoundId = Guid.NewGuid();
    private static readonly Guid TypeNotFoundId = Guid.NewGuid();
    private static readonly Guid OperationNotFoundId = Guid.NewGuid();

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

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", Balance = 50_000m, BaseCurrency = nameof(Currencies.UAH), IsDeleted = false, UserId = User1Id };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "user2 wallet", Balance = 20_000m, BaseCurrency = nameof(Currencies.UAH), IsDeleted = false, UserId = User2Id };
            var wallet3 = new Wallet { Id = Wallet3Id, Name = "DELETED wallet", Balance = 0m, BaseCurrency = nameof(Currencies.UAH), IsDeleted = true, UserId = User2Id };

            var type1 = new FinancialType { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true, IsDeleted = false };
            var type2 = new FinancialType { Id = Type2Id, Name = "Rent", Description = "Monthly rent payment", IsIncome = false, IsDeleted = false };
            var type3 = new FinancialType { Id = Type3Id, Name = "DELETED", Description = "Soft-deleted type", IsIncome = false, IsDeleted = true };
            var type4 = new FinancialType { Id = Type4Id, Name = "Groceries", Description = "Food and household items", IsIncome = false, IsDeleted = false };

            var wallet1Operation = new FinancialOperation
            { 
                Id = Wallet1OperationId, 
                Amount = 50_000m, 
                Date = new DateTime(2026, 3, 8), 
                Currency = nameof(Currencies.USD),
                Note = "wallet1 income operation", 
                IsDeleted = false, 
                FinancialTypeId = Type1Id, 
                WalletId = Wallet1Id 
            };

            var wallet2Operation1 = new FinancialOperation
            {
                Id = Wallet2Operation1Id,
                Amount = 30_000m,
                Date = new DateTime(2026, 3, 7),
                Currency = nameof(Currencies.USD),
                Note = "wallet2 income operation",
                IsDeleted = false,
                FinancialTypeId = Type1Id,
                WalletId = Wallet2Id
            };

            var wallet2Operation2 = new FinancialOperation
            {
                Id = Wallet2Operation2Id,
                Amount = 10_000m,
                Date = new DateTime(2026, 3, 8),
                Currency = nameof(Currencies.USD),
                Note = "wallet2 expence operation",
                IsDeleted = false,
                FinancialTypeId = Type2Id,
                WalletId = Wallet2Id
            };

            var wallet2Operation3 = new FinancialOperation
            {
                Id = Wallet2Operation3Id,
                Amount = 15_000m,
                Date = new DateTime(2026, 3, 7),
                Currency = nameof(Currencies.USD),
                Note = "wallet2 deleted operation",
                IsDeleted = true,
                FinancialTypeId = Type2Id,
                WalletId = Wallet2Id
            };

            context.Users.AddRange(user1, user2);
            context.Wallets.AddRange(wallet1, wallet2, wallet3);
            context.FinancialTypes.AddRange(type1, type2, type3, type4);
            context.FinancialOperations.AddRange(wallet1Operation, wallet2Operation1, wallet2Operation2, wallet2Operation3);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Success test operation update"
        };

        var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
        isUpdateSuccess.Should().BeTrue();

        var expectedOperation = new FinancialOperationDto
        {
            Id = operationId,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 2, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 9),
            Currency = nameof(Currencies.USD),
            Comment = "The amount in the transaction currency is 3000,00 USD",
            Note = "Success test operation update",
            TypeId = Type1Id,
            TypeName = "Salary",
            WalletId = Wallet1Id,
            WalletName = "user1 wallet"
        };

        var operationEntity = await _context.FinancialOperations
            .Where(o => o.Id == operationId)
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .FirstOrDefaultAsync();

        var actualOperation = new FinancialOperationDto
        {
            Id = operationEntity.Id,
            Amount = operationEntity.Amount,
            Date = operationEntity.Date,
            Currency = operationEntity.Currency,
            Comment = operationEntity.Comment,
            Note = operationEntity.Note,
            TypeId = operationEntity.FinancialTypeId,
            TypeName = operationEntity.Type.Name,
            WalletId = operationEntity.WalletId,
            WalletName = operationEntity.Wallet.Name
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 2_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Success test operation update"
        };

        var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
        isUpdateSuccess.Should().BeTrue();

        var expectedOperation = new FinancialOperationDto
        {
            Id = operationId,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 2, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 9),
            Currency = nameof(Currencies.USD),
            Comment = "The amount in the transaction currency is 2000,00 USD",
            Note = "Success test operation update",
            TypeId = Type1Id,
            TypeName = "Salary",
            WalletId = Wallet1Id,
            WalletName = "user1 wallet"
        };

        var operationEntity = await _context.FinancialOperations
            .Where(o => o.Id == operationId)
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .FirstOrDefaultAsync();

        var actualOperation = new FinancialOperationDto
        {
            Id = operationEntity.Id,
            Amount = operationEntity.Amount,
            Date = operationEntity.Date,
            Currency = operationEntity.Currency,
            Comment = operationEntity.Comment,
            Note = operationEntity.Note,
            TypeId = operationEntity.FinancialTypeId,
            TypeName = operationEntity.Type.Name,
            WalletId = operationEntity.WalletId,
            WalletName = operationEntity.Wallet.Name
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_ChangeTypePositiveCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var operationId = Wallet2Operation2Id;
        var request = new FinancialOperationRequest
        {
            TypeId = Type4Id,
            WalletId = Wallet2Id,
            Amount = 100m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Successful attempt to update the operation type"
        };

        var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
        isUpdateSuccess.Should().BeTrue();

        var expectedValue = "Groceries";

        var actualValue = await _context.FinancialOperations
            .Where(o => o.Id == operationId)
            .Include(o => o.Type)
            .Select(o => o.Type.Name)
            .FirstOrDefaultAsync();

        actualValue.Should().BeEquivalentTo(expectedValue);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_ExpenseOperationPositiveCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var operationId = Wallet2Operation2Id;
        var request = new FinancialOperationRequest
        {
            TypeId = Type2Id,
            WalletId = Wallet2Id,
            Amount = 200m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Success test expence operation update"
        };

        var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
        isUpdateSuccess.Should().BeTrue();

        var expectedOperation = new FinancialOperationDto
        {
            Id = operationId,
            Amount = Math.Round(request.Amount * _defaultRates.SaleRate, 2, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 9),
            Currency = nameof(Currencies.USD),
            Comment = "The amount in the transaction currency is 200,00 USD",
            Note = "Success test expence operation update",
            TypeId = Type2Id,
            TypeName = "Rent",
            WalletId = Wallet2Id,
            WalletName = "user2 wallet"
        };

        var operationEntity = await _context.FinancialOperations
            .Where(o => o.Id == operationId)
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .FirstOrDefaultAsync();

        var actualOperation = new FinancialOperationDto
        {
            Id = operationEntity.Id,
            Amount = operationEntity.Amount,
            Date = operationEntity.Date,
            Currency = operationEntity.Currency,
            Comment = operationEntity.Comment,
            Note = operationEntity.Note,
            TypeId = operationEntity.FinancialTypeId,
            TypeName = operationEntity.Type.Name,
            WalletId = operationEntity.WalletId,
            WalletName = operationEntity.Wallet.Name
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_InvalidOperationNewCurrencyCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = "AAA",
            Note = "Test operation with wrong currency code."
        };

        var expectedErrorMessage = $"The specified currency code: {request.CurrentCurrency} was not found.";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_InvalidDateCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 3_000m,
            Date = new DateTime(2036, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong date."
        };

        var expectedErrorMessage = "The specified date cannot be in the future.";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_OperationNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = OperationNotFoundId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong Id."
        };

        var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
        isUpdateSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task Test_UpdateAsync_OperationIsDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var operationId = Wallet2Operation3Id;
        var request = new FinancialOperationRequest
        {
            TypeId = Type2Id,
            WalletId = Wallet2Id,
            Amount = 300m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Attempt to update a deleted operation"
        };

        var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
        isUpdateSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task Test_UpdateAsync_TypeNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = TypeNotFoundId,
            WalletId = Wallet1Id,
            Amount = 2_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong type."
        };

        var expectedErrorMessage = "The specified type of operation does not exist.";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_TypeIsDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type3Id,
            WalletId = Wallet1Id,
            Amount = 2_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong type."
        };

        var expectedErrorMessage = "The specified type of operation does not exist.";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_WalletNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = WalletNotFoundId,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with non-found wallet."
        };

        var expectedErrorMessage = "The specified wallet does not exist.";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_WalletIsDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet3Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with deleted wallet."
        };

        var expectedErrorMessage = "The specified wallet does not exist.";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_WrongWalletCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet2Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong wallet."
        };

        var expectedErrorMessage = "You have selected the wrong wallet.";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with wrong user."
        };

        var expectedErrorMessage = "Access denied";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_NoExchangeRatesCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Test operation with missing exchange rates."
        };

        var expectedErrorMessage = $"No exchange rates available for this currency: {request.CurrentCurrency}";

        _ratesServiceMock
            .Setup(s => s.GetRateAsync(It.Is<string>(c => c == "USD"), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException(expectedErrorMessage));

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_ExpenseOperationNegativeWalletBalanceCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var operationId = Wallet2Operation2Id;
        var request = new FinancialOperationRequest
        {
            TypeId = Type2Id,
            WalletId = Wallet2Id,
            Amount = 1_000m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Attempt to update an expence operation that causes a negative wallet balance."
        };

        var expectedErrorMessage = "Operation aborted due to insufficient balance in the wallet";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_UpdateAsync_IncomeOperationNegativeWalletBalanceCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var operationId = Wallet2Operation1Id;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet2Id,
            Amount = 100m,
            Date = new DateTime(2026, 3, 9),
            CurrentCurrency = nameof(Currencies.USD),
            Note = "Attempt to update an income operation that causes a negative wallet balance."
        };

        var expectedErrorMessage = "Operation aborted due to insufficient balance in the wallet";

        try
        {
            var isUpdateSuccess = await _operationService.UpdateAsync(operationId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
