# 5.6 EF Core Code First 开发工作流程

## 概述

本文档详细说明了基于 EF Core Code First 方式的数据库开发工作流程，包括实体建模、迁移管理、部署策略等完整流程。

> 💡 **相关章节**：
> - 实体建模规范请参考 [5.2 表结构定义](表结构定义.md)
> - 业务逻辑设计请参考 [5.3 关键表说明](关键表说明.md)
> - 迁移命令详解请参考 [5.5 数据迁移方案](数据迁移方案.md)
> - Git 规范请参考 [6.4 Git 分支规范](../06_开发规范/Git分支规范.md)

## 开发环境准备

### 1. 工具安装

```bash
# 安装 EF Core 工具
dotnet tool install --global dotnet-ef

# 更新到最新版本
dotnet tool update --global dotnet-ef

# 验证安装
dotnet ef --version
```

### 2. 项目结构

```
src/
├── Zss.BilliardHall.Domain/           # 领域层
│   ├── Entities/                      # 实体定义
│   ├── Repositories/                  # 仓储接口
│   └── Shared/                        # 共享类型
├── Zss.BilliardHall.EntityFrameworkCore/  # 数据访问层
│   ├── EntityFrameworkCore/           # DbContext 和配置
│   ├── Repositories/                  # 仓储实现
│   └── Migrations/                    # 迁移文件
├── Zss.BilliardHall.DbMigrator/       # 迁移工具项目
└── Zss.BilliardHall.HttpApi.Host/     # Web API 项目
```

### 3. 连接字符串配置

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=BilliardHall_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "EntityFramework": {
    "LogLevel": "Information"
  }
}
```

## 实体建模规范

### 1. 实体基类选择

```csharp
// 完整审计实体（推荐用于业务核心实体）
public class Store : FullAuditedAggregateRoot<long>
{
    // 业务属性...
}

// 基础审计实体（用于配置类实体）
public class PricingRule : AuditedEntity<long>
{
    // 业务属性...
}

// 简单实体（用于日志类实体）
public class DeviceHeartbeat : Entity<long>
{
    // 业务属性...
}
```

### 2. 实体属性规范

```csharp
public class BilliardTable : FullAuditedAggregateRoot<long>
{
    // 必填字符串属性
    [Required]
    [MaxLength(20)]
    public string TableNumber { get; set; }
    
    // 可选字符串属性
    [MaxLength(500)]
    public string Location { get; set; }
    
    // 枚举属性
    public TableStatus Status { get; set; } = TableStatus.Available;
    
    // 货币金额属性
    [Column(TypeName = "decimal(18,2)")]
    public decimal HourlyRate { get; set; }
    
    // 外键属性
    public long StoreId { get; set; }
    
    // 导航属性
    public virtual Store Store { get; set; }
    public virtual ICollection<TableSession> Sessions { get; set; }
    
    // 构造函数（用于创建时的默认值）
    public BilliardTable()
    {
        Sessions = new HashSet<TableSession>();
    }
}
```

### 3. 枚举定义规范

```csharp
/// <summary>
/// 球台状态枚举
/// </summary>
public enum TableStatus
{
    /// <summary>
    /// 可用
    /// </summary>
    Available = 1,
    
    /// <summary>
    /// 使用中
    /// </summary>
    Occupied = 2,
    
    /// <summary>
    /// 已预约
    /// </summary>
    Reserved = 3,
    
    /// <summary>
    /// 维护中
    /// </summary>
    Maintenance = 4,
    
