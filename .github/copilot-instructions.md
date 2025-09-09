# GitHub Copilot Instructions - 智慧台球厅管理系统

## 项目概述 (Project Overview)

这是一个智慧台球厅管理系统，采用机器可读优先、人机混合协作、流程自动化的开发模式。本文档为 GitHub Copilot 提供全面的项目指导，确保代码生成符合项目架构和业务需求。

### 核心原则 (Core Principles)

1. **机器可读优先 (Machine-Readable First)**
   - 使用结构化数据格式 (JSON, YAML)
   - 遵循严格的命名约定和代码模式
   - 提供清晰的类型定义和接口规范

2. **人机混合协作 (Human-AI Collaboration)**
   - 提供上下文感知的代码建议
   - 支持渐进式功能增强
   - 保持代码可读性和维护性

3. **流程自动化 (Process Automation)**
   - 自动化测试和部署流程
   - 代码质量检查和修复
   - 文档自动生成和更新

## 技术栈 (Technology Stack)

```yaml
# 实际项目技术栈配置
backend:
  framework: "ABP Framework 9.3.2"
  base_framework: "ASP.NET Core 9.0"
  database: "MySQL"
  orm: "Entity Framework Core"
  api: "ABP Application Services + RESTful API"
  architecture: "Domain Driven Design (DDD)"
  
frontend:
  framework: "Blazor Server + Blazor WebAssembly"
  ui_library: "Blazorise + Bootstrap 5"
  icons: "FontAwesome"
  theme: "LeptonX Lite"
  
orchestration:
  platform: ".NET Aspire 9.4.1"
  service_discovery: "Microsoft.Extensions.ServiceDiscovery"
  resilience: "Microsoft.Extensions.Http.Resilience"
  
infrastructure:
  caching: "Redis"
  monitoring: "OpenTelemetry"
  health_checks: "ASP.NET Core Health Checks"
  containerization: "Docker (via Aspire)"
  logging: "Serilog"
  
abp_modules:
  - "Identity Management"
  - "Permission Management"
  - "Tenant Management"
  - "Audit Logging"
  - "Feature Management"
  - "Setting Management"
  - "Background Jobs"
  - "Blob Storage"
```

## 业务领域模型 (Business Domain Model)

### ABP 领域驱动设计架构

```csharp
// ABP 实体基类继承结构
// 台球厅核心业务实体 - 使用 ABP Entity 基类
public class BilliardHall : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    
    // 导航属性
    public virtual ICollection<BilliardTable> Tables { get; set; }
    public virtual ICollection<PrivateRoom> Rooms { get; set; }
}

public class BilliardTable : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public int Number { get; set; }
    public BilliardTableType Type { get; set; }
    public BilliardTableStatus Status { get; set; }
    public decimal HourlyRate { get; set; }
    public float LocationX { get; set; }
    public float LocationY { get; set; }
    
    // 外键
    public Guid BilliardHallId { get; set; }
    public virtual BilliardHall BilliardHall { get; set; }
}

public class Reservation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid BilliardTableId { get; set; }
    public DateTime StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalAmount { get; set; }
    public ReservationStatus Status { get; set; }
    
    // 导航属性
    public virtual IdentityUser Customer { get; set; }
    public virtual BilliardTable BilliardTable { get; set; }
}
```

### 枚举定义 (Enumerations)

```csharp
public enum BilliardTableType
{
    ChineseEightBall = 1,     // 中式黑八
    AmericanNineBall = 2,     // 美式九球
    Snooker = 3,              // 斯诺克
    EnglishPool = 4           // 英式台球
}

public enum BilliardTableStatus
{
    Available = 1,            // 空闲
    Occupied = 2,             // 占用中
    Reserved = 3,             // 已预约
    Maintenance = 4,          // 维护中
    OutOfOrder = 5            // 故障
}

public enum ReservationStatus
{
    Pending = 1,              // 待确认
    Confirmed = 2,            // 已确认
    InProgress = 3,           // 进行中
    Completed = 4,            // 已完成
    Cancelled = 5             // 已取消
}
```

## 代码生成指南 (Code Generation Guidelines)

### 1. ABP Application Service 开发模式

当生成 ABP Application Service 相关代码时：

