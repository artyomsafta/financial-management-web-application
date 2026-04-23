using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers.Enums;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassWalletServiceUnitTests;

[TestClass]
public class WalletGetMethodsUnitTests
{/*
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<WalletService>> _loggerMock;
    private WalletService _walletService;

    private static readonly Guid AdminUserId = Guid.NewGuid();
    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();
    private static readonly Guid User3Id = Guid.NewGuid();
    private static readonly Guid User4Id = Guid.NewGuid();

    private static readonly Guid AdminWalletId = Guid.NewGuid();
    private static readonly Guid Wallet1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Id = Guid.NewGuid();
    private static readonly Guid Wallet3Id = Guid.NewGuid();
    private static readonly Guid Wallet4Id = Guid.NewGuid();

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
            var adminUser = new User { Id = AdminUserId, Username = "__REMOVED_BOOTSTRAP_ADMIN_USERNAME__", Role = nameof(UserRoles.Admin), IsDeleted = false };
            var user1 = new User { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User), IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = nameof(UserRoles.User), IsDeleted = false };
            var user3 = new User { Id = User3Id, Username = "user3", Role = nameof(UserRoles.User), IsDeleted = false };
            var user4 = new User { Id = User4Id, Username = "user4", Role = nameof(UserRoles.User), IsDeleted = false };

            var adminWallet = new Wallet { Id = AdminWalletId, Name = "admin wallet", IsDeleted = false, UserId = AdminUserId };
            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", IsDeleted = false, UserId = User1Id };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "user2 wallet", IsDeleted = false, UserId = User2Id };
            var wallet3 = new Wallet { Id = Wallet3Id, Name = "user3 wallet", IsDeleted = false, UserId = User3Id };
            var wallet4 = new Wallet { Id = Wallet4Id, Name = "DELETED wallet", IsDeleted = true, UserId = User4Id };

            context.Users.AddRange(adminUser, user1, user2, user3, user4);
            context.Wallets.AddRange(adminWallet, wallet1, wallet2, wallet3, wallet4);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveAdminCases()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var wallets = await _walletService.GetListAsync();
        Assert.HasCount(4, wallets.ToList());
        Assert.IsTrue(wallets.Any(w => w.Name == "admin wallet"));
        Assert.IsTrue(wallets.Any(w => w.Name == "user1 wallet"));
        Assert.IsTrue(wallets.Any(w => w.Name == "user2 wallet"));
        Assert.IsTrue(wallets.Any(w => w.Name == "user3 wallet"));
        Assert.IsFalse(wallets.Any(w => w.Name == "DELETED wallet"));
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveUserCases()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var wallets = await _walletService.GetListAsync();
        Assert.HasCount(1, wallets.ToList());
        Assert.IsTrue(wallets.Any(w => w.Name == "user1 wallet"));
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var expectedWallet = new WalletDto 
        { 
            Id = Wallet1Id, 
            Name = "user1 wallet", 
            BaseCurrency = nameof(Currencies.UAH), 
            UserId = User1Id, 
            Username = "user1" 
        };
        var actualWallet = await _walletService.GetByIdAsync(Wallet1Id);

        actualWallet.Should().BeEquivalentTo(expectedWallet);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var expectedWallet = new WalletDto
        {
            Id = Wallet1Id,
            Name = "user1 wallet",
            BaseCurrency = nameof(Currencies.UAH),
            UserId = User1Id,
            Username = "user1"
        };
        var actualWallet = await _walletService.GetByIdAsync(Wallet1Id);

        actualWallet.Should().BeEquivalentTo(expectedWallet);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_WalletNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var nullWallet = await _walletService.GetByIdAsync(WalletNotFoundId);

        nullWallet.Should().BeNull();
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var expectedErrorMessage = "Access denied";

        try
        {
            var wrongWallet = await _walletService.GetByIdAsync(Wallet2Id);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }*/
}
