using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using Zss.BilliardHall.Entities;

namespace Zss.BilliardHall.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class BilliardHallDbContext :
    AbpDbContext<BilliardHallDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    
    // 业务实体
    public DbSet<Store> Stores { get; set; }
    public DbSet<BilliardTable> BilliardTables { get; set; }
    public DbSet<TableSession> TableSessions { get; set; }
    public DbSet<PaymentOrder> PaymentOrders { get; set; }


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public BilliardHallDbContext(DbContextOptions<BilliardHallDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();
        
        /* Configure your own tables/entities inside here */

        // 门店配置
        builder.Entity<Store>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "Stores", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Address).HasMaxLength(500);
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.BaseHourlyRate).HasPrecision(18, 2);
            
            b.HasIndex(x => x.Name);
        });

        // 台桌配置
        builder.Entity<BilliardTable>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardTables", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.Number).IsRequired().HasMaxLength(50);
            b.Property(x => x.Name).HasMaxLength(100);
            b.Property(x => x.QrCode).HasMaxLength(500);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.HourlyRate).HasPrecision(18, 2);
            
            b.HasIndex(x => x.StoreId);
            b.HasIndex(x => new { x.StoreId, x.Number }).IsUnique();
            b.HasIndex(x => x.QrCode);
            
            b.HasOne(x => x.Store)
             .WithMany()
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // 会话配置
        builder.Entity<TableSession>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "TableSessions", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.SessionToken).HasMaxLength(100);
            b.Property(x => x.Remarks).HasMaxLength(500);
            b.Property(x => x.HourlyRateSnapshot).HasPrecision(18, 2);
            b.Property(x => x.CalculatedAmount).HasPrecision(18, 2);
            b.Property(x => x.PaidAmount).HasPrecision(18, 2);
            
            b.HasIndex(x => x.TableId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.SessionToken).IsUnique();
            b.HasIndex(x => x.StartTime);
            
            b.HasOne(x => x.Table)
             .WithMany()
             .HasForeignKey(x => x.TableId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // 支付订单配置
        builder.Entity<PaymentOrder>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "PaymentOrders", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.OrderNumber).IsRequired().HasMaxLength(50);
            b.Property(x => x.ThirdPartyTransactionId).HasMaxLength(100);
            b.Property(x => x.CallbackData).HasMaxLength(2000);
            b.Property(x => x.Remarks).HasMaxLength(500);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.PaidAmount).HasPrecision(18, 2);
            
            b.HasIndex(x => x.OrderNumber).IsUnique();
            b.HasIndex(x => x.SessionId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.PaymentTime);
            
            b.HasOne(x => x.Session)
             .WithMany()
             .HasForeignKey(x => x.SessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
