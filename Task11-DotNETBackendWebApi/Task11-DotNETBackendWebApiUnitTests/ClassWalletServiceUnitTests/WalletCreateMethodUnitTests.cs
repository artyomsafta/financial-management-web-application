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
public class WalletCreateMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<WalletService>> _loggerMock;
    private WalletService _walletService;

    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();

    private static readonly Guid UserNotFoundId = Guid.NewGuid();

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

            var currencyUah = new Currency { Id = CurrencyUahId, Code = "UAH" };

            context.Users.AddRange(user1, user2);
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
    public async Task Test_CreateAsync_PositiveAdminCase(string walletName, string baseCurrency)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateWalletRequest { UserId = User1Id, Name = walletName, BaseCurrency = baseCurrency };

        var walletId = await _walletService.CreateAsync(request);
        var actualWallet = await _context.Wallets.Include(w => w.Currency).Include(w => w.User).SingleAsync(w => w.Id == walletId);

        actualWallet.Name.Should().Be("user1 wallet");
        actualWallet.UserId.Should().Be(User1Id);
        actualWallet.Currency.Id.Should().Be(CurrencyUahId);
        actualWallet.Currency.Code.Should().Be("UAH");
        actualWallet.User.Username.Should().Be("user1");
    }

    [TestMethod]
    public async Task Test_CreateAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new CreateWalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = "UAH" };

        var walletId = await _walletService.CreateAsync(request);
        var actualWallet = await _context.Wallets.Include(w => w.Currency).Include(w => w.User).SingleAsync(w => w.Id == walletId);

        actualWallet.Name.Should().Be("user1 wallet");
        actualWallet.UserId.Should().Be(User1Id);
        actualWallet.Currency.Code.Should().Be("UAH");
        actualWallet.User.Username.Should().Be("user1");
    }

    [TestMethod]
    public async Task Test_CreateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new CreateWalletRequest { UserId = User2Id, Name = "user2 wallet", BaseCurrency = "UAH" };

        Func<Task> action = () => _walletService.CreateAsync(request);
        (await action.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be("Access denied");
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("A")]
    [DataRow("AAAA")]
    public async Task Test_CreateAsync_InvalidBaseCurrencyCase(string failureCurrencyCode)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateWalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = failureCurrencyCode };

        Func<Task> action = () => _walletService.CreateAsync(request);
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
    public async Task Test_CreateAsync_NotUahBaseCurrencyCase(string failureCurrencyCode)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateWalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = failureCurrencyCode };

        Func<Task> action = () => _walletService.CreateAsync(request);
        (await action.Should().ThrowAsync<ValidationException>()).Which.Message.Should().Be("Currently, the base currency of the wallet can only be UAH");
    }

    [TestMethod]
    public async Task Test_CreateAsync_UserNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new CreateWalletRequest { UserId = UserNotFoundId, Name = "user1 wallet", BaseCurrency = "UAH" };

        Func<Task> action = () => _walletService.CreateAsync(request);
        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message.Should().Be($"User with ID {request.UserId} not found");
    }
}

