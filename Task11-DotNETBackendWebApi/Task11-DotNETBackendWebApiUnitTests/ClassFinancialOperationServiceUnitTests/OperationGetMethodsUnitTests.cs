using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassFinancialOperationServiceUnitTests;

[TestClass]
public class OperationGetMethodsUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ICurrencyRatesService> _ratesServiceMock;
    private Mock<ILogger<FinancialOperationService>> _loggerMock;
    private FinancialOperationService _operationService;

    private static readonly Guid AdminUserId = Guid.NewGuid();
    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();

    private static readonly Guid AdminWalletId = Guid.NewGuid();
    private static readonly Guid Wallet1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Id = Guid.NewGuid();

    private static readonly Guid Type1Id = Guid.NewGuid();

    private static readonly int CurrencyUahId = 1;
    private static readonly int CurrencyUsdId = 2;

    private static readonly Guid AdminOperationId = Guid.NewGuid();
    private static readonly Guid Wallet1OperationId = Guid.NewGuid();
    private static readonly Guid Wallet2OperationId = Guid.NewGuid();
    private static readonly Guid DeletedOperationId = Guid.NewGuid();



    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(_options);
        _userContextMock = new Mock<IUserContext>();
        _ratesServiceMock = new Mock<ICurrencyRatesService>();
        _loggerMock = new Mock<ILogger<FinancialOperationService>>();
        _operationService = new FinancialOperationService(_context, _userContextMock.Object, _ratesServiceMock.Object, _loggerMock.Object);

        this.SeedMockDb();
    }

    private void SeedMockDb()
    {
        using (var context = new AppDbContext(_options))
        {
            var adminUser = new User { Id = AdminUserId, Username = "__REMOVED_BOOTSTRAP_ADMIN_USERNAME__", Role = nameof(UserRoles.Admin), IsDeleted = false };
            var user1 = new User { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User), IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = nameof(UserRoles.User), IsDeleted = false };

            var adminWallet = new Wallet { Id = AdminWalletId, Name = "admin wallet", IsDeleted = false, UserId = AdminUserId };
            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", IsDeleted = false, UserId = User1Id };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "user2 wallet", IsDeleted = false, UserId = User2Id };

            var type1 = new FinancialType { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true, IsDeleted = false };

            var currencyUah = new Currency { Id = CurrencyUahId, Code = "UAH" };
            var currencyUsd = new Currency { Id = CurrencyUsdId, Code = "USD" };

            var walletAdminOperation = new FinancialOperation
            { 
                Id = AdminOperationId, 
                Amount = 250m, 
                Date = new DateTime(2026, 3, 1), 
                Note = "walletAdmin operation", 
                IsDeleted = false, 
                FinancialTypeId = Type1Id, 
                WalletId = AdminWalletId,
                CurrencyId = CurrencyUahId,
            };

            var wallet1Operation = new FinancialOperation
            { 
                Id = Wallet1OperationId, 
                Amount = 150m, 
                Date = new DateTime(2026, 3, 1), 
                Note = "wallet1 operation", 
                IsDeleted = false, 
                FinancialTypeId = Type1Id, 
                WalletId = Wallet1Id,
                CurrencyId = CurrencyUahId
            };

            var wallet2Operation = new FinancialOperation
            { 
                Id = Wallet2OperationId, 
                Amount = 120m, 
                Date = new DateTime(2026, 3, 1),
                Note = "wallet2 operation", 
                IsDeleted = false, 
                FinancialTypeId = Type1Id, 
                WalletId = Wallet2Id,
                CurrencyId = CurrencyUahId
            };

            var deletedOperation = new FinancialOperation
            { 
                Id = DeletedOperationId, 
                Amount = 300m, 
                Date = new DateTime(2026, 3, 1), 
                Note = "deleted operation", 
                IsDeleted = true, 
                FinancialTypeId = Type1Id, 
                WalletId = Wallet2Id,
                CurrencyId = CurrencyUahId
            };

            context.Users.AddRange(adminUser, user1, user2);
            context.Wallets.AddRange(adminWallet, wallet1, wallet2);
            context.FinancialTypes.Add(type1);
            context.Currencies.AddRange(currencyUah, currencyUsd);
            context.FinancialOperations.AddRange(walletAdminOperation, wallet1Operation, wallet2Operation, deletedOperation);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveAdminCases()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var operations = await _operationService.GetListAsync();
        Assert.HasCount(3, operations.ToList());
        Assert.IsTrue(operations.Any(o => o.Note == "walletAdmin operation"));
        Assert.IsTrue(operations.Any(o => o.Note == "wallet1 operation"));
        Assert.IsTrue(operations.Any(o => o.Note == "wallet2 operation"));
        Assert.IsFalse(operations.Any(o => o.Note == "deleted operation"));
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveUserCases()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var operations = await _operationService.GetListAsync();
        Assert.HasCount(1, operations.ToList());
        Assert.IsTrue(operations.Any(o => o.Note == "wallet1 operation"));
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var successResult = await _operationService.GetByIdAsync(Wallet1OperationId);
        var actualOperation = successResult.Data;

        var expectedOperation = new FinancialOperationDto
        {
            Id = Wallet1OperationId,
            Amount = 150m,
            Date = new DateTime(2026, 3, 1),
            Currency = new CurrencyListDto { Id = CurrencyUahId, Code = "UAH" },
            Note = "wallet1 operation",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var successResult = await _operationService.GetByIdAsync(Wallet1OperationId);
        var actualOperation = successResult.Data;

        var expectedOperation = new FinancialOperationDto
        {
            Id = Wallet1OperationId,
            Amount = 150m,
            Date = new DateTime(2026, 3, 1),
            Currency = new CurrencyListDto { Id = CurrencyUahId, Code = "UAH" },
            Note = "wallet1 operation",
            Type = new FinancialTypeListDto { Id = Type1Id, Name = "Salary" },
            Wallet = new WalletListDto { Id = Wallet1Id, Name = "user1 wallet" }
        };

        actualOperation.Should().BeEquivalentTo(expectedOperation);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_OperationNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var failureResult = await _operationService.GetByIdAsync(DeletedOperationId);
        var isFindSuccess = failureResult.IsSuccess;
        isFindSuccess.Should().BeFalse();

        var failureMessage = $"Operation with ID {DeletedOperationId} not found";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var expectedErrorMessage = "Access denied";

        try
        {
            var wrongOperation = await _operationService.GetByIdAsync(Wallet2OperationId);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
