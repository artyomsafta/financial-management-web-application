using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers.Enums;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassFinancialOperationServiceUnitTests;

[TestClass]
public class OperationDeleteMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ICurrencyRatesService> _ratesServiceMock;
    private Mock<ILogger<FinancialOperationService>> _loggerMock;
    private FinancialOperationService _operationService;

    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();

    private static readonly Guid Wallet1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Id = Guid.NewGuid();

    private static readonly Guid Type1Id = Guid.NewGuid();
    private static readonly Guid Type2Id = Guid.NewGuid();

    private static readonly Guid Wallet1OperationId = Guid.NewGuid();
    private static readonly Guid Wallet2Operation1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Operation2Id = Guid.NewGuid();
    private static readonly Guid Wallet2Operation3Id = Guid.NewGuid();

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

            var type1 = new FinancialType { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true, IsDeleted = false };
            var type2 = new FinancialType { Id = Type2Id, Name = "Rent", Description = "Monthly rent payment", IsIncome = false, IsDeleted = false };

            var wallet1Operation = new FinancialOperation
            {
                Id = Wallet1OperationId,
                Amount = 50_000m,
                Date = new DateTime(2026, 3, 8),
                CurrentCurrency = nameof(Currencies.USD),
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
                CurrentCurrency = nameof(Currencies.USD),
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
                CurrentCurrency = nameof(Currencies.USD),
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
                CurrentCurrency = nameof(Currencies.USD),
                Note = "wallet2 deleted operation",
                IsDeleted = true,
                FinancialTypeId = Type2Id,
                WalletId = Wallet2Id
            };

            context.Users.AddRange(user1, user2);
            context.Wallets.AddRange(wallet1, wallet2);
            context.FinancialTypes.AddRange(type1, type2);
            context.FinancialOperations.AddRange(wallet1Operation, wallet2Operation1, wallet2Operation2, wallet2Operation3);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var operationId = Wallet1OperationId;

        var isDeleteSuccess = await _operationService.SoftDeleteAsync(operationId);
        isDeleteSuccess.Should().BeTrue();

        var deletedOperation = await _context.FinancialOperations.FindAsync(operationId);
        deletedOperation.IsDeleted.Should().BeTrue();

        var walletBalance = await _context.Wallets
            .Where(w => w.Id == Wallet1Id)
            .Select(w => w.Balance)
            .FirstOrDefaultAsync();

        Assert.AreEqual(0m, walletBalance);
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var operationId = Wallet1OperationId;

        var isDeleteSuccess = await _operationService.SoftDeleteAsync(operationId);
        isDeleteSuccess.Should().BeTrue();

        var deletedOperation = await _context.FinancialOperations.FindAsync(operationId);
        deletedOperation.IsDeleted.Should().BeTrue();

        var walletBalance = await _context.Wallets
            .Where(w => w.Id == Wallet1Id)
            .Select(w => w.Balance)
            .FirstOrDefaultAsync();

        Assert.AreEqual(0m, walletBalance);
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_OperationNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var operationId = OperationNotFoundId;

        var isDeleteSuccess = await _operationService.SoftDeleteAsync(operationId);
        isDeleteSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_OperationAlreadyDeletedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var operationId = Wallet2Operation3Id;

        var isDeleteSuccess = await _operationService.SoftDeleteAsync(operationId);
        isDeleteSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var operationId = Wallet2Operation2Id;

        var expectedErrorMessage = "Access denied";

        try
        {
            var isDeleteSuccess = await _operationService.SoftDeleteAsync(operationId);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_NegativeWalletBalanceCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var operationId = Wallet2Operation1Id;

        var expectedErrorMessage = "Operation aborted due to insufficient balance in the wallet";

        try
        {
            var isDeleteSuccess = await _operationService.SoftDeleteAsync(operationId);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
