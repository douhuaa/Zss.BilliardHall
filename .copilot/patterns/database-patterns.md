# ABP Entity Framework Core + MySQL 数据库设计规范

## 总体原则 (General Principles)

### 1. ABP 框架数据访问模式
- 使用 ABP Entity Framework Core 集成
- 遵循 ABP 实体基类和约定
- 实现多租户数据隔离
- 支持软删除和审计字段
- 使用 ABP 仓储模式

### 2. MySQL 数据库优化
- 针对 MySQL 的字符集和排序规则
- 合理使用 MySQL 数据类型
- 优化索引策略
- 考虑 MySQL 的存储引擎特性
- 实现数据库连接池优化

### 3. 领域驱动设计原则
- 聚合根和实体的正确建模
- 值对象的恰当使用
- 领域事件的集成
- 仓储接口的抽象
- 数据一致性保证

## ABP 实体设计模式

### 1. 聚合根实体

```csharp
// BilliardHall.cs - 聚合根实体
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Zss.BilliardHall.Domain.Entities
{
    /// <summary>
    /// 台球厅聚合根
    /// </summary>
    public class BilliardHall : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; set; }
        
        [Required]
        [StringLength(BilliardHallConsts.MaxNameLength)]
        public string Name { get; protected set; }
        
        [StringLength(BilliardHallConsts.MaxAddressLength)]
        public string Address { get; set; }
        
        [StringLength(BilliardHallConsts.MaxPhoneLength)]
        public string Phone { get; set; }
        
        // MySQL TIME 类型映射
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        
        // JSON 字段 (MySQL 5.7+)
        public BilliardHallSettings Settings { get; set; }
        
        // 导航属性 - 一对多关系
        public virtual ICollection<BilliardTable> Tables { get; protected set; }
        public virtual ICollection<Room> Rooms { get; protected set; }
        
        // 私有构造函数 (EF Core 需要)
        protected BilliardHall() 
        {
            Tables = new List<BilliardTable>();
            Rooms = new List<Room>();
        }
        
        // 工厂方法
        public BilliardHall(
            Guid id,
            string name,
            string address = null,
            TimeSpan? openTime = null,
            TimeSpan? closeTime = null) : base(id)
        {
            SetName(name);
            Address = address;
            OpenTime = openTime ?? TimeSpan.FromHours(9);
            CloseTime = closeTime ?? TimeSpan.FromHours(23);
            Settings = new BilliardHallSettings();
            
            Tables = new List<BilliardTable>();
            Rooms = new List<Room>();
        }
        
        // 业务方法
        public void SetName(string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name), BilliardHallConsts.MaxNameLength);
            Name = name;
        }
        
        public BilliardTable AddTable(int number, BilliardTableType type, decimal hourlyRate)
        {
            Check.Condition(number > 0, nameof(number), "台球桌编号必须大于0");
            
            if (Tables.Any(t => t.Number == number))
            {
                throw new BusinessException(BilliardHallDomainErrorCodes.TableNumberAlreadyExists)
                    .WithData("Number", number)
                    .WithData("HallName", Name);
            }
            
            var table = new BilliardTable(
                GuidGenerator.Create(),
                Id, // 台球厅ID
                number,
                type,
                hourlyRate
            );
            
            Tables.Add(table);
            
            // 添加领域事件
            AddLocalEvent(new BilliardTableAddedEto
            {
                HallId = Id,
                TableId = table.Id,
                TableNumber = number,
                TableType = type
            });
            
            return table;
        }
    }
}
```

### 2. 子实体设计

```csharp
// BilliardTable.cs - 子实体
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Zss.BilliardHall.Domain.Entities
{
    /// <summary>
    /// 台球桌实体
    /// </summary>
    public class BilliardTable : FullAuditedEntity<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; set; }
        
        // 基本属性
        public int Number { get; set; }
        public BilliardTableType Type { get; set; }
        public BilliardTableStatus Status { get; protected set; }
        
        // MySQL DECIMAL 类型，精度为 10,2
        public decimal HourlyRate { get; set; }
        
        // MySQL FLOAT 类型用于坐标
        public float LocationX { get; set; }
        public float LocationY { get; set; }
        
        // 外键
        public Guid BilliardHallId { get; set; }
        
        // 导航属性
        public virtual BilliardHall BilliardHall { get; set; }
        public virtual ICollection<Reservation> Reservations { get; protected set; }
        
        // 私有构造函数
        protected BilliardTable()
        {
            Reservations = new List<Reservation>();
        }
        
        // 公共构造函数
        internal BilliardTable(
            Guid id,
            Guid billiardHallId,
            int number,
            BilliardTableType type,
            decimal hourlyRate,
            float locationX = 0,
            float locationY = 0) : base(id)
        {
            BilliardHallId = billiardHallId;
            Number = number;
            Type = type;
            Status = BilliardTableStatus.Available;
            HourlyRate = hourlyRate;
            LocationX = locationX;
            LocationY = locationY;
            
            Reservations = new List<Reservation>();
        }
        
        // 业务方法
        public void ChangeStatus(BilliardTableStatus newStatus, string reason = null)
        {
            if (Status == newStatus)
                return;
                
            var oldStatus = Status;
            Status = newStatus;
            
            // 添加领域事件
            AddLocalEvent(new BilliardTableStatusChangedEto
            {
                TableId = Id,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Reason = reason
            });
        }
        
        public void UpdateLocation(float x, float y)
        {
            LocationX = x;
            LocationY = y;
        }
    }
}
```

