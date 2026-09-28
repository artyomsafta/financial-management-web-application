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

namespace Task11_DotNETBackendWebApiUnitTests.ClassUserServiceUnitTests;

[TestClass]
public class UserUpdateMethodUnitTests
{
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
            var adminUser = new User { Id = AdminUserId, Username = "admin", Role = UserRoles.Admin, IsDeleted = false };
            var user1 = new User { Id = User1Id, Username = "user1", Role = UserRoles.User, IsDeleted = false };
            var user2 = new User { Id = User2Id, Username = "user2", Role = UserRoles.User, IsDeleted = false };

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
        var request = new UserRegisterRequest { Username = username, Password = "A#345678" };

        var successResult = await _userService.UpdateAsync(userId, request);
        var isUpdateSuccess = successResult;
        isUpdateSuccess.Should().BeTrue();

        var expectedUser = new UserDto { Id = userId, Username = "Updated user1", Role = UserRoles.User };

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
        var request = new UserRegisterRequest { Username = username, Password = "A#345678" };

        var successResult = await _userService.UpdateAsync(userId, request);
        var isUpdateSuccess = successResult;
        isUpdateSuccess.Should().BeTrue();

        var expectedUser = new UserDto { Id = userId, Username = "Updated user1", Role = UserRoles.User };

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
        var request = new UserRegisterRequest { Username = "Updated user1", Password = "A#345678" };

        Func<Task> action = () => _userService.UpdateAsync(userId, request);

        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message
            .Should().Be($"User with ID {userId} not found");
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User2Id);

        var userId = User1Id;
        var request = new UserRegisterRequest { Username = "Updated user1", Password = "A#345678" };

        Func<Task> action = () => _userService.UpdateAsync(userId, request);

        (await action.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("Access denied");
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

        var userId = User2Id;
        var request = new UserRegisterRequest { Username = username, Password = "A#345678" };

        Func<Task> action = () => _userService.UpdateAsync(userId, request);

        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message
            .Should().Be("A user with the same username already exists.");
    }

    [DataTestMethod]
    [DataRow("", "Password cannot be empty or consist only of spaces.")]
    [DataRow("    ", "Password cannot be empty or consist only of spaces.")]
    [DataRow("1A#", "Password must be between 8 and 64 characters long.")]
    [DataRow("1234567890abcdefghijklmnopqrstyvwxyzABCDEFGHIJKLMNOPQRSTYVWXYZ@#$", "Password must be between 8 and 64 characters long.")]
    [DataRow("1234567#", "Password must contain at least one uppercase letter.")]
    [DataRow("abcdefG#", "Password must contain at least one digit.")]
    [DataRow("1234567A", "Password must contain at least one special character.")]
    public async Task Test_UpdateAsync_InvalidPasswordComplexityCase(string weakPassword, string expectedErrorMessage)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);
        _userContextMock.Setup(c => c.UserId).Returns(User1Id);

        var userId = User1Id;
        var request = new UserRegisterRequest { Username = "Updated user1", Password = weakPassword };

        Func<Task> action = () => _userService.UpdateAsync(userId, request);

        (await action.Should().ThrowAsync<ValidationException>()).Which.Message
            .Should().Be(expectedErrorMessage);
    }
}

