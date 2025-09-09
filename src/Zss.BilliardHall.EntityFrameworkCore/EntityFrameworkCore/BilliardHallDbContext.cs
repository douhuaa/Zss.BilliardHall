using Microsoft.EntityFrameworkCore;

using Volo.Abp.AuditLogging.EntityFrameworkCore;

using Zss.BilliardHall.Books;
using Zss.BilliardHall.BilliardHalls;
using Zss.BilliardHall.Customers;
using Zss.BilliardHall.Reservations;

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

namespace Zss.BilliardHall.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class BilliardHallDbContext : AbpDbContext<BilliardHallDbContext>, ITenantManagementDbContext, IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<Book> Books { get; set; }
    
    // 智慧台球厅管理系统实体
    public DbSet<BilliardHall.BilliardHalls.BilliardHall> BilliardHalls { get; set; }
    public DbSet<BilliardTable> BilliardTables { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

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

        builder.Entity<Book>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "Books", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(128);
        });

        /* Configure your own tables/entities inside here */
        
        // 台球厅配置
        builder.Entity<BilliardHall.BilliardHalls.BilliardHall>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardHalls", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.BilliardHallConsts.MaxNameLength);
            
            b.Property(x => x.Address)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.BilliardHallConsts.MaxAddressLength);
            
            b.Property(x => x.Description)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.BilliardHallConsts.MaxDescriptionLength);
            
            b.Property(x => x.Phone)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.BilliardHallConsts.MaxPhoneLength);
            
            b.Property(x => x.Email)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.BilliardHallConsts.MaxEmailLength);
                
            b.Property(x => x.OpenTime)
                .IsRequired();
                
            b.Property(x => x.CloseTime)
                .IsRequired();
                
            b.Property(x => x.IsActive)
                .IsRequired();
            
            // 配置一对多关系
            b.HasMany(x => x.Tables)
                .WithOne(x => x.BilliardHall)
                .HasForeignKey(x => x.BilliardHallId)
                .IsRequired();
        });
        
        // 台球桌配置
        builder.Entity<BilliardTable>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardTables", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.Number)
                .IsRequired();
            
            b.Property(x => x.Type)
                .IsRequired();
            
            b.Property(x => x.Status)
                .IsRequired();
            
            b.Property(x => x.HourlyRate)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            b.Property(x => x.LocationX)
                .HasColumnType("float");
            
            b.Property(x => x.LocationY)
                .HasColumnType("float");
            
            b.Property(x => x.Description)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.BilliardTableConsts.MaxDescriptionLength);
            
            b.Property(x => x.BilliardHallId)
                .IsRequired();
            
            // 唯一索引：同一台球厅内台球桌编号唯一
            b.HasIndex(x => new { x.BilliardHallId, x.Number })
                .IsUnique()
                .HasDatabaseName("IX_BilliardTables_HallId_Number");
            
            // 多租户索引
            b.HasIndex(x => x.TenantId)
                .HasDatabaseName("IX_BilliardTables_TenantId");
        });
        
        // 客户配置
        builder.Entity<Customer>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "Customers", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.CustomerConsts.MaxNameLength);
            
            b.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.CustomerConsts.MaxPhoneLength);
            
            b.Property(x => x.Email)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.CustomerConsts.MaxEmailLength);
            
            b.Property(x => x.Level)
                .IsRequired();
            
            b.Property(x => x.Points)
                .IsRequired()
                .HasDefaultValue(0);
            
            b.Property(x => x.Balance)
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);
            
            b.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            b.Property(x => x.Notes)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.CustomerConsts.MaxNotesLength);
            
            b.Property(x => x.RegisterTime)
                .IsRequired();
            
            // 唯一索引：手机号唯一
            b.HasIndex(x => new { x.TenantId, x.Phone })
                .IsUnique()
                .HasDatabaseName("IX_Customers_TenantId_Phone");
            
            // 邮箱索引（如果不为空则唯一）
            b.HasIndex(x => new { x.TenantId, x.Email })
                .IsUnique()
                .HasDatabaseName("IX_Customers_TenantId_Email")
                .HasFilter("[Email] IS NOT NULL");
            
            // 多租户索引
            b.HasIndex(x => x.TenantId)
                .HasDatabaseName("IX_Customers_TenantId");
        });
        
        // 预约配置
        builder.Entity<Reservation>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "Reservations", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            b.Property(x => x.ReservationNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            b.Property(x => x.CustomerId)
                .IsRequired();
            
            b.Property(x => x.BilliardTableId)
                .IsRequired();
            
            b.Property(x => x.StartTime)
                .IsRequired();
            
            b.Property(x => x.DurationMinutes)
                .IsRequired();
            
            b.Property(x => x.EndTime)
                .IsRequired();
            
            b.Property(x => x.Status)
                .IsRequired();
            
            b.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            b.Property(x => x.Notes)
                .HasMaxLength(Zss.BilliardHall.BilliardHalls.ReservationConsts.MaxNotesLength);
            
            // 配置外键关系
            b.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            b.HasOne(x => x.BilliardTable)
                .WithMany()
                .HasForeignKey(x => x.BilliardTableId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            // 唯一索引：预约编号唯一
            b.HasIndex(x => x.ReservationNumber)
                .IsUnique()
                .HasDatabaseName("IX_Reservations_ReservationNumber");
            
            // 复合索引：台球桌和时间段查询优化
            b.HasIndex(x => new { x.BilliardTableId, x.StartTime, x.EndTime })
                .HasDatabaseName("IX_Reservations_Table_TimeSlot");
            
            // 客户索引
            b.HasIndex(x => x.CustomerId)
                .HasDatabaseName("IX_Reservations_CustomerId");
            
            // 多租户索引
            b.HasIndex(x => x.TenantId)
                .HasDatabaseName("IX_Reservations_TenantId");
        });

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(BilliardHallConsts.DbTablePrefix + "YourEntities", BilliardHallConsts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
