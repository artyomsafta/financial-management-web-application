using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassWalletServiceUnitTests;

[TestClass]
public class WalletDeleteMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<WalletService>> _loggerMock;
    private WalletService _walletService;

    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();
    private static readonly Guid User3Id = Guid.NewGuid();

    private static readonly Guid Wallet1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Id = Guid.NewGuid();
    private static readonly Guid Wallet3Id = Guid.NewGuid();

    private static readonly int CurrencyUahId = 1;
    private static readonly int CurrencyUsdId = 2;

    private static readonly Guid WalletNotFoundId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(_options);
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<ILogger<WalletService>>();
        _walletService = new WalletService(_context, _userContextMock.Object, _loggerMock.Object);

        this.SeedMockDb();
    }

    private void SeedMockDb()
    {
        using (var context = new AppDbContext(_options))
        {
            var user1 = new User { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User), IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = nameof(UserRoles.User), IsDeleted = false };
            var user3 = new User { Id = User3Id, Username = "user3", Role = nameof(UserRoles.User), IsDeleted = false };

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", IsDeleted = false, UserId = User1Id, CurrencyId = CurrencyUahId };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "user2 wallet", IsDeleted = false, UserId = User2Id, CurrencyId = CurrencyUahId };
            var wallet3 = new Wallet { Id = Wallet3Id, Name = "user3 wallet", IsDeleted = false, UserId = User3Id, CurrencyId = CurrencyUahId };

            var currencyUah = new Currency { Id = CurrencyUahId, Code = "UAH" };
            var currencyUsd = new Currency { Id = CurrencyUsdId, Code = "USD" };

            var wallet1Operation = new FinancialOperation
            {
                Id = Guid.NewGuid(),
                Amount = 100m,
                Date = DateTime.UtcNow,
                Note = "deleted operation for positive tests",
                IsDeleted = true,
                FinancialTypeId = Guid.NewGuid(),
                WalletId = Wallet1Id,
                CurrencyId = CurrencyUahId
            };

            var wallet3Operation = new FinancialOperation
            {
                Id = Guid.NewGuid(),
                Amount = 150m,
                Date = DateTime.UtcNow,
                Note = "existing operation. You can't delete my wallet!",
                IsDeleted = false,
                FinancialTypeId = Guid.NewGuid(),
                WalletId = Wallet3Id,
                CurrencyId = CurrencyUahId
            };

            context.Users.AddRange(user1, user2, user3);
            context.Wallets.AddRange(wallet1, wallet2, wallet3);
            context.Currencies.AddRange(currencyUah, currencyUsd);
            context.FinancialOperations.AddRange(wallet1Operation, wallet3Operation);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var walletId = Wallet1Id;

        var successResult = await _walletService.DeleteAsync(walletId);
        var isDeleteSuccess = successResult.IsSuccess;
        isDeleteSuccess.Should().BeTrue();

        var deletedWallet = await _context.Wallets.FindAsync(walletId);
        deletedWallet.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var walletId = Wallet1Id;

        var successResult = await _walletService.DeleteAsync(walletId);
        var isDeleteSuccess = successResult.IsSuccess;
        isDeleteSuccess.Should().BeTrue();

        var deletedWallet = await _context.Wallets.FindAsync(walletId);
        deletedWallet.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveCaseHasNoOperations()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var walletId = Wallet2Id;

        var successResult = await _walletService.DeleteAsync(walletId);
        var isDeleteSuccess = successResult.IsSuccess;
        isDeleteSuccess.Should().BeTrue();

        var deletedWallet = await _context.Wallets.FindAsync(walletId);
        deletedWallet.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_WalletNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var walletId = WalletNotFoundId;

        var failureResult = await _walletService.DeleteAsync(walletId);
        var isDeleteSuccess = failureResult.IsSuccess;
        isDeleteSuccess.Should().BeFalse();

        var failureMessage = "Wallet not found";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_DeleteAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var walletId = Wallet1Id;

        var expectedErrorMessage = "Access denied";

        try
        {
            var isDeleteSuccess = await _walletService.DeleteAsync(walletId);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_DeleteAsync_WalletHasOperationsCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User3Id);
        var walletId = Wallet3Id;

        var failureResult = await _walletService.DeleteAsync(walletId);
        var isDeleteSuccess = failureResult.IsSuccess;
        isDeleteSuccess.Should().BeFalse();

        var failureMessage = "Cannot delete a wallet that has associated financial operations.";
        failureResult.Errors.Should().Contain(failureMessage);
    }
}
