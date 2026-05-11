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

namespace Task11_DotNETBackendWebApiUnitTests.ClassUserServiceUnitTests;

[TestClass]
public class UserCreateMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<UserService>> _loggerMock;
    private UserService _userService;

    private static readonly Guid User1Id = Guid.NewGuid();

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
            var user1 = new User { Id = User1Id, Username = "user1", Role = UserRoles.User, IsDeleted = false };

            context.Users.Add(user1);
            context.SaveChanges();
        }
    }

    [DataTestMethod]
    [DataRow("New user")]
    [DataRow("New user   ")]
    [DataRow("   New user")]
    [DataRow("   New user   ")]
    public async Task Test_CreateAsync_PositiveCases(string username)
    {
        var request = new UserRegisterRequest { Username = username, Password = "A#345678" };

        var successResult = await _userService.CreateAsync(request);
        var actualUser = successResult.Data;

        var expectedUser = new UserDto { Id = actualUser.Id, Username = "New user", Role = UserRoles.User };

        actualUser.Should().BeEquivalentTo(expectedUser);
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
    public async Task Test_CreateAsync_NameTakenCase(string username)
    {
        var request = new UserRegisterRequest { Username = username, Password = "A#345678" };

        var failureResult = await _userService.CreateAsync(request);
        var failureMessage = "A user with the same username already exists.";

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [DataTestMethod]
    [DataRow("", "Password cannot be empty or consist only of spaces.")]
    [DataRow("    ", "Password cannot be empty or consist only of spaces.")]
    [DataRow("1A#", "Password must be between 8 and 64 characters long.")]
    [DataRow("1234567890abcdefghijklmnopqrstyvwxyzABCDEFGHIJKLMNOPQRSTYVWXYZ@#$", "Password must be between 8 and 64 characters long.")]
    [DataRow("1234567#", "Password must contain at least one uppercase letter.")]
    [DataRow("abcdefG#", "Password must contain at least one digit.")]
    [DataRow("1234567A", "Password must contain at least one special character.")]
    public async Task Test_CreateAsync_InvalidPasswordComplexityCase(string weakPassword, string expectedErrorMessage)
    {
        var request = new UserRegisterRequest { Username = "New user", Password = weakPassword };

        var failureResult = await _userService.CreateAsync(request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(expectedErrorMessage);
    }
}