### 3. 值对象设计

```csharp
// BilliardHallSettings.cs - 值对象
using Volo.Abp.Domain.Values;

namespace Zss.BilliardHall.Domain.ValueObjects
{
    /// <summary>
    /// 台球厅设置值对象 (存储为 JSON)
    /// </summary>
    public class BilliardHallSettings : ValueObject
    {
        public bool AllowOnlineReservation { get; set; } = true;
        public int MaxAdvanceReservationDays { get; set; } = 30;
        public decimal DepositRate { get; set; } = 0.2m; // 20% 预付款
        public TimeSpan MaxReservationDuration { get; set; } = TimeSpan.FromHours(4);
        public List<string> PaymentMethods { get; set; } = new();
        
        // 营业设置
        public Dictionary<DayOfWeek, BusinessHours> WeeklyHours { get; set; } = new();
        public List<Holiday> Holidays { get; set; } = new();
        
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return AllowOnlineReservation;
            yield return MaxAdvanceReservationDays;
            yield return DepositRate;
            yield return MaxReservationDuration;
            yield return string.Join(",", PaymentMethods);
        }
    }
    
    public class BusinessHours : ValueObject
    {
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public bool IsClosed { get; set; }
        
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return OpenTime;
            yield return CloseTime;
            yield return IsClosed;
        }
    }
}
```

## ABP DbContext 配置模式

### 1. DbContext 定义

```csharp
// BilliardHallDbContext.cs
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Zss.BilliardHall.EntityFrameworkCore
{
    [ReplaceDbContext(typeof(IBilliardHallDbContext))]
    [ConnectionStringName("Default")]
    public class BilliardHallDbContext : AbpDbContext<BilliardHallDbContext>, IBilliardHallDbContext
    {
        // 实体 DbSet
        public DbSet<BilliardHall> BilliardHalls { get; set; }
        public DbSet<BilliardTable> BilliardTables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public BilliardHallDbContext(DbContextOptions<BilliardHallDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 应用实体配置
            builder.ConfigureBilliardHall();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // MySQL 特定配置
            if (optionsBuilder.IsConfigured)
            {
                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(false);
            }
        }
    }
}
```

### 2. 实体配置 (Fluent API)

