using Shared.Models.DTOs;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Data.Entities;

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

    public static bool IsValidCurrencyCode(this string currencyCode)
    {
        var currency = currencyCode.Trim().ToUpper();

        if (string.IsNullOrEmpty(currency) || currency.Length != 3)
        {
            return false;
        } 
        return true;
    }

    public static FinancialTypeDto MapToFinTypeDto(this FinancialType type)
    {
        return new FinancialTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            Description = type.Description,
            IsIncome = type.IsIncome
        };
    }

    public static UserDto MapToUserDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }
}
