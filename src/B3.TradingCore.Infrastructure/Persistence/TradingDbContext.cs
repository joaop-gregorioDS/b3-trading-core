using B3.TradingCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace B3.TradingCore.Infrastructure.Persistence;

/// <summary>
/// DbContext do Entity Framework Core para o sistema de Renda Variável da B3.
/// Configurado para PostgreSQL com precisão monetária e índices de alta performance.
/// </summary>
public class TradingDbContext : DbContext
{
    public DbSet<Trade> Trades => Set<Trade>();

    public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Trade>(builder =>
        {
            builder.ToTable("trades");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.AccountId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(t => t.Ticker)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(t => t.Price)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(t => t.Quantity)
                .IsRequired();

            builder.Property(t => t.MarketType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.Side)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.ExecutedAt)
                .IsRequired();

            builder.Property(t => t.SettlementDate)
                .IsRequired();

            // TotalAmount é computada no domínio (Price * Quantity)
            builder.Ignore(t => t.TotalAmount);

            // Índices para consultas rápidas na mesa de operações
            builder.HasIndex(t => t.Ticker).HasDatabaseName("idx_trades_ticker");
            builder.HasIndex(t => t.AccountId).HasDatabaseName("idx_trades_account");
            builder.HasIndex(t => t.ExecutedAt).HasDatabaseName("idx_trades_executed_at");
        });
    }
}
