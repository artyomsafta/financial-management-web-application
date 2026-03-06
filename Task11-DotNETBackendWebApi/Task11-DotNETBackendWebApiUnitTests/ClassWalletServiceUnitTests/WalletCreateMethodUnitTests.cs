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

            context.Users.AddRange(user1, user2);
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

        var request = new WalletRequest { UserId = User1Id, Name = walletName, BaseCurrency = baseCurrency };
        var actualWallet = await _walletService.CreateAsync(request);

        var expectedWallet = new WalletDto 
        { 
            Id = actualWallet.Id, 
            Name = "user1 wallet", 
            Balance = 0m, 
            BaseCurrency = nameof(Currencies.UAH), 
            UserId = User1Id,
            Username = "user1"
        };

        actualWallet.Should().BeEquivalentTo(expectedWallet);
    }

    [TestMethod]
    public async Task Test_CreateAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = nameof(Currencies.UAH) };
        var actualWallet = await _walletService.CreateAsync(request);

        var expectedWallet = new WalletDto
        {
            Id = actualWallet.Id,
            Name = "user1 wallet",
            Balance = 0m,
            BaseCurrency = nameof(Currencies.UAH),
            UserId = User1Id,
            Username = "user1"
        };

        actualWallet.Should().BeEquivalentTo(expectedWallet);
    }

    [TestMethod]
    public async Task Test_CreateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var request = new WalletRequest { UserId = User2Id, Name = "user2 wallet", BaseCurrency = nameof(Currencies.UAH) };

        var expectedErrorMessage = "Access denied";

        try
        {
            var wrongWallet = await _walletService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_InvalidBaseCurrencyCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = "AAA" };

        var expectedErrorMessage = $"The specified currency code: {request.BaseCurrency} was not found.";

        try
        {
            var newWallet = await _walletService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_NotUahBaseCurrencyCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new WalletRequest { UserId = User1Id, Name = "user1 wallet", BaseCurrency = nameof(Currencies.USD) };

        var expectedErrorMessage = "Currently, the base currency of the wallet can only be UAH";

        try
        {
            var newWallet = await _walletService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_CreateAsync_UserNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new WalletRequest { UserId = UserNotFoundId, Name = "user1 wallet", BaseCurrency = nameof(Currencies.UAH) };

        var expectedErrorMessage = $"User with id {request.UserId} does not exist.";

        try
        {
            var newWallet = await _walletService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
