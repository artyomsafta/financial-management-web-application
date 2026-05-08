using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests.ClassUserServiceUnitTests;

[TestClass]
public class UserGetMethodsUnitTests
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
            var user3 = new User { Id = User3Id, Username = "DELETED", Role = nameof(UserRoles.User), IsDeleted = true };

            context.Users.AddRange(adminUser, user1, user2, user3);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveAdminCases()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var users = await _userService.GetListAsync();
        Assert.HasCount(3, users.ToList());
        Assert.IsTrue(users.Any(u => u.Username == "admin"));
        Assert.IsTrue(users.Any(u => u.Username == "user1"));
        Assert.IsTrue(users.Any(u => u.Username == "user2"));
        Assert.IsFalse(users.Any(u => u.Username == "DELETED"));
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveUserCases()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var users = await _userService.GetListAsync();
        Assert.HasCount(1, users.ToList());
        Assert.IsTrue(users.Any(u => u.Username == "user1"));
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveAdminCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var successResult = await _userService.GetByIdAsync(User1Id);
        var actualUser = successResult.Data;

        var expectedUser = new UserDto { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User) };

        actualUser.Should().BeEquivalentTo(expectedUser);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveUserCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var successResult = await _userService.GetByIdAsync(User1Id);
        var actualUser = successResult.Data;

        var expectedUser = new UserDto { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User) };

        actualUser.Should().BeEquivalentTo(expectedUser);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_UserNotFoundCase()
    {
        _userContextMock.Setup(c => c.UserId).Returns(UserNotFoundId);

        var failureResult = await _userService.GetByIdAsync(UserNotFoundId);
        var isFindSuccess = failureResult.IsSuccess;
        isFindSuccess.Should().BeFalse();

        var failureMessage = $"User with ID {UserNotFoundId} not found";
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
            var wrongUser = await _userService.GetByIdAsync(User2Id);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
