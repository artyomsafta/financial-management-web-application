using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data.Entities;

namespace Task11_DotNETBackendWebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<FinancialType> FinancialTypes { get; set; }
    public DbSet<FinancialOperation> FinancialOperations { get; set; }

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

            entity.HasMany(ft => ft.FinancialOperations)
                .WithOne(fo => fo.Type)
                .HasForeignKey(fo => fo.FinancialTypeId)
                .OnDelete(DeleteBehavior.Restrict);
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

            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("NOTE");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasColumnName("IS_DELETED");

            entity.Property(e => e.FinancialTypeId)
                .IsRequired()
                .HasColumnName("FINANCIAL_TYPE_ID");

            entity.HasQueryFilter(fo => !fo.IsDeleted);
        });
    }
}
