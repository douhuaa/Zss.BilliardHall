using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Zss.BilliardHall.Entities;

namespace Zss.BilliardHall.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class BilliardHallDbContext : AbpDbContext<BilliardHallDbContext>
{
    public DbSet<BilliardTable> BilliardTables { get; set; }
    public DbSet<TableSession> TableSessions { get; set; }

    public BilliardHallDbContext(DbContextOptions<BilliardHallDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 配置台球桌实体
        builder.Entity<BilliardTable>(b =>
        {
            b.ToTable("billiard_tables");
            
            b.Property(x => x.TableNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.TableName).IsRequired().HasMaxLength(100);
            b.Property(x => x.Location).HasMaxLength(200);
            b.Property(x => x.DeviceId).HasMaxLength(100);
            b.Property(x => x.QrCodeContent).HasMaxLength(500);
            b.Property(x => x.Remarks).HasMaxLength(500);
            b.Property(x => x.BasePrice).HasColumnType("decimal(10,2)");

            b.HasIndex(x => new { x.StoreId, x.TableNumber }).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.StoreId);
        });

        // 配置台球会话实体
        builder.Entity<TableSession>(b =>
        {
            b.ToTable("table_sessions");
            
            b.Property(x => x.Remarks).HasMaxLength(500);
            b.Property(x => x.BaseHourlyRate).HasColumnType("decimal(10,2)");
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(10,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(10,2)");
            b.Property(x => x.ActualPaidAmount).HasColumnType("decimal(10,2)");

            b.HasIndex(x => x.BilliardTableId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.StoreId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.StartTime);
        });
    }
}