```csharp
// Application Service 接口定义
public interface IBilliardTableAppService : IApplicationService
{
    Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input);
    Task<BilliardTableDto> GetAsync(Guid id);
    Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input);
    Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input);
    Task DeleteAsync(Guid id);
    Task<BilliardTableDto> ChangeStatusAsync(Guid id, ChangeBilliardTableStatusDto input);
}

// Application Service 实现
[Authorize(BilliardHallPermissions.BilliardTables.Default)]
public class BilliardTableAppService : BilliardHallAppService, IBilliardTableAppService
{
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;
    private readonly BilliardTableManager _billiardTableManager;

    public BilliardTableAppService(
        IRepository<BilliardTable, Guid> billiardTableRepository,
        BilliardTableManager billiardTableManager)
    {
        _billiardTableRepository = billiardTableRepository;
        _billiardTableManager = billiardTableManager;
    }

    public virtual async Task<PagedResultDto<BilliardTableDto>> GetListAsync(GetBilliardTablesInput input)
    {
        var queryable = await _billiardTableRepository.GetQueryableAsync();
        
        queryable = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Number.ToString().Contains(input.Filter))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        
        queryable = queryable.OrderBy(input.Sorting ?? nameof(BilliardTable.Number))
                            .PageBy(input.SkipCount, input.MaxResultCount);

        var tables = await AsyncExecuter.ToListAsync(queryable);
        
        return new PagedResultDto<BilliardTableDto>(
            totalCount,
            await MapToGetListOutputDtosAsync(tables)
        );
    }

    [Authorize(BilliardHallPermissions.BilliardTables.Create)]
    public virtual async Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input)
    {
        var table = await _billiardTableManager.CreateAsync(
            input.Number,
            input.Type,
            input.HourlyRate,
            input.LocationX,
            input.LocationY
        );

        await _billiardTableRepository.InsertAsync(table);
        
        return await MapToGetOutputDtoAsync(table);
    }
}

// DTO 定义
public class BilliardTableDto : FullAuditedEntityDto<Guid>
{
    public int Number { get; set; }
    public BilliardTableType Type { get; set; }
    public BilliardTableStatus Status { get; set; }
    public decimal HourlyRate { get; set; }
    public float LocationX { get; set; }
    public float LocationY { get; set; }
}

public class CreateBilliardTableDto
{
    [Required]
    [Range(1, 999)]
    public int Number { get; set; }
    
    [Required]
    public BilliardTableType Type { get; set; }
    
    [Required]
    [Range(0.01, 9999.99)]
    public decimal HourlyRate { get; set; }
    
    public float LocationX { get; set; }
    public float LocationY { get; set; }
}

// Domain Manager (领域服务)
public class BilliardTableManager : DomainService
{
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;

    public BilliardTableManager(IRepository<BilliardTable, Guid> billiardTableRepository)
    {
        _billiardTableRepository = billiardTableRepository;
    }

    public async Task<BilliardTable> CreateAsync(
        int number,
        BilliardTableType type,
        decimal hourlyRate,
        float locationX,
        float locationY)
    {
        await CheckNumberNotExistsAsync(number);

        return new BilliardTable(
            GuidGenerator.Create(),
            number,
            type,
            hourlyRate,
            locationX,
            locationY
        );
    }

    private async Task CheckNumberNotExistsAsync(int number)
    {
        var exists = await _billiardTableRepository.AnyAsync(x => x.Number == number);
        if (exists)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.BilliardTableNumberAlreadyExists)
                .WithData("Number", number);
        }
    }
}
```

### 2. ABP Entity Framework Core + MySQL 设计模式

