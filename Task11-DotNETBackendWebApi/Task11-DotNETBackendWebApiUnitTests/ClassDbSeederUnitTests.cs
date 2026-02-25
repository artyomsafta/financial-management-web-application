using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;

namespace Task11_DotNETBackendWebApiUnitTests;

[TestClass]
public class ClassDbSeederUnitTests
{
    [TestMethod]
    public void Test_Seed_EmptyDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "MockDb01Empty")
            .Options;

        using (var context = new AppDbContext(options))
        {
            var emptyDbSeeder = new DbSeeder(context);
            emptyDbSeeder.Seed();

            Assert.AreEqual(4, context.FinancialTypes.Count());
            Assert.AreEqual(1, context.Users.Count());
        }
    }

    [TestMethod]
    public void Test_Seed_DbWithData()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "MockDb01HasData")
            .Options;

        using (var context = new AppDbContext(options))
        {
            var existingFinancialType = new FinancialType { Name = "Existing Type", IsIncome = true };
            var existingUsers = new List<User>
            {
                new User { Username = "Existing User1", Role = "User" },
                new User { Username = "Existing User2", Role = "User" }
            };

            context.FinancialTypes.Add(existingFinancialType);
            context.Users.AddRange(existingUsers);

            context.SaveChanges();

            var dbWithDataSeeder = new DbSeeder(context);
            dbWithDataSeeder.Seed();

            Assert.AreEqual(1, context.FinancialTypes.Count());
            Assert.AreEqual(2, context.Users.Count());
        }
    }
}
