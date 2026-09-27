using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var configuration = new ConfigurationBuilder().Build();
            var emptyDbSeeder = new DbSeeder(context, configuration);
            emptyDbSeeder.Seed();

            Assert.AreEqual(4, context.FinancialTypes.Count());
            Assert.AreEqual(5, context.Currencies.Count());
            Assert.AreEqual(0, context.Users.Count());
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

            var existingCurrencies = new List<Currency>
            {
                new Currency { Code = "UAH" },
                new Currency { Code = "USD" },
                new Currency { Code = "EUR" }
            };

            var existingUsers = new List<User>
            {
                new User { Username = "Existing User1", Role = UserRoles.User },
                new User { Username = "Existing User2", Role = UserRoles.User }
            };

            context.FinancialTypes.Add(existingFinancialType);
            context.Currencies.AddRange(existingCurrencies);
            context.Users.AddRange(existingUsers);

            context.SaveChanges();

            var configuration = new ConfigurationBuilder().Build();
            var dbWithDataSeeder = new DbSeeder(context, configuration);
            dbWithDataSeeder.Seed();

            Assert.AreEqual(1, context.FinancialTypes.Count());
            Assert.AreEqual(3, context.Currencies.Count());
            Assert.AreEqual(2, context.Users.Count());
        }
    }
}
