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

namespace Task11_DotNETBackendWebApiUnitTests;

[TestClass]
public class ClassFinancialTypeServiceUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ILogger<FinancialTypeService>> _loggerMock;
    private FinancialTypeService _financialTypeService;

    private static readonly Guid Type1Id = Guid.NewGuid();
    private static readonly Guid Type2Id = Guid.NewGuid();
    private static readonly Guid Type3Id = Guid.NewGuid();
    private static readonly Guid Type4Id = Guid.NewGuid();

    private static readonly Guid TypeNotFoundId = Guid.NewGuid();

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(_options);
        _userContextMock = new Mock<IUserContext>();
        _loggerMock = new Mock<ILogger<FinancialTypeService>>();
        _financialTypeService = new FinancialTypeService(_context, _userContextMock.Object, _loggerMock.Object);

        this.SeedMockDb();
    }

    private void SeedMockDb()
    {
        using (var context = new AppDbContext(_options))
        {
            var type1 = new FinancialType { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true, IsDeleted = false };
            var type2 = new FinancialType { Id = Type2Id, Name = "Groceries", Description = "Food and household items", IsIncome = false, IsDeleted = false };
            var type3 = new FinancialType { Id = Type3Id, Name = "Entertainment", Description = "Movies, games, etc.", IsIncome = false, IsDeleted = false };
            var type4 = new FinancialType { Id = Type4Id, Name = "DELETED", Description = "Soft-deleted type", IsIncome = false, IsDeleted = true };

            var type1Operation = new FinancialOperation
            {
                Id = Guid.NewGuid(),
                Amount = 10_000m,
                Date = DateTime.UtcNow,
                Note = "existing operation. You can't delete my type!",
                IsDeleted = false,
                FinancialTypeId = Type1Id,
                WalletId = Guid.NewGuid(),
                CurrencyId = 1
            };

            var type2Operation = new FinancialOperation
            {
                Id = Guid.NewGuid(),
                Amount = 250,
                Date = DateTime.UtcNow,
                Note = "deleted operation for positive tests",
                IsDeleted = true,
                FinancialTypeId = Type2Id,
                WalletId = Guid.NewGuid(),
                CurrencyId = 1
            };

            context.FinancialTypes.AddRange(type1, type2, type3, type4);
            context.FinancialOperations.AddRange(type1Operation, type2Operation);
            context.SaveChanges();
        }
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveCases()
    {
        var types = await _financialTypeService.GetListAsync();
        Assert.HasCount(3, types.ToList());
        Assert.IsTrue(types.Any(t => t.Name == "Salary"));
        Assert.IsTrue(types.Any(t => t.Name == "Groceries"));
        Assert.IsTrue(types.Any(t => t.Name == "Entertainment"));
        Assert.IsFalse(types.Any(t => t.Name == "DELETED"));
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveCase()
    {
        var expectedType = new FinancialTypeDto { Id = Type1Id, Name = "Salary", Description = "Monthly salary", IsIncome = true };
        var successResult = await _financialTypeService.GetByIdAsync(Type1Id);
        var actualType = successResult.Data;

        actualType.Should().BeEquivalentTo(expectedType);
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_TypeNotFoundCase()
    {
        var failureResult = await _financialTypeService.GetByIdAsync(TypeNotFoundId);
        var isFindSuccess = failureResult.IsSuccess;
        isFindSuccess.Should().BeFalse();

        var failureMessage = $"Type with ID {TypeNotFoundId} not found";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [DataTestMethod]
    [DataRow("Name")]
    [DataRow("Name   ")]
    [DataRow("   Name")]
    [DataRow("   Name   ")]
    public async Task Test_CreateAsync_PositiveCase(string typeName)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialTypeRequest 
        { 
            Name = typeName, 
            Description = "New type", 
            IsIncome = true 
        };

        var successResult = await _financialTypeService.CreateAsync(request);
        var actualType = successResult.Data;

        var expectedType = new FinancialTypeDto 
        { 
            Id = actualType.Id, 
            Name = "Name", 
            Description = "New type", 
            IsIncome = true 
        };

        actualType.Should().BeEquivalentTo(expectedType);
    }

    [DataTestMethod]
    [DataRow("Salary")]
    [DataRow("Salary   ")]
    [DataRow("   Salary")]
    [DataRow("   Salary   ")]
    [DataRow("SALARY")]
    [DataRow("SALARY   ")]
    [DataRow("   SALARY")]
    [DataRow("   SALARY   ")]
    [DataRow("SaLAry")]
    [DataRow("   SaLAry   ")]
    public async Task Test_CreateAsync_NameTakenCase(string typeName)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var request = new FinancialTypeRequest 
        { 
            Name = typeName, 
            Description = "Monthly salary", 
            IsIncome = true 
        };

        var failureMessage = "A financial type with the same name already exists.";

        var failureResult = await _financialTypeService.CreateAsync(request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_CreateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);

        var request = new FinancialTypeRequest
        {
            Name = "Name",
            Description = "New type",
            IsIncome = true
        };

        var expectedErrorMessage = "Access denied";

        try
        {
            var newType = await _financialTypeService.CreateAsync(request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [DataTestMethod]
    [DataRow("My salary", "My monthly salary")]
    [DataRow("My salary   ", "   My monthly salary")]
    [DataRow("   My salary", "My monthly salary   ")]
    [DataRow("   My salary   ", "   My monthly salary   ")]
    public async Task Test_UpdateAsync_PositiveCase(string typeName, string description)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var typeId = Type1Id;
        var request = new FinancialTypeRequest 
        { 
            Name = typeName, 
            Description = description, 
            IsIncome = true 
        };

        var successResult = await _financialTypeService.UpdateAsync(typeId, request);
        var isUpdateSuccess = successResult.IsSuccess;
        isUpdateSuccess.Should().BeTrue();

        var expectedType = new FinancialTypeDto 
        { 
            Id = typeId, 
            Name = "My salary", 
            Description = "My monthly salary", 
            IsIncome = true 
        };

        var typeEntity = await _context.FinancialTypes.FindAsync(typeId);

        var actualType = new FinancialTypeDto
        {
            Id = typeEntity.Id,
            Name = typeEntity.Name,
            Description = typeEntity.Description,
            IsIncome = typeEntity.IsIncome
        };

        actualType.Should().BeEquivalentTo(expectedType);
    }

    [DataTestMethod]
    [DataRow("Salary")]
    [DataRow("Salary   ")]
    [DataRow("   Salary")]
    [DataRow("   Salary   ")]
    [DataRow("SALARY")]
    [DataRow("SALARY   ")]
    [DataRow("   SALARY")]
    [DataRow("   SALARY   ")]
    [DataRow("SaLAry")]
    [DataRow("   SaLAry   ")]
    public async Task Test_UpdateAsync_NameTakenCase(string typeName)
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var typeId = Type2Id;

        var request = new FinancialTypeRequest
        {
            Name = typeName,
            Description = "Monthly salary",
            IsIncome = true
        };

        var failureMessage = "The financial type with the same name already exists.";

        var failureResult = await _financialTypeService.UpdateAsync(typeId, request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_TypeNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var typeId = TypeNotFoundId;

        var request = new FinancialTypeRequest 
        { 
            Name = "My salary", 
            Description = "My monthly salary", 
            IsIncome = true 
        };

        var failureMessage = "The financial type does not exist.";

        var failureResult = await _financialTypeService.UpdateAsync(typeId, request);

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);

        var typeId = Type1Id;

        var request = new FinancialTypeRequest 
        { 
            Name = "My salary", 
            Description = "My monthly salary", 
            IsIncome = true 
        };

        var expectedErrorMessage = "Access denied";

        try
        {
            var isUpdateSuccess = await _financialTypeService.UpdateAsync(typeId, request);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var typeId = Type2Id;

        var successResult = await _financialTypeService.DeleteAsync(typeId);
        var isDeleteSuccess = successResult.IsSuccess;
        isDeleteSuccess.Should().BeTrue();

        var deletedType = await _context.FinancialTypes.FindAsync(typeId);
        deletedType.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveCaseHasNoOperations()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var typeId = Type3Id;

        var successResult = await _financialTypeService.DeleteAsync(typeId);
        var isDeleteSuccess = successResult.IsSuccess;
        isDeleteSuccess.Should().BeTrue();

        var deletedType = await _context.FinancialTypes.FindAsync(typeId);
        deletedType.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_TypeNotFoundCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var typeId = TypeNotFoundId;

        var failureResult = await _financialTypeService.DeleteAsync(typeId);
        var isDeleteSuccess = failureResult.IsSuccess;
        isDeleteSuccess.Should().BeFalse();

        var failureMessage = "The financial type does not exist.";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_DeleteAsync_TypeHasOperationsCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(true);

        var typeId = Type1Id;

        var failureResult = await _financialTypeService.DeleteAsync(typeId);
        var isDeleteSuccess = failureResult.IsSuccess;
        isDeleteSuccess.Should().BeFalse();

        var failureMessage = "You cannot delete a type that has financial operations.";
        failureResult.Errors.Should().Contain(failureMessage);
    }

    [TestMethod]
    public async Task Test_DeleteAsync_UnauthorizedCase()
    {
        _userContextMock.Setup(с => с.IsAdmin).Returns(false);

        var typeId = Type2Id;

        var expectedErrorMessage = "Access denied";

        try
        {
            var isDeleteSuccess = await _financialTypeService.DeleteAsync(typeId);
            Assert.Fail("Expected Exception was not thrown.");
        }
        catch (UnauthorizedAccessException actualError)
        {
            Assert.AreEqual(expectedErrorMessage, actualError.Message);
        }
    }
}