```csharp
// DbContext 配置
public class BilliardHallDbContext : AbpDbContext<BilliardHallDbContext>
{
    public DbSet<BilliardHall> BilliardHalls { get; set; }
    public DbSet<BilliardTable> BilliardTables { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    public BilliardHallDbContext(DbContextOptions<BilliardHallDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureBilliardHall();
    }
}

// Entity Configuration
public static class BilliardHallDbContextModelCreatingExtensions
{
    public static void ConfigureBilliardHall(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // BilliardHall 表配置
        builder.Entity<BilliardHall>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardHalls", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(BilliardHallConsts.MaxNameLength);
            b.Property(x => x.Address).IsRequired().HasMaxLength(BilliardHallConsts.MaxAddressLength);
            b.Property(x => x.Phone).HasMaxLength(BilliardHallConsts.MaxPhoneLength);

            // MySQL 特定配置
            b.Property(x => x.OpenTime).HasColumnType("time");
            b.Property(x => x.CloseTime).HasColumnType("time");

            // 索引
            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.TenantId);
        });

        // BilliardTable 表配置
        builder.Entity<BilliardTable>(b =>
        {
            b.ToTable(BilliardHallConsts.DbTablePrefix + "BilliardTables", BilliardHallConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Number).IsRequired();
            b.Property(x => x.Type).IsRequired().HasConversion<int>();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(BilliardTableStatus.Available);
            b.Property(x => x.HourlyRate).IsRequired().HasPrecision(10, 2);

            // MySQL 空间数据支持
            b.Property(x => x.LocationX).HasColumnType("float");
            b.Property(x => x.LocationY).HasColumnType("float");

            // 外键关系
            b.HasOne(x => x.BilliardHall)
             .WithMany(x => x.Tables)
             .HasForeignKey(x => x.BilliardHallId)
             .OnDelete(DeleteBehavior.Cascade);

            // 唯一约束
            b.HasIndex(x => new { x.BilliardHallId, x.Number }).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.Type);
            b.HasIndex(x => x.TenantId);
        });
    }
}

// Migration 示例
[Migration("20240909_AddBilliardTables")]
public partial class AddBilliardTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppBilliardTables",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                TenantId = table.Column<Guid>(type: "char(36)", nullable: true),
                Number = table.Column<int>(type: "int", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                HourlyRate = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                LocationX = table.Column<float>(type: "float", nullable: false),
                LocationY = table.Column<float>(type: "float", nullable: false),
                BilliardHallId = table.Column<Guid>(type: "char(36)", nullable: false),
                ExtraProperties = table.Column<string>(type: "longtext", nullable: false),
                ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                CreatorId = table.Column<Guid>(type: "char(36)", nullable: true),
                LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                LastModifierId = table.Column<Guid>(type: "char(36)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                DeleterId = table.Column<Guid>(type: "char(36)", nullable: true),
                DeletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppBilliardTables", x => x.Id);
                table.ForeignKey(
                    name: "FK_AppBilliardTables_AppBilliardHalls_BilliardHallId",
                    column: x => x.BilliardHallId,
                    principalTable: "AppBilliardHalls",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // 创建索引
        migrationBuilder.CreateIndex(
            name: "IX_AppBilliardTables_BilliardHallId_Number",
            table: "AppBilliardTables",
            columns: new[] { "BilliardHallId", "Number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AppBilliardTables_Status",
            table: "AppBilliardTables",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_AppBilliardTables_Type",
            table: "AppBilliardTables",
            column: "Type");
    }
}
```

### 3. Blazor 组件开发模式

