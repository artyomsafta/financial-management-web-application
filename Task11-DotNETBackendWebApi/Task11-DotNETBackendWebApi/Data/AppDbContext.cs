using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data.Entities;

namespace Task11_DotNETBackendWebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<FinancialType> FinancialTypes { get; set; } = null!;
    public DbSet<FinancialOperation> FinancialOperations { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FinancialType>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.HasMany(ft => ft.FinancialOperations)
                .WithOne(fo => fo.Type)
                .HasForeignKey(fo => fo.FinancialTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(ft => !ft.IsDeleted);
        });

        modelBuilder.Entity<FinancialOperation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.HasQueryFilter(fo => !fo.IsDeleted);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("IX_USERS_USERNAME");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.HasMany(u => u.Wallets)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.HasMany(w => w.FinancialOperations)
                .WithOne(fo => fo.Wallet)
                .HasForeignKey(fo => fo.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(w => !w.IsDeleted);
        });
    }
}