    /// <summary>
    /// 故障
    /// </summary>
    OutOfOrder = 5
}
```

## DbContext 配置

### 1. 主 DbContext

```csharp
[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
public class BilliardHallDbContext : 
    AbpDbContext<BilliardHallDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    // 实体集合
    public DbSet<Store> Stores { get; set; }
    public DbSet<BilliardTable> BilliardTables { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TableSession> TableSessions { get; set; }
    public DbSet<PaymentOrder> PaymentOrders { get; set; }
    public DbSet<BillingSnapshot> BillingSnapshots { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceHeartbeat> DeviceHeartbeats { get; set; }
    public DbSet<Membership> Memberships { get; set; }
    
    // ABP 框架相关 DbSet
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    // ... 其他 ABP 实体
    
    public BilliardHallDbContext(DbContextOptions<BilliardHallDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // 配置 ABP 框架实体
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureIdentityServer();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();
        
        // 配置业务实体
        ConfigureBilliardHall(builder);
    }
    
    private void ConfigureBilliardHall(ModelBuilder builder)
    {
        builder.Entity<Store>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "Stores", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            // 属性配置
            b.Property(x => x.Name).IsRequired().HasMaxLength(StoreConsts.MaxNameLength);
            b.Property(x => x.Address).HasMaxLength(StoreConsts.MaxAddressLength);
            b.Property(x => x.ContactPhone).HasMaxLength(StoreConsts.MaxPhoneLength);
            
            // 索引
            b.HasIndex(x => x.Name);
        });
        
        builder.Entity<BilliardTable>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardTables", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            // 属性配置
            b.Property(x => x.TableNumber).IsRequired().HasMaxLength(TableConsts.MaxTableNumberLength);
            b.Property(x => x.HourlyRate).HasColumnType("decimal(18,2)");
            b.Property(x => x.Location).HasMaxLength(TableConsts.MaxLocationLength);
            
            // 索引
            b.HasIndex(x => new { x.StoreId, x.TableNumber }).IsUnique();
            b.HasIndex(x => x.Status);
            
            // 关系配置
            b.HasOne(x => x.Store)
             .WithMany(x => x.Tables)
             .HasForeignKey(x => x.StoreId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<TableSession>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "TableSessions", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();
            
            // 属性配置
            b.Property(x => x.SessionToken).HasMaxLength(SessionConsts.MaxTokenLength);
            b.Property(x => x.HourlyRate).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.FinalAmount).HasColumnType("decimal(18,2)");
            
            // 索引
            b.HasIndex(x => x.SessionToken).IsUnique();
            b.HasIndex(x => new { x.TableId, x.StartTime });
            b.HasIndex(x => new { x.UserId, x.Status });
            
            // 关系配置
            b.HasOne(x => x.User)
             .WithMany(x => x.Sessions)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);
             
            b.HasOne(x => x.Table)
             .WithMany(x => x.Sessions)
             .HasForeignKey(x => x.TableId)
             .OnDelete(DeleteBehavior.Restrict);
        });
        
        // ... 其他实体配置
    }
}
```

### 2. 常量定义

```csharp
public static class BilliardHallConsts
{
    public const string DbTablePrefix = "App";
    public const string DbSchema = null;
}

public static class StoreConsts
{
    public const int MaxNameLength = 100;
    public const int MaxAddressLength = 500;
    public const int MaxPhoneLength = 20;
}
```

## 迁移开发流程

### 1. 创建新实体的完整流程

```bash
# Step 1: 在 Domain 层创建实体类
# Step 2: 在 EntityFrameworkCore 层配置实体映射
# Step 3: 添加 DbSet 到 DbContext
# Step 4: 生成迁移文件

dotnet ef migrations add AddMembershipEntity \
  -p src/Zss.BilliardHall.EntityFrameworkCore \
  -s src/Zss.BilliardHall.DbMigrator \
  --verbose
```

### 2. 修改现有实体的流程

```bash
# Step 1: 修改实体属性
# Step 2: 更新 DbContext 配置（如需要）
# Step 3: 生成迁移文件

dotnet ef migrations add UpdateUserAddBalance \
  -p src/Zss.BilliardHall.EntityFrameworkCore \
  -s src/Zss.BilliardHall.DbMigrator
```

### 3. 迁移文件质量检查

```csharp
public partial class AddMembershipEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppMemberships",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                // ... 其他列
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppMemberships", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppMemberships_AppUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppMemberships_UserId",
            table: "AppMemberships",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AppMemberships");
    }
}
```

### 4. 数据迁移处理

```csharp
// 复杂的数据迁移示例
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. 添加新列（可空）
    migrationBuilder.AddColumn<string>(
        name: "NewColumn",
        table: "AppUsers",
        type: "nvarchar(100)",
        maxLength: 100,
        nullable: true);
    
    // 2. 数据迁移
    migrationBuilder.Sql(@"
        UPDATE AppUsers 
        SET NewColumn = CASE 
            WHEN LegacyColumn IS NOT NULL THEN CONVERT(NVARCHAR(100), LegacyColumn)
            ELSE '默认值'
        END
    ");
    
    // 3. 设置为非空
    migrationBuilder.AlterColumn<string>(
        name: "NewColumn",
        table: "AppUsers",
        type: "nvarchar(100)",
        maxLength: 100,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(100)",
        oldMaxLength: 100,
        oldNullable: true);
    
    // 4. 删除旧列
    migrationBuilder.DropColumn(
        name: "LegacyColumn",
        table: "AppUsers");
}
```

## 数据种子管理

### 1. 数据种子基类

```csharp
public abstract class BilliardHallDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; set; }
    
    protected BilliardHallDataSeedContributor(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<BilliardHallDataSeedContributor>>();
    }
    
    public abstract Task SeedAsync(DataSeedContext context);
    
    protected async Task<T> GetRequiredServiceAsync<T>()
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}
```

### 2. 门店数据种子

```csharp
public class StoreDataSeedContributor : BilliardHallDataSeedContributor
{
    public StoreDataSeedContributor(IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
    }
    