```razor
@* BilliardTableCard.razor - Blazor Server 组件 *@
@using Zss.BilliardHall.BilliardTables
@inherits BilliardHallComponentBase
@inject IBilliardTableAppService BilliardTableAppService
@inject IMessageService MessageService

<Card>
    <CardHeader>
        <CardTitle>
            台球桌 #@BilliardTable.Number
            <Badge Color="@GetStatusColor(BilliardTable.Status)" class="float-end">
                @L[$"Enum:BilliardTableStatus.{BilliardTable.Status}"]
            </Badge>
        </CardTitle>
    </CardHeader>
    <CardBody>
        <CardText>
            <div class="mb-2">
                <Icon Name="IconName.Table" class="me-1" />
                类型: @L[$"Enum:BilliardTableType.{BilliardTable.Type}"]
            </div>
            <div class="mb-2">
                <Icon Name="IconName.Money" class="me-1" />
                价格: ¥@BilliardTable.HourlyRate.ToString("F2")/小时
            </div>
        </CardText>
    </CardBody>
    <CardActions>
        @if (BilliardTable.Status == BilliardTableStatus.Available)
        {
            <Button Color="Color.Primary" 
                    Clicked="@(() => OnReserveClicked.InvokeAsync(BilliardTable.Id))"
                    Loading="@IsReserving">
                <Icon Name="IconName.Calendar" class="me-1" />
                预约
            </Button>
        }
        @if (HasPermission(BilliardHallPermissions.BilliardTables.Edit))
        {
            <Dropdown>
                <DropdownToggle Color="Color.Secondary" Size="Size.Small">
                    <Icon Name="IconName.Settings" />
                </DropdownToggle>
                <DropdownMenu>
                    <DropdownItem Clicked="@(() => OnEditClicked.InvokeAsync(BilliardTable.Id))">
                        <Icon Name="IconName.Edit" class="me-1" />
                        编辑
                    </DropdownItem>
                    <DropdownDivider />
                    <DropdownItem Clicked="@(() => ChangeStatusAsync(BilliardTableStatus.Maintenance))">
                        <Icon Name="IconName.Wrench" class="me-1" />
                        设为维护
                    </DropdownItem>
                </DropdownMenu>
            </Dropdown>
        }
    </CardActions>
</Card>

@code {
    [Parameter] public BilliardTableDto BilliardTable { get; set; } = new();
    [Parameter] public EventCallback<Guid> OnReserveClicked { get; set; }
    [Parameter] public EventCallback<Guid> OnEditClicked { get; set; }

    private bool IsReserving { get; set; }

    private Color GetStatusColor(BilliardTableStatus status)
    {
        return status switch
        {
            BilliardTableStatus.Available => Color.Success,
            BilliardTableStatus.Occupied => Color.Danger,
            BilliardTableStatus.Reserved => Color.Warning,
            BilliardTableStatus.Maintenance => Color.Secondary,
            BilliardTableStatus.OutOfOrder => Color.Dark,
            _ => Color.Light
        };
    }

    private async Task ChangeStatusAsync(BilliardTableStatus newStatus)
    {
        try
        {
            var input = new ChangeBilliardTableStatusDto { Status = newStatus };
            await BilliardTableAppService.ChangeStatusAsync(BilliardTable.Id, input);
            
            await MessageService.Success(L["StatusChangedSuccessfully"]);
            
            // 刷新父组件
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }
}
```

```csharp
// BilliardTableList.razor.cs - 页面代码后置
public partial class BilliardTableList
{
    [Inject] protected IBilliardTableAppService BilliardTableAppService { get; set; }
    [Inject] protected IDataGridEntityActionsColumn<BilliardTableDto> EntityActionsColumn { get; set; }

    protected IReadOnlyList<BilliardTableDto> BilliardTableList { get; set; } = new List<BilliardTableDto>();
    protected int PageSize { get; } = LimitedResultRequestDto.DefaultMaxResultCount;
    protected int CurrentPage { get; set; } = 1;
    protected string CurrentSorting { get; set; } = string.Empty;
    protected int TotalCount { get; set; }
    protected bool ShowAdvancedFilters { get; set; }
    protected GetBilliardTablesInput Filter { get; set; }
    
    // Modals
    protected CreateBilliardTableModal CreateBilliardTableModal { get; set; } = new();
    protected UpdateBilliardTableModal UpdateBilliardTableModal { get; set; } = new();

    public BilliardTableList()
    {
        Filter = new GetBilliardTablesInput
        {
            MaxResultCount = PageSize,
            SkipCount = (CurrentPage - 1) * PageSize,
            Sorting = CurrentSorting
        };
        
        LocalizationResource = typeof(BilliardHallResource);
    }

    protected override async Task OnInitializedAsync()
    {
        await GetBilliardTablesAsync();
    }

    protected virtual async Task GetBilliardTablesAsync()
    {
        Filter.MaxResultCount = PageSize;
        Filter.SkipCount = (CurrentPage - 1) * PageSize;
        Filter.Sorting = CurrentSorting;

        var result = await BilliardTableAppService.GetListAsync(Filter);
        BilliardTableList = result.Items;
        TotalCount = (int)result.TotalCount;
    }

    protected virtual async Task OnDataGridReadAsync(DataGridReadDataEventArgs<BilliardTableDto> e)
    {
        CurrentSorting = e.Columns
            .Where(c => c.SortDirection != SortDirection.Default)
            .Select(c => c.Field + (c.SortDirection == SortDirection.Descending ? " DESC" : ""))
            .JoinAsString(",");
        CurrentPage = e.Page;

        await GetBilliardTablesAsync();
        await InvokeAsync(StateHasChanged);
    }
}
```

## 测试策略 (Testing Strategy)

### ABP 测试基础设施

