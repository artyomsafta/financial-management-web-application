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

namespace Task11_DotNETBackendWebApiUnitTests;

[TestClass]
public class GroupCreateMethodUnitTests
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
            var user1 = new User { Id = User1Id, Username = "user1", Role = nameof(UserRoles.User), IsDeleted = false };

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
        var request = new UserRegisterRequest { Username = username, Password = "12345" };
        var newUser = await _userService.CreateAsync(request);

        var expectedUser = new UserDto { Id = newUser.Id, Username = "New user", Role = nameof(UserRoles.User) };

        var userEntity = await _context.Users.FindAsync(newUser.Id);
        var actualUser = new UserDto
        {
            Id = userEntity.Id,
            Username = userEntity.Username,
            Role = userEntity.Role
        };

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
        var request = new UserRegisterRequest { Username = username, Password = "12345" };

        var expectedErrorMessage = "A user with the same name already exists.";

        try
        {
            var newUser = await _userService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (InvalidOperationException actualError) 
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
