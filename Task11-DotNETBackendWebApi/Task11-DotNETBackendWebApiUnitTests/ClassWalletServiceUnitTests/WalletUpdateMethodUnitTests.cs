using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassWalletServiceUnitTests;

[TestClass]
public class WalletUpdateMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<WalletService>> _loggerMock;
    private WalletService _walletService;

    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();

    private static readonly Guid Wallet1Id = Guid.NewGuid();

    private static readonly Guid UserNotFoundId = Guid.NewGuid();
    private static readonly Guid WalletNotFoundId = Guid.NewGuid();

    private static readonly int CurrencyUahId = 1;

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
            var user1 = new User { Id = User1Id, Username = "user1", Role = UserRoles.User, IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = UserRoles.User, IsDeleted = false };

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "wallet", IsDeleted = false, UserId = User1Id, CurrencyId = CurrencyUahId };

            var currencyUah = new Currency { Id = CurrencyUahId, Code = "UAH" };

            context.Users.AddRange(user1, user2);
            context.Wallets.Add(wallet1);
            context.Currencies.Add(currencyUah);
            context.SaveChanges();
        }
    }

    [DataTestMethod]
    [DataRow("user1 wallet", "UAH")]
    [DataRow("   user1 wallet", "   UAH")]
    [DataRow("user1 wallet   ", "UAH   ")]
    [DataRow("   user1 wallet   ", "   UAH   ")]
    [DataRow("   user1 wallet   ", "   uah   ")]
    [DataRow("user1 wallet   ", "   uAH   ")]
    public async Task Test_UpdateAsync_PositiveAdminCase(string walletName, string baseCurrency)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var walletId = Wallet1Id;

        var request = new WalletRequest { UserId = User1Id, Name = walletName, BaseCurrency = baseCurrency };

        var successResult = await _walletService.UpdateAsync(walletId, request);
        var isUpdateSuccess = successResult.IsSuccess;
        isUpdateSuccess.Should().BeTrue();

        var expectedWallet = new WalletDto
        {
            Id = walletId,
            Name = "user1 wallet",
            BaseCurrency = new CurrencyListDto { Id = CurrencyUahId, Code = "UAH" },
            User = new UserDto { Id = User1Id, Username = "user1", Role = UserRoles.User }
        };

        var walletEntity = await _context.Wallets
            .Include(w => w.User)
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == walletId);       

        var actualWallet = new WalletDto
        {
            Id = walletEntity.Id,
            Name = walletEntity.Name,
            BaseCurrency = new CurrencyListDto { Id = walletEntity.Currency.Id, Code = walletEntity.Currency.Code },
            User = new UserDto { Id = walletEntity.User.Id, Username = walletEntity.User.Username, Role = walletEntity.User.Role }
        };

        actualWallet.Should().BeEquivalentTo(expectedWallet);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var walletId = Wallet1Id;

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = "UAH" };

        var successResult = await _walletService.UpdateAsync(walletId, request);
        var isUpdateSuccess = successResult.IsSuccess;
        isUpdateSuccess.Should().BeTrue();

        var expectedWallet = new WalletDto
        {
            Id = walletId,
            Name = "user1 wallet",
            BaseCurrency = new CurrencyListDto { Id = CurrencyUahId, Code = "UAH" },
            User = new UserDto { Id = User1Id, Username = "user1", Role = UserRoles.User }
        };

        var walletEntity = await _context.Wallets
            .Include(w => w.User)
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == walletId);

        var actualWallet = new WalletDto
        {
            Id = walletEntity.Id,
            Name = walletEntity.Name,
            BaseCurrency = new CurrencyListDto { Id = walletEntity.Currency.Id, Code = walletEntity.Currency.Code },
            User = new UserDto { Id = walletEntity.User.Id, Username = walletEntity.User.Username, Role = walletEntity.User.Role }
        };

        actualWallet.Should().BeEquivalentTo(expectedWallet);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var walletId = Wallet1Id;

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = "UAH" };

        var expectedErrorMessage = "Access denied";

        try
        {
            var isUpdateSuccess = await _walletService.UpdateAsync(walletId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("A")]
    [DataRow("AAAA")]
    public async Task Test_UpdateAsync_InvalidBaseCurrencyCase(string failureCurrencyCode)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var walletId = Wallet1Id;

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = failureCurrencyCode };

        var failureMessage = "The currency code is incorrect.";

        var failureResult = await _walletService.UpdateAsync(walletId, request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [DataTestMethod]
    [DataRow("usd")]
    [DataRow("   usd")]
    [DataRow("usd   ")]
    [DataRow("UsD")]
    [DataRow("  uSD")]
    [DataRow("  USD   ")]
    [TestMethod]
    public async Task Test_UpdateAsync_NotUahBaseCurrencyCase(string failureCurrencyCode)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var walletId = Wallet1Id;

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = failureCurrencyCode };

        var failureMessage = "Currently, the base currency of the wallet can only be UAH";

        var failureResult = await _walletService.UpdateAsync(walletId, request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_WalletNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var walletId = WalletNotFoundId;

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = "UAH" };

        var failureMessage = "Wallet not found";

        var failureResult = await _walletService.UpdateAsync(walletId, request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UserNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var walletId = Wallet1Id;

        var request = new WalletRequest { UserId = UserNotFoundId, Name = "user1 wallet", BaseCurrency = "UAH" };

        var failureMessage = $"User with ID {request.UserId} not found";

        var failureResult = await _walletService.UpdateAsync(walletId, request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }
}