```csharp
// Domain 层测试基类
public abstract class BilliardHallDomainTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }
}

// Application 层测试基类  
public abstract class BilliardHallApplicationTestBase<TStartupModule> : BilliardHallDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected IBilliardTableAppService BilliardTableAppService { get; }
    protected IRepository<BilliardTable, Guid> BilliardTableRepository { get; }

    protected BilliardHallApplicationTestBase()
    {
        BilliardTableAppService = GetRequiredService<IBilliardTableAppService>();
        BilliardTableRepository = GetRequiredService<IRepository<BilliardTable, Guid>>();
    }
}

// Application Service 测试示例
public class BilliardTableAppService_Tests : BilliardHallApplicationTestBase<BilliardHallApplicationTestModule>
{
    private readonly IBilliardTableAppService _billiardTableAppService;
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;

    public BilliardTableAppService_Tests()
    {
        _billiardTableAppService = GetRequiredService<IBilliardTableAppService>();
        _billiardTableRepository = GetRequiredService<IRepository<BilliardTable, Guid>>();
    }

    [Fact]
    public async Task Should_Create_BilliardTable_With_Valid_Input()
    {
        // Arrange
        var input = new CreateBilliardTableDto
        {
            Number = 1,
            Type = BilliardTableType.ChineseEightBall,
            HourlyRate = 50.00m,
            LocationX = 100.0f,
            LocationY = 200.0f
        };

        // Act
        var result = await _billiardTableAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Number.ShouldBe(input.Number);
        result.Type.ShouldBe(input.Type);
        result.HourlyRate.ShouldBe(input.HourlyRate);
        result.Status.ShouldBe(BilliardTableStatus.Available);

        // 验证数据库中的数据
        var tableInDb = await _billiardTableRepository.GetAsync(result.Id);
        tableInDb.ShouldNotBeNull();
        tableInDb.Number.ShouldBe(input.Number);
    }

    [Fact]
    public async Task Should_Not_Create_BilliardTable_With_Duplicate_Number()
    {
        // Arrange
        await _billiardTableRepository.InsertAsync(new BilliardTable(
            Guid.NewGuid(),
            1,
            BilliardTableType.ChineseEightBall,
            50.00m,
            100.0f,
            200.0f
        ));

        var input = new CreateBilliardTableDto
        {
            Number = 1, // 重复号码
            Type = BilliardTableType.AmericanNineBall,
            HourlyRate = 60.00m,
            LocationX = 150.0f,
            LocationY = 250.0f
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(
            () => _billiardTableAppService.CreateAsync(input)
        );
        
        exception.Code.ShouldBe(BilliardHallDomainErrorCodes.BilliardTableNumberAlreadyExists);
    }

    [Fact]
    public async Task Should_Get_Paged_List_With_Filtering()
    {
        // Arrange - 创建测试数据
        var tables = new[]
        {
            new BilliardTable(Guid.NewGuid(), 1, BilliardTableType.ChineseEightBall, 50.00m, 0, 0),
            new BilliardTable(Guid.NewGuid(), 2, BilliardTableType.AmericanNineBall, 60.00m, 0, 0),
            new BilliardTable(Guid.NewGuid(), 3, BilliardTableType.Snooker, 70.00m, 0, 0)
        };

        foreach (var table in tables)
        {
            await _billiardTableRepository.InsertAsync(table);
        }

        var input = new GetBilliardTablesInput
        {
            Type = BilliardTableType.ChineseEightBall,
            MaxResultCount = 10,
            SkipCount = 0
        };

        // Act
        var result = await _billiardTableAppService.GetListAsync(input);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items.First().Type.ShouldBe(BilliardTableType.ChineseEightBall);
    }
}

// Domain Service 测试
public class BilliardTableManager_Tests : BilliardHallDomainTestBase<BilliardHallDomainTestModule>
{
    private readonly BilliardTableManager _billiardTableManager;
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;

    public BilliardTableManager_Tests()
    {
        _billiardTableManager = GetRequiredService<BilliardTableManager>();
        _billiardTableRepository = GetRequiredService<IRepository<BilliardTable, Guid>>();
    }

    [Fact]
    public async Task Should_Create_BilliardTable()
    {
        // Act
        var table = await _billiardTableManager.CreateAsync(
            1,
            BilliardTableType.ChineseEightBall,
            50.00m,
            100.0f,
            200.0f
        );

        // Assert
        table.ShouldNotBeNull();
        table.Number.ShouldBe(1);
        table.Type.ShouldBe(BilliardTableType.ChineseEightBall);
        table.Status.ShouldBe(BilliardTableStatus.Available);
    }
}

// Entity 测试
public class BilliardTable_Tests : BilliardHallDomainTestBase<BilliardHallDomainTestModule>
{
    [Fact]
    public void Should_Create_BilliardTable_With_Valid_Data()
    {
        // Act
        var table = new BilliardTable(
            Guid.NewGuid(),
            1,
            BilliardTableType.ChineseEightBall,
            50.00m,
            100.0f,
            200.0f
        );

        // Assert
        table.Number.ShouldBe(1);
        table.Type.ShouldBe(BilliardTableType.ChineseEightBall);
        table.Status.ShouldBe(BilliardTableStatus.Available);
        table.HourlyRate.ShouldBe(50.00m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Not_Create_BilliardTable_With_Invalid_Number(int invalidNumber)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new BilliardTable(
            Guid.NewGuid(),
            invalidNumber,
            BilliardTableType.ChineseEightBall,
            50.00m,
            100.0f,
            200.0f
        ));
    }
}
```

