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

    private static readonly int CurrencyUahId = 1;
    private static readonly int CurrencyUsdId = 2;

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
        var successResult = Result<CurrencyRateResult>.Success(_defaultRates);
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
            var user1 = new User { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User), IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = nameof(UserRoles.User), IsDeleted = false };

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", IsDeleted = false, UserId = User1Id, CurrencyId = CurrencyUahId };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "user2 wallet", IsDeleted = false, UserId = User2Id, CurrencyId = CurrencyUahId };
            var wallet3 = new Wallet { Id = Wallet3Id, Name = "DELETED wallet", IsDeleted = true, UserId = User2Id, CurrencyId = CurrencyUahId };

            var type1 = new FinancialType { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true, IsDeleted = false };
            var type2 = new FinancialType { Id = Type2Id, Name = "Rent", Description = "Monthly rent payment", IsIncome = false, IsDeleted = false };
            var type3 = new FinancialType { Id = Type3Id, Name = "DELETED", Description = "Soft-deleted type", IsIncome = false, IsDeleted = true };
            var type4 = new FinancialType { Id = Type4Id, Name = "Groceries", Description = "Food and household items", IsIncome = false, IsDeleted = false };

            var currencyUah = new Currency { Id = CurrencyUahId, Code = "UAH" };
            var currencyUsd = new Currency { Id = CurrencyUsdId, Code = "USD" };

            var wallet1Operation = new FinancialOperation
            { 
                Id = Wallet1OperationId, 
                Amount = 50_000m, 
                Date = new DateTime(2026, 3, 8), 
                Note = "wallet1 income operation", 
                IsDeleted = false, 
                FinancialTypeId = Type1Id, 
                WalletId = Wallet1Id,
                CurrencyId = CurrencyUsdId
            };

            var wallet2Operation1 = new FinancialOperation
            {
                Id = Wallet2Operation1Id,
                Amount = 30_000m,
                Date = new DateTime(2026, 3, 7),
                Note = "wallet2 income operation",
                IsDeleted = false,
                FinancialTypeId = Type1Id,
                WalletId = Wallet2Id,
                CurrencyId = CurrencyUsdId
            };

            var wallet2Operation2 = new FinancialOperation
            {
                Id = Wallet2Operation2Id,
                Amount = 10_000m,
                Date = new DateTime(2026, 3, 8),
                Note = "wallet2 expence operation",
                IsDeleted = false,
                FinancialTypeId = Type2Id,
                WalletId = Wallet2Id,
                CurrencyId = CurrencyUsdId
            };

            var wallet2Operation3 = new FinancialOperation
            {
                Id = Wallet2Operation3Id,
                Amount = 15_000m,
                Date = new DateTime(2026, 3, 7),
                Note = "wallet2 deleted operation",
                IsDeleted = true,
                FinancialTypeId = Type2Id,
                WalletId = Wallet2Id,
                CurrencyId = CurrencyUsdId
            };

            context.Users.AddRange(user1, user2);
            context.Wallets.AddRange(wallet1, wallet2, wallet3);
            context.FinancialTypes.AddRange(type1, type2, type3, type4);
            context.Currencies.AddRange(currencyUah, currencyUsd);
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
            Currency = "USD",
            Note = "Success test operation update"
        };

        var successResult = await _operationService.UpdateAsync(operationId, request);
        var isUpdateSuccess = successResult.IsSuccess;
        isUpdateSuccess.Should().BeTrue();

        var expectedOperation = new FinancialOperationDto
        {
            Id = operationId,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 4, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 9),
            Currency = new CurrencyListDto { Id = CurrencyUsdId, Code = "USD" },
            Comment = "The amount in the transaction currency is 3000,0000 USD",
            Note = "Success test operation update",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        var operationEntity = await _context.FinancialOperations
            .Where(o => o.Id == operationId)
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .Include(o => o.Currency)
            .FirstOrDefaultAsync();

        var actualOperation = new FinancialOperationDto
        {
            Id = operationEntity.Id,
            Amount = operationEntity.Amount,
            Date = operationEntity.Date,
            Currency = new CurrencyListDto { Id = operationEntity.Currency.Id, Code = operationEntity.Currency.Code },
            Comment = operationEntity.Comment,
            Note = operationEntity.Note,
            Type = new FinancialTypeListDto { Id = operationEntity.FinancialTypeId, Name = operationEntity.Type.Name },
            Wallet = new WalletListDto { Id = operationEntity.WalletId, Name = operationEntity.Wallet.Name }
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
            Currency = "USD",
            Note = "Success test operation update"
        };

        var successResult = await _operationService.UpdateAsync(operationId, request);
        var isUpdateSuccess = successResult.IsSuccess;
        isUpdateSuccess.Should().BeTrue();

        var expectedOperation = new FinancialOperationDto
        {
            Id = operationId,
            Amount = Math.Round(request.Amount * _defaultRates.PurchaseRate, 4, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 9),
            Currency = new CurrencyListDto { Id = CurrencyUsdId, Code = "USD" },
            Comment = "The amount in the transaction currency is 2000,0000 USD",
            Note = "Success test operation update",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        var operationEntity = await _context.FinancialOperations
            .Where(o => o.Id == operationId)
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .Include(o => o.Currency)
            .FirstOrDefaultAsync();

        var actualOperation = new FinancialOperationDto
        {
            Id = operationEntity.Id,
            Amount = operationEntity.Amount,
            Date = operationEntity.Date,
            Currency = new CurrencyListDto { Id = operationEntity.Currency.Id, Code = operationEntity.Currency.Code },
            Comment = operationEntity.Comment,
            Note = operationEntity.Note,
            Type = new FinancialTypeListDto { Id = operationEntity.FinancialTypeId, Name = operationEntity.Type.Name },
            Wallet = new WalletListDto { Id = operationEntity.WalletId, Name = operationEntity.Wallet.Name }
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
            Currency = "USD",
            Note = "Successful attempt to update the operation type"
        };

        var successResult = await _operationService.UpdateAsync(operationId, request);
        var isUpdateSuccess = successResult.IsSuccess;
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
            Currency = "USD",
            Note = "Success test expence operation update"
        };

        var successResult = await _operationService.UpdateAsync(operationId, request);
        var isUpdateSuccess = successResult.IsSuccess;
        isUpdateSuccess.Should().BeTrue();

        var expectedOperation = new FinancialOperationDto
        {
            Id = operationId,
            Amount = Math.Round(request.Amount * _defaultRates.SaleRate, 4, MidpointRounding.AwayFromZero),
            Date = new DateTime(2026, 3, 9),
            Currency = new CurrencyListDto { Id = CurrencyUsdId, Code = "USD" },
            Comment = "The amount in the transaction currency is 200,0000 USD",
            Note = "Success test expence operation update",
            Type = new FinancialTypeListDto { Id = Type2Id, Name = "Rent" },
            Wallet = new WalletListDto { Id = Wallet2Id, Name = "user2 wallet" }
        };

        var operationEntity = await _context.FinancialOperations
            .Where(o => o.Id == operationId)
            .Include(o => o.Type)
            .Include(o => o.Wallet)
            .Include(o => o.Currency)
            .FirstOrDefaultAsync();

        var actualOperation = new FinancialOperationDto
        {
            Id = operationEntity.Id,
            Amount = operationEntity.Amount,
            Date = operationEntity.Date,
            Currency = new CurrencyListDto { Id = operationEntity.Currency.Id, Code = operationEntity.Currency.Code },
            Comment = operationEntity.Comment,
            Note = operationEntity.Note,
            Type = new FinancialTypeListDto { Id = operationEntity.FinancialTypeId, Name = operationEntity.Type.Name },
            Wallet = new WalletListDto { Id = operationEntity.WalletId, Name = operationEntity.Wallet.Name }
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_InvalidOperationNewCurrencyCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var failureCurrencyCode = "AAA";
        var failureMessage = $"Exchange rates for '{failureCurrencyCode}' are currently unavailable.";

        _ratesServiceMock
            .Setup(s => s.GetRatesAsync(failureCurrencyCode, It.IsAny<DateTime>()))
            .ReturnsAsync(Result<CurrencyRateResult>.Failure(failureMessage));

        var operationId = Wallet1OperationId;
        var request = new FinancialOperationRequest
        {
            TypeId = Type1Id,
            WalletId = Wallet1Id,
            Amount = 3_000m,
            Date = new DateTime(2026, 3, 9),
            Currency = failureCurrencyCode,
            Note = "Test operation with wrong currency code."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
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
            Currency = "USD",
            Note = "Test operation with wrong date."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "Cannot create operation in the future." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
            Note = "Test operation with wrong Id."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "The financial operation does not exist." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
            Note = "Attempt to update a deleted operation"
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "The financial operation does not exist." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
            Note = "Test operation with wrong type."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "There is no such type of operation." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
            Note = "Test operation with wrong type."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "There is no such type of operation." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
            Note = "Test operation with non-found wallet."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "There is no such wallet." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
            Note = "Test operation with deleted wallet."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "There is no such wallet." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
            Note = "Test operation with wrong wallet."
        };

        var failureResult = await _operationService.UpdateAsync(operationId, request);
        var actualFailureMessages = failureResult.Errors;

        var expectedErrorMessage = new List<string> { "You have selected the wrong wallet." };

        failureResult.IsSuccess.Should().BeFalse();
        actualFailureMessages.Should().BeEquivalentTo(expectedErrorMessage);
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
            Currency = "USD",
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
}