    public override async Task SeedAsync(DataSeedContext context)
    {
        var storeRepository = await GetRequiredServiceAsync<IRepository<Store, long>>();
        
        if (await storeRepository.GetCountAsync() > 0)
        {
            Logger.LogInformation("门店数据已存在，跳过种子数据");
            return;
        }
        
        var stores = new[]
        {
            new Store
            {
                Name = "旗舰店",
                Address = "北京市朝阳区三里屯SOHO 1号楼",
                ContactPhone = "010-12345678",
                Status = StoreStatus.Active,
                OpenTime = new TimeSpan(9, 0, 0),
                CloseTime = new TimeSpan(23, 0, 0),
                Description = "我们的旗舰门店，设备齐全，环境优雅"
            },
            new Store
            {
                Name = "CBD分店", 
                Address = "北京市朝阳区国贸大厦B座",
                ContactPhone = "010-87654321",
                Status = StoreStatus.Active,
                OpenTime = new TimeSpan(10, 0, 0),
                CloseTime = new TimeSpan(22, 0, 0),
                Description = "CBD核心区域，交通便利"
            }
        };
        
        await storeRepository.InsertManyAsync(stores);
        Logger.LogInformation($"已创建 {stores.Length} 个门店");
    }
}
```

### 3. 球台数据种子

```csharp
public class BilliardTableDataSeedContributor : BilliardHallDataSeedContributor
{
    public BilliardTableDataSeedContributor(IServiceProvider serviceProvider) 
        : base(serviceProvider)
    {
    }
    
    public override async Task SeedAsync(DataSeedContext context)
    {
        var storeRepository = await GetRequiredServiceAsync<IRepository<Store, long>>();
        var tableRepository = await GetRequiredServiceAsync<IRepository<BilliardTable, long>>();
        
        var stores = await storeRepository.GetListAsync();
        if (!stores.Any())
        {
            Logger.LogWarning("没有找到门店数据，无法创建球台");
            return;
        }
        
        foreach (var store in stores)
        {
            var existingTables = await tableRepository.GetCountAsync(x => x.StoreId == store.Id);
            if (existingTables > 0)
            {
                Logger.LogInformation($"门店 {store.Name} 的球台数据已存在，跳过");
                continue;
            }
            
            var tables = new List<BilliardTable>();
            
            // 标准台
            for (int i = 1; i <= 8; i++)
            {
                tables.Add(new BilliardTable
                {
                    TableNumber = $"标准{i:D2}",
                    StoreId = store.Id,
                    Type = TableType.Standard,
                    Status = TableStatus.Available,
                    HourlyRate = 30.00m,
                    Location = $"一楼标准区{i}号位"
                });
            }
            
            // 斯诺克台
            for (int i = 1; i <= 4; i++)
            {
                tables.Add(new BilliardTable
                {
                    TableNumber = $"斯诺克{i:D2}",
                    StoreId = store.Id,
                    Type = TableType.Snooker,
                    Status = TableStatus.Available,
                    HourlyRate = 40.00m,
                    Location = $"二楼斯诺克区{i}号位"
                });
            }
            
            // VIP包间
            for (int i = 1; i <= 2; i++)
            {
                tables.Add(new BilliardTable
                {
                    TableNumber = $"VIP{i:D2}",
                    StoreId = store.Id,
                    Type = TableType.Standard,
                    Status = TableStatus.Available,
                    HourlyRate = 60.00m,
                    Location = $"三楼VIP包间{i}"
                });
            }
            
            await tableRepository.InsertManyAsync(tables);
            Logger.LogInformation($"为门店 {store.Name} 创建了 {tables.Count} 张球台");
        }
    }
}
```

## 环境管理

### 1. 开发环境配置

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=BilliardHall_Dev;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "EntityFramework": {
    "LogLevel": "Information",
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  },
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### 2. 测试环境配置

```json
// appsettings.Staging.json
{
  "ConnectionStrings": {
    "Default": "Server=test-db.company.com;Database=BilliardHall_Test;User Id=testuser;Password=testpass123;TrustServerCertificate=true;"
  },
  "EntityFramework": {
    "LogLevel": "Warning"
  }
}
```

### 3. 生产环境配置

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "Default": "${DB_CONNECTION_STRING}"
  },
  "EntityFramework": {
    "LogLevel": "Error"
  }
}
```

