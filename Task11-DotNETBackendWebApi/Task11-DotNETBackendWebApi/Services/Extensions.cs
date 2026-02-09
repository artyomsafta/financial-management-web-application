using Task11_DotNETBackendWebApi.Data;

namespace Task11_DotNETBackendWebApi.Services;

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
}
