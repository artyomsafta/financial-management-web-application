using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassUserServiceUnitTests;

[TestClass]
public class UserDeleteMethodUnitTests
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

    private static readonly int CurrencyUahId = 1;

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
            var adminUser = new User { Id = AdminUserId, Username = "admin", Role = UserRoles.Admin, IsDeleted = false };
            var user1 = new User { Id = User1Id, Username = "user1", Role = UserRoles.User, IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = UserRoles.User, IsDeleted = false };
            var user3 = new User { Id = User3Id, Username = "user3", Role = UserRoles.User, IsDeleted = false };

            var wallet1 = new Wallet { Id = Wallet1Id, Name = "user1 wallet", IsDeleted = true, UserId = User1Id, CurrencyId = CurrencyUahId };
            var wallet2 = new Wallet { Id = Wallet2Id, Name = "user2 wallet", IsDeleted = false, UserId = User2Id, CurrencyId = CurrencyUahId };

            context.Users.AddRange(adminUser, user1, user2, user3);
            context.Wallets.AddRange(wallet1, wallet2);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var userId = User1Id;

        var successResult = await _userService.DeleteAsync(userId);
        var isDeleteSuccess = successResult;
        isDeleteSuccess.Should().BeTrue();

        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);
        var userId = User1Id;

        var successResult = await _userService.DeleteAsync(userId);
        var isDeleteSuccess = successResult;
        isDeleteSuccess.Should().BeTrue();

        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveCaseHasNoWallets()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var userId = User3Id;

        var successResult = await _userService.DeleteAsync(userId);
        var isDeleteSuccess = successResult;
        isDeleteSuccess.Should().BeTrue();

        var deletedUser = await _context.Users.FindAsync(userId);
        deletedUser.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_UserNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        var userId = UserNotFoundId;

        Func<Task> action = () => _userService.DeleteAsync(userId);

        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message
            .Should().Be($"User with ID {userId} not found");
    }

    [TestMethod]
    public async Task Test_DeleteAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var userId = User1Id;

        Func<Task> action = () => _userService.DeleteAsync(userId);

        (await action.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("Access denied");
    }

    [TestMethod]
    public async Task Test_DeleteAsync_UserHasWalletsCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);
        var userId = User2Id;

        Func<Task> action = () => _userService.DeleteAsync(userId);

        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be("You cannot delete a user that has active wallets.");
    }
}