## 部署策略

### 1. 自动迁移部署

```csharp
// Program.cs 中的自动迁移
public async Task<int> Main(string[] args)
{
    var configuration = BuildConfiguration();
    
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

    try
    {
        Log.Information("启动数据库迁移器");
        await CreateHostBuilder(args).RunConsoleAsync();
        Log.Information("数据库迁移完成");
        return 0;
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "数据库迁移失败");
        return 1;
    }
    finally
    {
        Log.CloseAndFlush();
    }
}
```

### 2. Docker 容器部署

```dockerfile
# Dockerfile.DbMigrator
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Zss.BilliardHall.DbMigrator/Zss.BilliardHall.DbMigrator.csproj", "src/Zss.BilliardHall.DbMigrator/"]
COPY ["src/Zss.BilliardHall.EntityFrameworkCore/Zss.BilliardHall.EntityFrameworkCore.csproj", "src/Zss.BilliardHall.EntityFrameworkCore/"]

RUN dotnet restore "src/Zss.BilliardHall.DbMigrator/Zss.BilliardHall.DbMigrator.csproj"

COPY . .
WORKDIR "/src/src/Zss.BilliardHall.DbMigrator"
RUN dotnet build "Zss.BilliardHall.DbMigrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Zss.BilliardHall.DbMigrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Zss.BilliardHall.DbMigrator.dll"]
```

### 3. CI/CD 集成

```yaml
# .github/workflows/deploy.yml
name: 部署应用

on:
  push:
    branches: [ main ]

jobs:
  database-migration:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: 设置 .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: 恢复依赖
      run: dotnet restore
    
    - name: 构建项目
      run: dotnet build --configuration Release --no-restore
    
    - name: 运行数据库迁移
      run: dotnet run --project src/Zss.BilliardHall.DbMigrator --configuration Release
      env:
        ConnectionStrings__Default: ${{ secrets.DATABASE_CONNECTION_STRING }}
```

## 最佳实践总结

### 1. 命名规范

- **实体类名**：使用业务领域术语，如 `BilliardTable`、`TableSession`
- **属性名**：使用英文，清晰表达含义，如 `HourlyRate`、`SessionToken`
- **迁移名**：英文动宾结构，如 `AddUserBalanceColumn`、`UpdatePaymentSchema`
- **提交信息**：中文描述，如 `feat(数据库): 添加会员管理相关表结构`

### 2. 性能优化

- 合理设计索引，避免过度索引
- 使用合适的数据类型，如 `decimal` 用于货币
- 避免在 `OnModelCreating` 中执行复杂逻辑
- 使用 `AsNoTracking()` 进行只读查询

### 3. 安全注意事项

- 敏感数据加密存储
- 使用连接字符串加密
- 定期备份数据库
- 在生产环境禁用敏感数据日志

---

## 📚 相关文档

### 同级文档
- [5.1 概念模型（ER 图）](概念模型_ER图.md)
- [5.2 表结构定义](表结构定义.md)
- [5.3 关键表说明](关键表说明.md)
- [5.4 索引与优化](索引与优化.md)
- [5.5 数据迁移方案](数据迁移方案.md)

### 返回上级
- [🔙 数据库设计总览](README.md)
- [🏠 项目文档首页](../自助台球系统项目文档.md)

### 相关章节
- [3. 系统架构设计](../03_系统架构设计/README.md)
- [6. 开发规范](../06_开发规范/README.md)
- [8. 配置管理](../08_配置管理/README.md)
- [10. 部署与运维](../10_部署与运维/README.md)