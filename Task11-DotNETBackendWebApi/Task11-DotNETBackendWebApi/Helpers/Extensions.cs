using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;
using Task11_DotNETBackendWebApi.Helpers.Enums;

namespace Task11_DotNETBackendWebApi.Helpers;

public static class Extensions
{
    public static IHost InitDatabase(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var initializer = services.GetRequiredService<DbInitializer>();
                initializer.Initialize();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
            }
        }

        return host;
    }

    public static async Task EnsureTypeExistsAsync(this IQueryable<FinancialType> query, Guid id)
    {
        var typeExists = await query.AnyAsync(t => t.Id == id);

        if (!typeExists)
        {
            throw new InvalidOperationException("The specified type of operation does not exist.");
        }
    }

    public static async Task EnsureWalletExistsAsync(this IQueryable<Wallet> query, Guid Id)
    {
        var walletExists = await query.AnyAsync(w => w.Id == Id);

        if (!walletExists)
        {
            throw new InvalidOperationException("The specified wallet does not exist.");
        }
    }

    public static async Task EnsureUserExistsAsync(this IQueryable<User> query, Guid Id)
    {
        var userExists = await query.AnyAsync(u => u.Id == Id);

        if (!userExists)
        {
            throw new InvalidOperationException($"User with id {Id} does not exist.");
        }
    }

    public static async Task EnsureUsernameNotTakenAync(this IQueryable<User> query, string username)
    {
        if (await query.AnyAsync(u => u.Username.ToLower() == username.Trim().ToLower()))
        {
            throw new InvalidOperationException("A user with the same name already exists.");
        }
    }

    public static async Task EnsureTypeNameNotTakenAync(this IQueryable<FinancialType> query, string typeName)
    {
        if (await query.AnyAsync(t => t.Name.ToLower() == typeName.Trim().ToLower()))
        {
            throw new InvalidOperationException("A financial type with the same name already exists.");
        }
    }

    public static void EnsureCurrencyIsValid(this string currencyCode)
    {
        if (!Enum.TryParse<Currencies>(currencyCode.Trim().ToUpper(), out _))
        {
            throw new InvalidOperationException($"The specified currency code: {currencyCode} was not found.");
        }
    }

    public static void EnsureDateIsAcceptable(this DateTime date)
    {
        if (date > DateTime.Now)
        {
            throw new InvalidOperationException("The specified date cannot be in the future.");
        }
    }
}
