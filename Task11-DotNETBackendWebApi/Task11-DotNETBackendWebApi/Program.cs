using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;

namespace Task11_DotNETBackendWebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        builder.Services.AddControllers();
        builder.Services.AddOpenApi(); //replace it with Swagger (as required by the task)

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            //app.UseSwaggerUI() use here to work with UI Swagger!
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