```csharp
// BilliardHallDbContextModelCreatingExtensions.cs
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Zss.BilliardHall.EntityFrameworkCore
{
    public static class BilliardHallDbContextModelCreatingExtensions
    {
        public static void ConfigureBilliardHall(this ModelBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            // 配置台球厅表
            builder.Entity<BilliardHall>(b =>
            {
                b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardHalls", BilliardHallConsts.DbSchema);
                b.ConfigureByConvention(); // ABP 约定配置

                // 基本属性配置
                b.Property(x => x.Name)
                 .IsRequired()
                 .HasMaxLength(BilliardHallConsts.MaxNameLength)
                 .HasCharSet("utf8mb4")
                 .HasCollation("utf8mb4_unicode_ci");

                b.Property(x => x.Address)
                 .HasMaxLength(BilliardHallConsts.MaxAddressLength)
                 .HasCharSet("utf8mb4")
                 .HasCollation("utf8mb4_unicode_ci");

                b.Property(x => x.Phone)
                 .HasMaxLength(BilliardHallConsts.MaxPhoneLength);

                // MySQL TIME 类型配置
                b.Property(x => x.OpenTime).HasColumnType("time");
                b.Property(x => x.CloseTime).HasColumnType("time");

                // JSON 字段配置 (MySQL 5.7+)
                b.Property(x => x.Settings)
                 .HasColumnType("json")
                 .HasConversion(
                     v => JsonSerializer.Serialize(v),
                     v => JsonSerializer.Deserialize<BilliardHallSettings>(v));

                // 索引配置
                b.HasIndex(x => x.Name).HasDatabaseName("IX_BilliardHalls_Name");
                b.HasIndex(x => x.TenantId).HasDatabaseName("IX_BilliardHalls_TenantId");
                b.HasIndex(x => new { x.TenantId, x.Name })
                 .IsUnique()
                 .HasDatabaseName("UK_BilliardHalls_TenantId_Name");
            });

            // 配置台球桌表
            builder.Entity<BilliardTable>(b =>
            {
                b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardTables", BilliardHallConsts.DbSchema);
                b.ConfigureByConvention();

                // 基本属性
                b.Property(x => x.Number).IsRequired();
                b.Property(x => x.Type).IsRequired().HasConversion<int>();
                b.Property(x => x.Status).IsRequired().HasConversion<int>()
                 .HasDefaultValue(BilliardTableStatus.Available);

                // MySQL DECIMAL 类型
                b.Property(x => x.HourlyRate)
                 .IsRequired()
                 .HasPrecision(10, 2)
                 .HasColumnType("decimal(10,2)");

                // MySQL FLOAT 类型
                b.Property(x => x.LocationX).HasColumnType("float");
                b.Property(x => x.LocationY).HasColumnType("float");

                // 外键关系
                b.HasOne(x => x.BilliardHall)
                 .WithMany(x => x.Tables)
                 .HasForeignKey(x => x.BilliardHallId)
                 .OnDelete(DeleteBehavior.Cascade)
                 .HasConstraintName("FK_BilliardTables_BilliardHalls");

                // 唯一约束
                b.HasIndex(x => new { x.BilliardHallId, x.Number })
                 .IsUnique()
                 .HasDatabaseName("UK_BilliardTables_HallId_Number");

                // 性能索引
                b.HasIndex(x => x.Status).HasDatabaseName("IX_BilliardTables_Status");
                b.HasIndex(x => x.Type).HasDatabaseName("IX_BilliardTables_Type");
                b.HasIndex(x => x.TenantId).HasDatabaseName("IX_BilliardTables_TenantId");
                b.HasIndex(x => new { x.TenantId, x.Status })
                 .HasDatabaseName("IX_BilliardTables_TenantId_Status");
            });

            // 配置预约表
            builder.Entity<Reservation>(b =>
            {
                b.ToTable(BilliardHallConsts.DbTablePrefix + "Reservations", BilliardHallConsts.DbSchema);
                b.ConfigureByConvention();

                // 时间字段 - MySQL DATETIME(6) 支持微秒
                b.Property(x => x.StartTime)
                 .IsRequired()
                 .HasColumnType("datetime(6)");

                b.Property(x => x.EndTime)
                 .IsRequired()
                 .HasColumnType("datetime(6)");

                // 金额字段
                b.Property(x => x.TotalAmount)
                 .HasPrecision(10, 2)
                 .HasColumnType("decimal(10,2)");

                b.Property(x => x.DepositAmount)
                 .HasPrecision(10, 2)
                 .HasColumnType("decimal(10,2)");

                // 外键关系
                b.HasOne(x => x.BilliardTable)
                 .WithMany(x => x.Reservations)
                 .HasForeignKey(x => x.BilliardTableId)
                 .OnDelete(DeleteBehavior.Restrict); // 防止误删

                b.HasOne<IdentityUser>()
                 .WithMany()
                 .HasForeignKey(x => x.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

                // 业务索引
                b.HasIndex(x => new { x.BilliardTableId, x.StartTime })
                 .HasDatabaseName("IX_Reservations_TableId_StartTime");

                b.HasIndex(x => new { x.CustomerId, x.Status })
                 .HasDatabaseName("IX_Reservations_CustomerId_Status");

                b.HasIndex(x => x.StartTime)
                 .HasDatabaseName("IX_Reservations_StartTime");
            });
        }
    }
}
```

## MySQL 数据库迁移模式

### 1. Migration 创建

