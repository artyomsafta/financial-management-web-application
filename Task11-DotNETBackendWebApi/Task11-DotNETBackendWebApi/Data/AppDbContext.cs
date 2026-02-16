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
            entity.ToTable("FINANCIAL_TYPES");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("NAME");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.IsIncome)
                .IsRequired()
                .HasColumnName("IS_INCOME");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IS_DELETED");

            entity.HasMany(ft => ft.FinancialOperations)
                .WithOne(fo => fo.Type)
                .HasForeignKey(fo => fo.FinancialTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(ft => !ft.IsDeleted);
        });

        modelBuilder.Entity<FinancialOperation>(entity =>
        {
            entity.ToTable("FINANCIAL_OPERATIONS");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");

            entity.Property(e => e.Amount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnName("AMOUNT");

            entity.Property(e => e.Date)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasColumnName("DATE");

            entity.Property(e => e.CurrentCurrency)
                .IsRequired()
                .HasColumnName("CURRENT_CURRENCY");

            entity.Property(e => e.TransactionComment)
                .HasMaxLength(255)
                .HasColumnName("TRANSACTION_COMMENT");

            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("NOTE");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IS_DELETED");

            entity.Property(e => e.FinancialTypeId)
                .IsRequired()
                .HasColumnName("FINANCIAL_TYPE_ID");

            entity.Property(e => e.WalletId)
                .IsRequired()
                .HasColumnName("WALLET_ID");

            entity.HasQueryFilter(fo => !fo.IsDeleted);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("IX_USERS_USERNAME");

            entity.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");

            entity.Property(e => e.Username)
                .IsRequired()
                .HasColumnName("USERNAME");

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasColumnName("PASSWORD_HASH");

            entity.Property(e => e.Role)
                .IsRequired()
                .HasColumnName("ROLE");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IS_DELETED");

            entity.HasMany(u => u.Wallets)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("WALLETS");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("NAME");

            entity.Property(e => e.Balance)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnName("BALANCE");

            entity.Property(e => e.BaseCurrency)
                .IsRequired()
                .HasColumnName("BASE_CURRENCY");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("IS_DELETED");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasColumnName("USER_ID");

            entity.HasMany(w => w.FinancialOperations)
                .WithOne(fo => fo.Wallet)
                .HasForeignKey(fo => fo.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(w => !w.IsDeleted);
        });
    }
}
