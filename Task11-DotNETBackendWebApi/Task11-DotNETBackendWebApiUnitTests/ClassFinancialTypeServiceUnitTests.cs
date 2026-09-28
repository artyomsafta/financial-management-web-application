using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests;

[TestClass]
public class ClassFinancialTypeServiceUnitTests
{
    private AppDbContext _context = null!;
    private Mock<IUserContext> _user = null!;
    private FinancialTypeService _service = null!;
    private Guid _incomeId;
    private Guid _expenseId;

    [TestInitialize]
    public void Setup()
    {
        _context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        _incomeId = Guid.NewGuid(); _expenseId = Guid.NewGuid();
        _context.FinancialTypes.AddRange(
            new FinancialType { Id = _incomeId, Name = "Salary", Description = "Monthly salary", IsIncome = true },
            new FinancialType { Id = _expenseId, Name = "Rent", Description = "Monthly rent", IsIncome = false });
        _context.SaveChanges();
        _user = new Mock<IUserContext>();
        _user.SetupGet(x => x.IsAdmin).Returns(true);
        _service = new FinancialTypeService(_context, _user.Object, Mock.Of<ILogger<FinancialTypeService>>());
    }

    [TestMethod]
    public async Task Test_GetListAsync_PositiveCases()
    {
        var types = await _service.GetListAsync();
        types.Should().BeEquivalentTo(new[] { new FinancialTypeDto { Id = _incomeId, Name = "Salary", Description = "Monthly salary", IsIncome = true }, new FinancialTypeDto { Id = _expenseId, Name = "Rent", Description = "Monthly rent", IsIncome = false } });
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_PositiveCase()
    {
        var type = await _service.GetByIdAsync(_incomeId);
        type.Should().BeEquivalentTo(new FinancialTypeDto { Id = _incomeId, Name = "Salary", Description = "Monthly salary", IsIncome = true });
    }

    [TestMethod]
    public async Task Test_GetByIdAsync_TypeNotFoundCase()
    {
        Func<Task> action = () => _service.GetByIdAsync(Guid.NewGuid());
        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [DataTestMethod]
    [DataRow("Bonus", "Annual bonus", true)]
    [DataRow(" Food ", " Groceries ", false)]
    public async Task Test_CreateAsync_PositiveCase(string name, string description, bool isIncome)
    {
        var id = await _service.CreateAsync(new FinancialTypeRequest { Name = name, Description = description, IsIncome = isIncome });
        var entity = await _context.FinancialTypes.FindAsync(id);
        entity.Should().NotBeNull(); entity!.Name.Should().Be(name.Trim()); entity.Description.Should().Be(description.Trim()); entity.IsIncome.Should().Be(isIncome);
    }

    [DataTestMethod]
    [DataRow("Salary")]
    [DataRow(" salary ")]
    public async Task Test_CreateAsync_NameTakenCase(string name)
    {
        Func<Task> action = () => _service.CreateAsync(new FinancialTypeRequest { Name = name, Description = "x", IsIncome = true });
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("A financial type with the same name already exists.");
    }

    [TestMethod]
    public async Task Test_CreateAsync_UnauthorizedCase()
    {
        _user.SetupGet(x => x.IsAdmin).Returns(false);
        Func<Task> action = () => _service.CreateAsync(new FinancialTypeRequest { Name = "Bonus", Description = "", IsIncome = true });
        (await action.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be("Access denied");
    }

    [DataTestMethod]
    [DataRow("Bonus", "Annual", true)]
    [DataRow(" Food ", " Groceries ", false)]
    public async Task Test_UpdateAsync_PositiveCase(string name, string description, bool isIncome)
    {
        var updated = await _service.UpdateAsync(_incomeId, new FinancialTypeRequest { Name = name, Description = description, IsIncome = isIncome });
        updated.Should().BeTrue();
        var entity = await _context.FinancialTypes.FindAsync(_incomeId);
        entity!.Name.Should().Be(name.Trim()); entity.Description.Should().Be(description.Trim()); entity.IsIncome.Should().Be(isIncome);
    }

    [DataTestMethod]
    [DataRow("Rent")]
    [DataRow(" rent ")]
    public async Task Test_UpdateAsync_NameTakenCase(string name)
    {
        Func<Task> action = () => _service.UpdateAsync(_incomeId, new FinancialTypeRequest { Name = name, Description = "x", IsIncome = true });
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("The financial type with the same name already exists.");
    }

    [TestMethod]
    public async Task Test_UpdateAsync_TypeNotFoundCase()
    {
        Func<Task> action = () => _service.UpdateAsync(Guid.NewGuid(), new FinancialTypeRequest { Name = "Bonus", Description = "", IsIncome = true });
        (await action.Should().ThrowAsync<KeyNotFoundException>()).Which.Message.Should().Be("The financial type does not exist.");
    }

    [TestMethod]
    public async Task Test_UpdateAsync_UnauthorizedCase()
    {
        _user.SetupGet(x => x.IsAdmin).Returns(false);
        Func<Task> action = () => _service.UpdateAsync(_incomeId, new FinancialTypeRequest { Name = "Bonus", Description = "", IsIncome = true });
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveCase()
    {
        (await _service.DeleteAsync(_incomeId)).Should().BeTrue();
        (await _context.FinancialTypes.FindAsync(_incomeId))!.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_PositiveCaseHasNoOperations()
    {
        (await _service.DeleteAsync(_expenseId)).Should().BeTrue();
        (await _context.FinancialTypes.FindAsync(_expenseId))!.IsDeleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_TypeNotFoundCase()
    {
        Func<Task> action = () => _service.DeleteAsync(Guid.NewGuid());
        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [TestMethod]
    public async Task Test_DeleteAsync_TypeHasOperationsCase()
    {
        _context.FinancialOperations.Add(new FinancialOperation { Id = Guid.NewGuid(), FinancialTypeId = _incomeId, WalletId = Guid.NewGuid(), CurrencyId = 1, Amount = 1, Date = DateTime.Today, Note = "active" });
        await _context.SaveChangesAsync();
        Func<Task> action = () => _service.DeleteAsync(_incomeId);
        (await action.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("You cannot delete a type that has financial operations.");
    }

    [TestMethod]
    public async Task Test_DeleteAsync_UnauthorizedCase()
    {
        _user.SetupGet(x => x.IsAdmin).Returns(false);
        Func<Task> action = () => _service.DeleteAsync(_incomeId);
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
