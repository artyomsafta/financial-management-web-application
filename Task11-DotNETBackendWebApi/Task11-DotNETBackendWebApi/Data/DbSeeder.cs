using Microsoft.AspNetCore.Identity;
using Shared.Models;
using Task11_DotNETBackendWebApi.Data.Entities;

namespace Task11_DotNETBackendWebApi.Data;

public class DbSeeder
{
    private readonly AppDbContext _context;

    public DbSeeder(AppDbContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        if (!_context.FinancialTypes.Any())
        {
            var financialTypes = new List<FinancialType>
            {
                new FinancialType { Name = "Salary", Description = "Monthly salary", IsIncome = true },
                new FinancialType { Name = "Groceries", Description = "Food and household items", IsIncome = false },
                new FinancialType { Name = "Rent", Description = "Monthly rent payment", IsIncome = false },
                new FinancialType { Name = "Investment", Description = "Returns from investments", IsIncome = true }
            };

            _context.FinancialTypes.AddRange(financialTypes);
            _context.SaveChanges();
        }

        if (!_context.Currencies.Any())
        {
            var currencies = new List<Currency>
            {
                new Currency { Code = "UAH" },
                new Currency { Code = "USD" },
                new Currency { Code = "EUR" },
                new Currency { Code = "GBP" },
                new Currency { Code = "CHF" }
            };
            _context.Currencies.AddRange(currencies);
            _context.SaveChanges();
        }

        if (!_context.Users.Any())
        {
            var hasher = new PasswordHasher<User>();
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "__REMOVED_BOOTSTRAP_ADMIN_USERNAME__",
                Role = UserRoles.Admin
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "__REMOVED_BOOTSTRAP_ADMIN_PASSWORD__");

            _context.Users.Add(adminUser);
            _context.SaveChanges();
        }
    }
}
