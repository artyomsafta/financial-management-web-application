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

namespace Task11_DotNETBackendWebApiUnitTests.ClassUserServiceUnitTests;

[TestClass]
public class UserUpdateMethodUnitTests
{/*
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<UserService>> _loggerMock;
    private UserService _userService;

    private static readonly Guid AdminUserId = Guid.NewGuid();
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

            context.Users.AddRange(adminUser, user1, user2);
            context.SaveChanges();
        }
    }

    [DataTestMethod]
    [DataRow("Updated user1")]
    [DataRow("   Updated user1")]
    [DataRow("Updated user1   ")]
    [DataRow("   Updated user1   ")]
    public async Task Test_UpdateAsync_PositiveAdminCase(string username)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var userId = User1Id;
        var request = new UserRegisterRequest { Username = username, Password = "12345" };

        var isUpdateSuccess = await _userService.UpdateAsync(userId, request);
        isUpdateSuccess.Should().BeTrue();

        var expectedUser = new UserDto { Id = userId, Username = "Updated user1", Role = nameof(UserRoles.User) };
        var userEntity = await _context.Users.FindAsync(userId);
        var actualUser = new UserDto
        {
            Id = userEntity.Id,
            Username = userEntity.Username,
            Role = userEntity.Role
        };

        actualUser.Should().BeEquivalentTo(expectedUser);
    }

    [DataTestMethod]
    [DataRow("Updated user1")]
    [DataRow("   Updated user1")]
    [DataRow("Updated user1   ")]
    [DataRow("   Updated user1   ")]
    public async Task Test_UpdateAsync_PositiveUserCase(string username)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var userId = User1Id;
        var request = new UserRegisterRequest { Username = username, Password = "12345" };

        var isUpdateSuccess = await _userService.UpdateAsync(userId, request);
        isUpdateSuccess.Should().BeTrue();

        var expectedUser = new UserDto { Id = userId, Username = "Updated user1", Role = nameof(UserRoles.User) };
        var userEntity = await _context.Users.FindAsync(userId);
        var actualUser = new UserDto
        {
            Id = userEntity.Id,
            Username = userEntity.Username,
            Role = userEntity.Role
        };

        actualUser.Should().BeEquivalentTo(expectedUser);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UserNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);
        
        var userId = UserNotFoundId;
        var request = new UserRegisterRequest { Username = "Updated user1", Password = "12345" };

        var isUpdateSuccess = await _userService.UpdateAsync(userId, request);
        isUpdateSuccess.Should().BeFalse();
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var userId = User1Id;
        var request = new UserRegisterRequest { Username = "Updated user1", Password = "12345" };

        var expectedErrorMessage = "Access denied";

        try
        {
            var isUpdateSuccess = await _userService.UpdateAsync(userId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [DataTestMethod]
    [DataRow("user1")]
    [DataRow("user1   ")]
    [DataRow("   user1")]
    [DataRow("   user1   ")]
    [DataRow("USER1")]
    [DataRow("USER1   ")]
    [DataRow("   USER1")]
    [DataRow("   USER1   ")]
    [DataRow("UsEr1")]
    public async Task Test_UpdateAsync_NameTakenCase(string username)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var request = new UserRegisterRequest { Username = username, Password = "12345" };
        var userId = User2Id;

        var expectedErrorMessage = "A user with the same name already exists.";

        try
        {
            var isUpdateSuccess = await _userService.UpdateAsync(userId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }*/
}