## 错误处理模式 (Error Handling Patterns)

```csharp
// 统一异常处理
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(context, ex);
        }
    }
}
```

## 性能优化指南 (Performance Guidelines)

1. **数据库查询优化**
   - 使用适当的索引
   - 避免 N+1 查询问题
   - 实施分页和排序

2. **缓存策略**
   - Redis 用于热点数据缓存
   - 内存缓存用于静态配置
   - CDN 用于静态资源

3. **API 设计**
   - 实施 API 版本控制
   - 使用压缩响应
   - 实现请求限流

## ABP 权限和安全模式 (ABP Permissions and Security Patterns)

```csharp
// 权限定义
public static class BilliardHallPermissions
{
    public const string GroupName = "BilliardHall";

    public static class BilliardTables
    {
        public const string Default = GroupName + ".BilliardTables";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageStatus = Default + ".ManageStatus";
    }

    public static class Reservations
    {
        public const string Default = GroupName + ".Reservations";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
    }
}

// 权限定义提供者
public class BilliardHallPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var billiardHallGroup = context.AddGroup(BilliardHallPermissions.GroupName, L("Permission:BilliardHall"));

        var billiardTablesPermission = billiardHallGroup.AddPermission(
            BilliardHallPermissions.BilliardTables.Default, L("Permission:BilliardTables"));
        billiardTablesPermission.AddChild(BilliardHallPermissions.BilliardTables.Create, L("Permission:Create"));
        billiardTablesPermission.AddChild(BilliardHallPermissions.BilliardTables.Edit, L("Permission:Edit"));
        billiardTablesPermission.AddChild(BilliardHallPermissions.BilliardTables.Delete, L("Permission:Delete"));
        billiardTablesPermission.AddChild(BilliardHallPermissions.BilliardTables.ManageStatus, L("Permission:ManageStatus"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BilliardHallResource>(name);
    }
}

// 多租户权限策略
[Authorize(BilliardHallPermissions.BilliardTables.Default)]
[RemoteService(IsEnabled = false)]
public class BilliardTableAppService : BilliardHallAppService, IBilliardTableAppService
{
    // 自动应用多租户过滤
    // ABP 自动处理 TenantId 过滤，无需手动添加条件
}

// JWT 配置 (在 Program.cs 中)
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // ABP 会自动配置 JWT 认证
        builder.Host.AddAppSettingsSecretsJson()
               .UseAutofac();
               
        await builder.AddApplicationAsync<BilliardHallBlazorModule>();
        
        var app = builder.Build();
        
        // ABP 自动配置安全中间件
        await app.InitializeApplicationAsync();
        await app.RunAsync();
        
        return 0;
    }
}
```

## .NET Aspire 编排和部署 (Aspire Orchestration & Deployment)