```csharp
// 20240909120000_AddBilliardTables.cs
[Migration("20240909120000")]
public partial class AddBilliardTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppBilliardHalls",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, 
                                          charset: "utf8mb4", collation: "utf8mb4_unicode_ci"),
                Address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true,
                                             charset: "utf8mb4", collation: "utf8mb4_unicode_ci"),
                Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                OpenTime = table.Column<TimeSpan>(type: "time", nullable: false),
                CloseTime = table.Column<TimeSpan>(type: "time", nullable: false),
                Settings = table.Column<string>(type: "json", nullable: true),
                ExtraProperties = table.Column<string>(type: "longtext", nullable: false, charset: "utf8mb4"),
                ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, charset: "utf8mb4"),
                CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppBilliardHalls", x => x.Id);
            })
            .Annotation("MySQL:Charset", "utf8mb4")
            .Annotation("MySQL:Collation", "utf8mb4_unicode_ci");

        migrationBuilder.CreateTable(
            name: "AppBilliardTables",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                TenantId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                Number = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                HourlyRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                LocationX = table.Column<float>(type: "float", nullable: false),
                LocationY = table.Column<float>(type: "float", nullable: false),
                BilliardHallId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                ExtraProperties = table.Column<string>(type: "longtext", nullable: false, charset: "utf8mb4"),
                ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false, charset: "utf8mb4"),
                CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CreatorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppBilliardTables", x => x.Id);
                table.ForeignKey(
                    name: "FK_BilliardTables_BilliardHalls",
                    column: x => x.BilliardHallId,
                    principalTable: "AppBilliardHalls",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        // 创建索引
        migrationBuilder.CreateIndex(
            name: "IX_BilliardHalls_Name",
            table: "AppBilliardHalls",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_BilliardHalls_TenantId",
            table: "AppBilliardHalls",
            column: "TenantId");

        migrationBuilder.CreateIndex(
            name: "UK_BilliardHalls_TenantId_Name",
            table: "AppBilliardHalls",
            columns: new[] { "TenantId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UK_BilliardTables_HallId_Number",
            table: "AppBilliardTables",
            columns: new[] { "BilliardHallId", "Number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_BilliardTables_Status",
            table: "AppBilliardTables",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_BilliardTables_Type",
            table: "AppBilliardTables",
            column: "Type");

        migrationBuilder.CreateIndex(
            name: "IX_BilliardTables_TenantId_Status",
            table: "AppBilliardTables",
            columns: new[] { "TenantId", "Status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AppBilliardTables");
        migrationBuilder.DropTable(name: "AppBilliardHalls");
    }
}
```

### 2. 数据种子配置

```csharp
// BilliardHallDataSeeder.cs
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Zss.BilliardHall.EntityFrameworkCore
{
    public class BilliardHallDataSeeder : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<BilliardHall, Guid> _billiardHallRepository;
        private readonly BilliardHallManager _billiardHallManager;

        public BilliardHallDataSeeder(
            IRepository<BilliardHall, Guid> billiardHallRepository,
            BilliardHallManager billiardHallManager)
        {
            _billiardHallRepository = billiardHallRepository;
            _billiardHallManager = billiardHallManager;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // 检查是否已有数据
            if (await _billiardHallRepository.CountAsync() > 0)
                return;

            // 创建示例台球厅
            var hall = await _billiardHallManager.CreateAsync(
                "星际台球厅",
                "北京市朝阳区某某街道123号",
                "010-12345678"
            );

            // 添加台球桌
            hall.AddTable(1, BilliardTableType.ChineseEightBall, 50.00m);
            hall.AddTable(2, BilliardTableType.ChineseEightBall, 50.00m);
            hall.AddTable(3, BilliardTableType.AmericanNineBall, 60.00m);
            hall.AddTable(4, BilliardTableType.Snooker, 80.00m);

            await _billiardHallRepository.InsertAsync(hall);
        }
    }
}
```

## MySQL 性能优化模式

### 1. 查询优化

