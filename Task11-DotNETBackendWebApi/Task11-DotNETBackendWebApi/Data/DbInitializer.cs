using Microsoft.EntityFrameworkCore;

namespace Task11_DotNETBackendWebApi.Data;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly DbSeeder _dbSeeder;
    public DbInitializer(AppDbContext context, DbSeeder dbSeeder)
    {
        _context = context;
        _dbSeeder = dbSeeder;
    }
    public void Initialize()
    {
        _context.Database.Migrate();
        _dbSeeder.Seed();
    }
}
