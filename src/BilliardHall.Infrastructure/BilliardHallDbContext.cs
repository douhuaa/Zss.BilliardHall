using Microsoft.EntityFrameworkCore;
using BilliardHall.Domain;

namespace BilliardHall.Infrastructure;

public class BilliardHallDbContext : DbContext
{
    public BilliardHallDbContext(DbContextOptions<BilliardHallDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<BilliardTable> BilliardTables => Set<BilliardTable>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TableSession> TableSessions => Set<TableSession>();
    public DbSet<BillingSnapshot> BillingSnapshots => Set<BillingSnapshot>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceHeartbeat> DeviceHeartbeats => Set<DeviceHeartbeat>();
    public DbSet<EventLog> EventLogs => Set<EventLog>();
    public DbSet<PricingRule> PricingRules => Set<PricingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity mappings to match schema.sql
        ConfigureStore(modelBuilder);
        ConfigureBilliardTable(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureTableSession(modelBuilder);
        ConfigureBillingSnapshot(modelBuilder);
        ConfigurePaymentOrder(modelBuilder);
        ConfigureDevice(modelBuilder);
        ConfigureDeviceHeartbeat(modelBuilder);
        ConfigureEventLog(modelBuilder);
        ConfigurePricingRule(modelBuilder);
    }

    private static void ConfigureStore(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>(entity =>
        {
            entity.ToTable("store");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
            entity.Property(e => e.City).HasColumnName("city").HasMaxLength(64);
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
    }

    private static void ConfigureBilliardTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BilliardTable>(entity =>
        {
            entity.ToTable("billiard_table");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(64);
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.LastSessionId).HasColumnName("last_session_id");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne<Store>()
                .WithMany()
                .HasForeignKey(e => e.StoreId)
                .HasConstraintName("fk_table_store");

            entity.HasIndex(e => new { e.StoreId, e.Code })
                .HasDatabaseName("uk_table_code")
                .IsUnique();
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(32);
            entity.Property(e => e.Nickname).HasColumnName("nickname").HasMaxLength(64);
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.Source).HasColumnName("source").HasMaxLength(64);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Phone).IsUnique();
        });
    }

    private static void ConfigureTableSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TableSession>(entity =>
        {
            entity.ToTable("table_session");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SessionNo).HasColumnName("session_no").HasMaxLength(64);
            entity.Property(e => e.SessionToken).HasColumnName("session_token").HasMaxLength(128);
            entity.Property(e => e.TableId).HasColumnName("table_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalMinutes).HasColumnName("total_minutes");
            entity.Property(e => e.BillingAmount).HasColumnName("billing_amount");
            entity.Property(e => e.PayStatus).HasColumnName("pay_status");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.SessionNo).IsUnique();
            entity.HasIndex(e => e.SessionToken).HasDatabaseName("idx_session_token");
        });
    }

    // Simplified configurations for remaining entities
    private static void ConfigureBillingSnapshot(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingSnapshot>(entity =>
        {
            entity.ToTable("billing_snapshot");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RuleApplied).HasColumnType("json");
            entity.Property(e => e.DetailBreakdown).HasColumnType("json");
        });
    }

    private static void ConfigurePaymentOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.ToTable("payment_order");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestPayload).HasColumnType("json");
            entity.Property(e => e.NotifyPayload).HasColumnType("json");
        });
    }

    private static void ConfigureDevice(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("device");
            entity.HasKey(e => e.Id);
        });
    }

    private static void ConfigureDeviceHeartbeat(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceHeartbeat>(entity =>
        {
            entity.ToTable("device_heartbeat");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MetricsJson).HasColumnType("json");
        });
    }

    private static void ConfigureEventLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.ToTable("event_log");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Payload).HasColumnType("json");
        });
    }

    private static void ConfigurePricingRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PricingRule>(entity =>
        {
            entity.ToTable("pricing_rule");
            entity.HasKey(e => e.Id);
        });
    }
}