```csharp
// BilliardTableRepository.cs - 自定义仓储
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Zss.BilliardHall.EntityFrameworkCore.Repositories
{
    public class BilliardTableRepository : EfCoreRepository<BilliardHallDbContext, BilliardTable, Guid>, 
                                          IBilliardTableRepository
    {
        public BilliardTableRepository(IDbContextProvider<BilliardHallDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task<List<BilliardTable>> GetAvailableTablesAsync(
            BilliardTableType? type = null,
            DateTime? startTime = null,
            DateTime? endTime = null)
        {
            var query = await GetQueryableAsync();

            // 基础过滤
            query = query.Where(t => t.Status == BilliardTableStatus.Available);

            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            // 时间冲突检查 - 使用 SQL 优化
            if (startTime.HasValue && endTime.HasValue)
            {
                query = query.Where(t => !t.Reservations.Any(r => 
                    r.Status == ReservationStatus.Confirmed &&
                    r.StartTime < endTime && r.EndTime > startTime));
            }

            // 包含相关数据，减少 N+1 查询
            return await query
                .Include(t => t.BilliardHall)
                .OrderBy(t => t.Number)
                .ToListAsync();
        }

        public async Task<Dictionary<BilliardTableStatus, int>> GetStatusStatisticsAsync(Guid? hallId = null)
        {
            var query = await GetQueryableAsync();

            if (hallId.HasValue)
            {
                query = query.Where(t => t.BilliardHallId == hallId.Value);
            }

            // 使用 GroupBy 进行统计，生成高效 SQL
            var result = await query
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(x => x.Status, x => x.Count);
        }

        public async Task<bool> IsNumberUniqueAsync(int number, Guid hallId, Guid? excludeId = null)
        {
            var query = await GetQueryableAsync();
            
            query = query.Where(t => t.Number == number && t.BilliardHallId == hallId);
            
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            // 使用 AnyAsync 进行存在性检查，生成 EXISTS 查询
            return !await query.AnyAsync();
        }
    }
}
```

### 2. 连接池配置

```csharp
// 在 Program.cs 或 Module 中配置
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var configuration = context.Services.GetConfiguration();
    var connectionString = configuration.GetConnectionString("Default");

    Configure<AbpDbContextOptions>(options =>
    {
        options.Configure<BilliardHallDbContext>(context =>
        {
            context.DbContextOptions.UseMySql(connectionString, 
                ServerVersion.AutoDetect(connectionString), mySqlOptions =>
                {
                    // 连接池配置
                    mySqlOptions.CommandTimeout(30);
                    
                    // 重试策略
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                    
                    // 字符集配置
                    mySqlOptions.CharSetBehavior(CharSetBehavior.NeverAppend);
                    mySqlOptions.CharSet(CharSet.Utf8mb4);
                });
        });
    });
}
```

### 3. 索引优化策略

```sql
-- 复合索引优化建议

-- 1. 多租户 + 状态查询
CREATE INDEX IX_BilliardTables_TenantId_Status_Type 
ON AppBilliardTables (TenantId, Status, Type);

-- 2. 预约时间查询优化
CREATE INDEX IX_Reservations_TableId_StartTime_EndTime_Status 
ON AppReservations (BilliardTableId, StartTime, EndTime, Status);

-- 3. 客户预约查询
CREATE INDEX IX_Reservations_CustomerId_StartTime_Status 
ON AppReservations (CustomerId, StartTime, Status);

-- 4. 覆盖索引 (包含常用字段，避免回表)
CREATE INDEX IX_BilliardTables_Status_COVERING 
ON AppBilliardTables (Status) 
INCLUDE (Id, Number, Type, HourlyRate);

-- 5. 全文搜索索引 (MySQL 5.7+)
ALTER TABLE AppBilliardHalls ADD FULLTEXT INDEX FT_BilliardHalls_Name_Address (Name, Address);
```

## 多租户数据隔离模式

### 1. 行级租户隔离

```csharp
// 自动租户过滤器配置
public override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // ABP 自动配置多租户过滤器
    builder.ConfigureByConvention();
    
    // 手动配置全局查询过滤器
    builder.Entity<BilliardTable>()
           .HasQueryFilter(t => !CurrentTenant.IsAvailable || t.TenantId == CurrentTenant.Id);
}

// 在 Application Service 中使用
public class BilliardTableAppService : BilliardHallAppService
{
    public async Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input)
    {
        // ABP 自动应用租户过滤，无需手动添加 TenantId 条件
        var queryable = await _billiardTableRepository.GetQueryableAsync();
        
        // 其他业务过滤...
        var result = await AsyncExecuter.ToListAsync(queryable);
        return new PagedResultDto<BilliardTableDto>(totalCount, result);
    }
}
```

### 2. 跨租户数据访问

```csharp
// 跨租户数据查询
public class ReportAppService : BilliardHallAppService
{
    public async Task<GlobalStatisticsDto> GetGlobalStatisticsAsync()
    {
        // 使用 DataFilter 临时禁用多租户过滤
        using (DataFilter.Disable<IMultiTenant>())
        {
            var totalTables = await _billiardTableRepository.CountAsync();
            var totalReservations = await _reservationRepository.CountAsync();
            
            return new GlobalStatisticsDto
            {
                TotalTables = totalTables,
                TotalReservations = totalReservations
            };
        }
    }
}
```

这些数据库设计模式确保了与 ABP 框架的完美集成，同时充分利用了 MySQL 的特性和性能优化机制。