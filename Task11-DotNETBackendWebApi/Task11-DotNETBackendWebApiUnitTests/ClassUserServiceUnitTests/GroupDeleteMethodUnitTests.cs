using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers.Enums;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests;

[TestClass]
public class GroupDeleteMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<UserService>> _loggerMock;
    private UserService _userService;

    private static readonly Guid AdminUserId = Guid.NewGuid();
    private static readonly Guid User1Id = Guid.NewGuid();
    private static readonly Guid User2Id = Guid.NewGuid();
    private static readonly Guid User3Id = Guid.NewGuid();

    private static readonly Guid UserNotFoundId = Guid.NewGuid();

    private static readonly Guid Wallet1Id = Guid.NewGuid();
    private static readonly Guid Wallet2Id = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(_options);
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_context, _userContextMock.Object, _loggerMock.Object);

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

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", IsDeleted = true, UserId = User1Id };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "user2 wallet", IsDeleted = false, UserId = User2Id };

            context.Users.AddRange(adminUser, user1, user2, user3);
            context.Wallets.AddRange(wallet1, wallet2);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var userId = User1Id;

        var isDeletSuccess = await _userService.SoftDeleteAsync(userId);
        isDeletSuccess.Should().BeTrue();

        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var userId = User1Id;

        var isDeletSuccess = await _userService.SoftDeleteAsync(userId);
        isDeletSuccess.Should().BeTrue();

        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_PositiveCaseHasNoWallets()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var userId = User3Id;

        var isDeletSuccess = await _userService.SoftDeleteAsync(userId);
        isDeletSuccess.Should().BeTrue();

        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_UserNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var userId = UserNotFoundId;

        var isDeletSuccess = await _userService.SoftDeleteAsync(userId);
        isDeletSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var userId = User1Id;

        var expectedErrorMessage = "Access denied";

        try
        {
            var isDeletSuccess = await _userService.SoftDeleteAsync(userId);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_SoftDeleteAsync_UserHasWalletsCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var userId = User2Id;

        var expectedErrorMessage = "You cannot delete a user that has active wallets.";

        try
        {
            var isDeletSuccess = await _userService.SoftDeleteAsync(userId);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
