using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Shared.Models;
using Shared.Models.DTOs;
using System.ComponentModel.DataAnnotations;
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

        var request = new UpdateWalletRequest { Name = walletName, BaseCurrency = baseCurrency };

        var successResult = await _walletService.UpdateAsync(walletId, request);
        var isUpdateSuccess = successResult;
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

        var request = new UpdateWalletRequest { Name = "user1 wallet", BaseCurrency = "UAH" };

        var successResult = await _walletService.UpdateAsync(walletId, request);
        var isUpdateSuccess = successResult;
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

        var request = new UpdateWalletRequest { Name = "user1 wallet", BaseCurrency = "UAH" };

        Func<Task> action = () => _walletService.UpdateAsync(walletId, request);
        (await action.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be("Access denied");
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

        var request = new UpdateWalletRequest { Name = "user1 wallet", BaseCurrency = failureCurrencyCode };

        Func<Task> action = () => _walletService.UpdateAsync(walletId, request);
        (await action.Should().ThrowAsync<ValidationException>()).Which.Message.Should().Be("The currency code is incorrect.");
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

        var request = new UpdateWalletRequest { Name = "user1 wallet", BaseCurrency = failureCurrencyCode };

        Func<Task> action = () => _walletService.UpdateAsync(walletId, request);
        (await action.Should().ThrowAsync<ValidationException>()).Which.Message.Should().Be("Currently, the base currency of the wallet can only be UAH");
    }

    [TestMethod]
    public async Task Test_UpdateAsync_WalletNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var walletId = WalletNotFoundId;

        var request = new UpdateWalletRequest { Name = "user1 wallet", BaseCurrency = "UAH" };

        Func<Task> action = () => _walletService.UpdateAsync(walletId, request);
        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message.Should().Be("Wallet not found");
    }

    [TestMethod]
    public async Task Test_UpdateAsync_DoesNotReassignOrValidateRequestUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var walletId = Wallet1Id;

        var request = new UpdateWalletRequest { Name = "user1 wallet", BaseCurrency = "UAH" };

        (await _walletService.UpdateAsync(walletId, request)).Should().BeTrue();
        (await _context.Wallets.FindAsync(walletId))!.UserId.Should().Be(User1Id);
    }
}