```csharp
// AppHost.cs - Aspire 应用编排
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// 基础设施服务
var redis = builder
    .AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume(); // 持久化数据

var mysql = builder
    .AddMySql("mysql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("MYSQL_ROOT_PASSWORD", "mypassword123")
    .WithEnvironment("MYSQL_DATABASE", "BilliardHall");

var db = mysql.AddDatabase("db");

// 应用服务编排
var blazorApp = builder
    .AddProject<Zss_BilliardHall_Blazor>("zss-billiardhall-blazor")
    .WithReference(redis, "Redis")      // Redis 连接
    .WithReference(db, "Default")       // 数据库连接
    .WaitFor(redis)                     // 等待 Redis 启动
    .WaitFor(db)                        // 等待数据库启动
    .WithReplicas(2);                   // 横向扩展

// 数据库迁移服务
var migrator = builder
    .AddProject<Zss_BilliardHall_DbMigrator>("zss-billiardhall-dbmigrator")
    .WithReference(db, "Default")
    .WaitFor(db);

// 开发环境配置
if (builder.Environment.IsDevelopment())
{
    // 添加开发工具
    mysql.WithPgAdmin();                // MySQL 管理工具
    redis.WithRedisCommander();         // Redis 管理工具
}

builder.Build().Run();
```

```csharp
// ServiceDefaults/Extensions.cs - 服务默认配置
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        // OpenTelemetry 配置
        builder.ConfigureOpenTelemetry();

        // 健康检查
        builder.AddDefaultHealthChecks();

        // 服务发现
        builder.Services.AddServiceDiscovery();

        // HTTP 客户端默认配置
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // 启用弹性处理
            http.AddStandardResilienceHandler();
            
            // 启用服务发现
            http.AddServiceDiscovery();
        });

        return builder;
    }

    private static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation()
                       .AddEntityFrameworkCoreInstrumentation(); // EF Core 追踪
            });

        return builder;
    }
}
```

```yaml
# Docker Compose 部署配置（Aspire 生成）
version: '3.8'

services:
  zss-billiardhall-blazor:
    image: zss-billiardhall-blazor:latest
    build:
      context: .
      dockerfile: src/Zss.BilliardHall.Blazor/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Server=mysql;Database=BilliardHall;Uid=root;Pwd=mypassword123
      - ConnectionStrings__Redis=redis:6379
      - OTEL_EXPORTER_OTLP_ENDPOINT=http://jaeger:4317
    depends_on:
      - mysql
      - redis
      - jaeger
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '1.0'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M

  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=mypassword123
      - MYSQL_DATABASE=BilliardHall
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"  # Jaeger UI
      - "4317:4317"    # OTLP gRPC receiver
    environment:
      - COLLECTOR_OTLP_ENABLED=true

volumes:
  mysql_data:
  redis_data:
```

```bash
# 本地开发命令
# 启动 Aspire 应用编排
dotnet run --project src/Zss.BilliardHall.AppHost

# 构建所有项目
dotnet build

# 运行数据库迁移
dotnet run --project src/Zss.BilliardHall.DbMigrator

# 运行测试
dotnet test

# 生成 Docker 镜像
dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer

# 监控和日志
# Aspire Dashboard: https://localhost:15888
# Jaeger UI: http://localhost:16686
# Application Logs: 通过 Aspire Dashboard 查看
```

## 相关指令文件 (Related Instruction Files)

- [ABP 代码模式和约定](./.copilot/patterns/coding-patterns.md) - ABP Framework 编码规范
- [ABP Application Service 设计规范](./.copilot/patterns/api-patterns.md) - Application Service 开发模式
- [ABP + MySQL 数据库设计规范](./.copilot/patterns/database-patterns.md) - EF Core + MySQL 设计模式
- [ABP 测试指导](./.copilot/patterns/testing-patterns.md) - ABP 测试基础设施使用
- [Blazor 组件开发模式](./.copilot/patterns/blazor-patterns.md) - Blazor Server + WASM 开发
- [Aspire 编排模式](./.copilot/patterns/aspire-patterns.md) - .NET Aspire 服务编排
- [ABP 工作流模板](./.copilot/workflows/README.md) - 开发工作流程
- [ABP 实体架构](./.copilot/schemas/abp-entities.json) - ABP 实体定义架构
- [Aspire 配置架构](./.copilot/schemas/aspire-config.json) - Aspire 编排配置架构

---

> 此文档基于实际项目架构（ABP Framework 9.3.2 + .NET Aspire 9.4.1 + Blazor + MySQL）更新。所有代码生成都应遵循 ABP 框架约定和 Aspire 编排模式，确保一致性和质量。