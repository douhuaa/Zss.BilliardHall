# ABP Framework 编码模式和约定 (ABP Framework Coding Patterns and Conventions)

## 总体原则 (General Principles)

### 1. ABP 框架优先 (ABP Framework First)
- 遵循 ABP 框架的 DDD 架构模式
- 使用 ABP 提供的基类和服务
- 遵循 ABP 的命名约定和项目结构
- 利用 ABP 的内置功能（权限、多租户、本地化等）

### 2. 领域驱动设计 (Domain Driven Design)
- 明确区分领域层、应用层、基础设施层
- 实体、聚合根、领域服务的正确使用
- 遵循 DDD 战术模式
- 保持业务逻辑在领域层

### 3. .NET Aspire 集成 (Aspire Integration)
- 使用 Aspire 进行服务编排和配置
- 集成 OpenTelemetry 进行监控和追踪
- 利用 Aspire 的健康检查和服务发现
- 遵循云原生开发实践

## ABP C# 编码规范

### 命名约定 (Naming Conventions)

```csharp
// 实体类：PascalCase，继承 ABP 基类
public class BilliardTable : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    // 属性使用 PascalCase
    public string Name { get; set; }
}

// Application Service 接口：I + PascalCase + AppService
public interface IBilliardTableAppService : IApplicationService
{
    // 方法名：PascalCase + Async 后缀
    Task<BilliardTableDto> GetAsync(Guid id);
    Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input);
}

// Application Service 实现：PascalCase + AppService
public class BilliardTableAppService : BilliardHallAppService, IBilliardTableAppService
{
    // 私有字段：_camelCase
    private readonly IRepository<BilliardTable, Guid> _billiardTableRepository;
    private readonly BilliardTableManager _billiardTableManager;
}

// DTO 类：PascalCase + Dto 后缀
public class BilliardTableDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; }
}

// 领域服务：PascalCase + Manager 后缀
public class BilliardTableManager : DomainService
{
    // 方法名：PascalCase + Async 后缀
    public async Task<BilliardTable> CreateAsync(string name)
}

// 权限常量：UPPER_SNAKE_CASE 或 PascalCase
public static class BilliardHallPermissions
{
    public const string GroupName = "BilliardHall";
    
    public static class BilliardTables
    {
        public const string Default = GroupName + ".BilliardTables";
        public const string Create = Default + ".Create";
    }
}

// 枚举：PascalCase
public enum BilliardTableStatus
{
    Available = 1,
    Occupied = 2,
    Reserved = 3
}
```
    Reserved,
### ABP 项目结构约定 (ABP Project Structure Conventions)

```
Zss.BilliardHall.Domain/
├── Entities/                    # 实体
│   ├── BilliardTable.cs
│   └── Reservation.cs
├── Managers/                    # 领域服务
│   └── BilliardTableManager.cs
├── Repositories/                # 仓储接口
│   └── IBilliardTableRepository.cs
├── Shared/                      # 共享常量和枚举
│   ├── BilliardTableConsts.cs
│   └── BilliardHallDomainErrorCodes.cs
└── BilliardHallDomainModule.cs

Zss.BilliardHall.Application.Contracts/
├── BilliardTables/
│   ├── IBilliardTableAppService.cs
│   ├── BilliardTableDto.cs
│   ├── CreateBilliardTableDto.cs
│   └── GetBilliardTablesInput.cs
├── Permissions/
│   └── BilliardHallPermissions.cs
└── BilliardHallApplicationContractsModule.cs

Zss.BilliardHall.Application/
├── BilliardTables/
│   └── BilliardTableAppService.cs
├── BilliardHallAppService.cs     # 基类
├── BilliardHallAutoMapperProfile.cs
└── BilliardHallApplicationModule.cs

Zss.BilliardHall.EntityFrameworkCore/
├── EntityFrameworkCore/
│   ├── BilliardHallDbContext.cs
│   ├── BilliardHallDbContextModelCreatingExtensions.cs
│   └── IBilliardHallDbContext.cs
├── Repositories/
│   └── BilliardTableRepository.cs (可选)
└── BilliardHallEntityFrameworkCoreModule.cs
```

### ABP 实体开发模式 (ABP Entity Patterns)

```csharp
// 聚合根实体
public class BilliardHall : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    [Required]
    [StringLength(BilliardHallConsts.MaxNameLength)]
    public string Name { get; set; }
    
    [StringLength(BilliardHallConsts.MaxAddressLength)]
    public string Address { get; set; }
    
    // 值对象
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    
    // 导航属性（集合）
    public virtual ICollection<BilliardTable> Tables { get; set; }
    
    // 私有构造函数（用于 EF Core）
    protected BilliardHall() { }
    
    // 公共构造函数（用于领域层）
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
        CloseTime = closeTime ?? TimeSpan.FromHours(22);
        Tables = new List<BilliardTable>();
    }
    
    // 业务方法
    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name), BilliardHallConsts.MaxNameLength);
        Name = name;
    }
    
    public BilliardTable AddTable(int number, BilliardTableType type, decimal hourlyRate)
    {
        Check.Condition(number > 0, nameof(number), "Table number must be positive");
        
        if (Tables.Any(t => t.Number == number))
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.TableNumberAlreadyExists)
                .WithData("Number", number);
        }
        
        var table = new BilliardTable(
            GuidGenerator.Create(),
            number,
            type,
            hourlyRate
        );
        
        Tables.Add(table);
        return table;
    }
}

// 子实体
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
    
    protected BilliardTable() { }
    
    internal BilliardTable(
        Guid id,
        int number,
        BilliardTableType type,
        decimal hourlyRate,
        float locationX = 0,
        float locationY = 0) : base(id)
    {
        Check.Condition(number > 0, nameof(number));
        Check.Condition(hourlyRate > 0, nameof(hourlyRate));
        
        Number = number;
        Type = type;
        Status = BilliardTableStatus.Available;
        HourlyRate = hourlyRate;
        LocationX = locationX;
        LocationY = locationY;
    }
    
    public void ChangeStatus(BilliardTableStatus newStatus)
    {
        // 业务规则验证
        if (Status == BilliardTableStatus.OutOfOrder && newStatus != BilliardTableStatus.Maintenance)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.CannotChangeStatusFromOutOfOrder);
        }
        
        Status = newStatus;
    }
}
```

### ABP Application Service 开发模式

```csharp
// Application Service 基类
public abstract class BilliardHallAppService : ApplicationService
{
    protected BilliardHallAppService()
    {
        LocalizationResource = typeof(BilliardHallResource);
    }
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
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), 
                     x => x.Number.ToString().Contains(input.Filter))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type);

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        
        queryable = queryable
            .OrderBy(input.Sorting ?? nameof(BilliardTable.Number))
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

    [Authorize(BilliardHallPermissions.BilliardTables.Edit)]
    public virtual async Task<BilliardTableDto> UpdateAsync(Guid id, UpdateBilliardTableDto input)
    {
        var table = await _billiardTableRepository.GetAsync(id);
        
        // 使用 ABP 的 ObjectMapper 自动映射
        await MapToEntityAsync(input, table);
        
        table = await _billiardTableRepository.UpdateAsync(table);
        
        return await MapToGetOutputDtoAsync(table);
    }
}
```

### ABP 领域服务模式 (Domain Service Pattern)

```csharp
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
        float locationX = 0,
        float locationY = 0)
    {
        // 业务规则验证
        await CheckNumberNotExistsAsync(number);
        await CheckHourlyRateValidAsync(hourlyRate);

        return new BilliardTable(
            GuidGenerator.Create(),
            number,
            type,
            hourlyRate,
            locationX,
            locationY
        );
    }

    public async Task ChangeStatusAsync(BilliardTable table, BilliardTableStatus newStatus)
    {
        Check.NotNull(table, nameof(table));
        
        // 复杂业务规则
        await ValidateStatusChangeAsync(table, newStatus);
        
        table.ChangeStatus(newStatus);
        
        // 领域事件
        table.AddLocalEvent(new BilliardTableStatusChangedEto
        {
            TableId = table.Id,
            OldStatus = table.Status,
            NewStatus = newStatus
        });
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

    private async Task CheckHourlyRateValidAsync(decimal hourlyRate)
    {
        if (hourlyRate <= 0 || hourlyRate > 999.99m)
        {
            throw new BusinessException(BilliardHallDomainErrorCodes.InvalidHourlyRate)
                .WithData("Rate", hourlyRate);
        }
    }
}
```

### ABP 错误处理模式 (Error Handling Pattern)

```csharp
// 领域错误代码
public static class BilliardHallDomainErrorCodes
{
    public const string BilliardTableNumberAlreadyExists = "BilliardHall:BilliardTableNumberAlreadyExists";
    public const string InvalidHourlyRate = "BilliardHall:InvalidHourlyRate";
    public const string CannotChangeStatusFromOutOfOrder = "BilliardHall:CannotChangeStatusFromOutOfOrder";
}

// 本地化资源
{
  "BilliardHall:BilliardTableNumberAlreadyExists": "台球桌号码 {Number} 已存在",
  "BilliardHall:InvalidHourlyRate": "无效的小时费率: {Rate}",
  "BilliardHall:CannotChangeStatusFromOutOfOrder": "无法从故障状态直接更改到其他状态"
}

// 异常处理
public class BilliardTableAppService : BilliardHallAppService
{
    public async Task<BilliardTableDto> CreateAsync(CreateBilliardTableDto input)
    {
        try
        {
            // 业务逻辑
            var table = await _billiardTableManager.CreateAsync(...);
            return await MapToGetOutputDtoAsync(table);
        }
        catch (BusinessException ex) when (ex.Code == BilliardHallDomainErrorCodes.BilliardTableNumberAlreadyExists)
        {
            // ABP 自动处理本地化和返回适当的 HTTP 状态码
            throw;
        }
    }
}
```

### ABP 自动映射配置 (AutoMapper Profile)

```csharp
public class BilliardHallAutoMapperProfile : Profile
{
    public BilliardHallAutoMapperProfile()
    {
        // Entity -> DTO
        CreateMap<BilliardTable, BilliardTableDto>();
        CreateMap<BilliardHall, BilliardHallDto>();
        
        // Input DTO -> Entity (Create)
        CreateMap<CreateBilliardTableDto, BilliardTable>()
            .IgnoreFullAuditedObjectProperties()
            .IgnoreExtraProperties();
            
        // Input DTO -> Entity (Update)
        CreateMap<UpdateBilliardTableDto, BilliardTable>()
            .IgnoreFullAuditedObjectProperties()
            .IgnoreExtraProperties();
    }
}
```

### 项目结构 (Project Structure)

```
Zss.BilliardHall/
├── src/
│   ├── Zss.BilliardHall.Domain/          # 域模型
│   │   ├── Entities/                     # 实体类
│   │   ├── ValueObjects/                 # 值对象
│   │   ├── Enums/                        # 枚举类型
│   │   ├── Interfaces/                   # 域接口
│   │   └── Services/                     # 域服务
│   ├── Zss.BilliardHall.Application/     # 应用服务层
│   │   ├── DTOs/                         # 数据传输对象
│   │   ├── Services/                     # 应用服务
│   │   ├── Validators/                   # 验证器
│   │   └── Mappers/                      # 映射器
│   ├── Zss.BilliardHall.Infrastructure/  # 基础设施层
│   │   ├── Data/                         # 数据访问
│   │   ├── Repositories/                 # 仓储实现
│   │   ├── ExternalServices/             # 外部服务
│   │   └── Configuration/                # 配置
│   └── Zss.BilliardHall.Api/            # API层
│       ├── Controllers/                  # 控制器
│       ├── Middleware/                   # 中间件
│       ├── Filters/                      # 过滤器
│       └── Extensions/                   # 扩展方法
├── tests/
│   ├── Zss.BilliardHall.Domain.Tests/
│   ├── Zss.BilliardHall.Application.Tests/
│   └── Zss.BilliardHall.Api.Tests/
└── docs/
```

### 实体类模式 (Entity Class Pattern)

```csharp
/// <summary>
/// 台球桌实体
/// </summary>
public class BilliardTable : BaseEntity
{
    /// <summary>
    /// 台球桌编号
    /// </summary>
    public int Number { get; private set; }
    
    /// <summary>
    /// 台球桌类型
    /// </summary>
    public TableType Type { get; private set; }
    
    /// <summary>
    /// 当前状态
    /// </summary>
    public TableStatus Status { get; private set; }
    
    /// <summary>
    /// 小时费率
    /// </summary>
    public Money HourlyRate { get; private set; }
    
    /// <summary>
    /// 位置信息
    /// </summary>
    public TableLocation Location { get; private set; }
    
    /// <summary>
    /// 所属台球厅ID
    /// </summary>
    public Guid HallId { get; private set; }
    
    // 私有构造函数，用于 EF Core
    private BilliardTable() { }
    
    /// <summary>
    /// 创建新的台球桌
    /// </summary>
    public BilliardTable(int number, TableType type, Money hourlyRate, 
                        TableLocation location, Guid hallId)
    {
        ValidateInputs(number, hourlyRate, location, hallId);
        
        Number = number;
        Type = type;
        Status = TableStatus.Available;
        HourlyRate = hourlyRate;
        Location = location;
        HallId = hallId;
    }
    
    /// <summary>
    /// 更新台球桌状态
    /// </summary>
    public void UpdateStatus(TableStatus newStatus, string reason = null)
    {
        if (Status == newStatus) return;
        
        ValidateStatusTransition(Status, newStatus);
        
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        
        // 记录状态变更事件
        AddDomainEvent(new TableStatusChangedEvent(Id, Status, newStatus, reason));
    }
    
    private void ValidateInputs(int number, Money hourlyRate, 
                               TableLocation location, Guid hallId)
    {
        if (number <= 0)
            throw new ArgumentException("台球桌编号必须大于0", nameof(number));
            
        if (hourlyRate == null || hourlyRate.Amount <= 0)
            throw new ArgumentException("小时费率必须大于0", nameof(hourlyRate));
            
        if (location == null)
            throw new ArgumentException("位置信息不能为空", nameof(location));
            
        if (hallId == Guid.Empty)
            throw new ArgumentException("台球厅ID不能为空", nameof(hallId));
    }
    
    private void ValidateStatusTransition(TableStatus from, TableStatus to)
    {
        // 业务规则：状态转换验证逻辑
        var validTransitions = new Dictionary<TableStatus, TableStatus[]>
        {
            [TableStatus.Available] = new[] { TableStatus.Occupied, TableStatus.Reserved, TableStatus.Maintenance },
            [TableStatus.Occupied] = new[] { TableStatus.Available, TableStatus.Maintenance },
            [TableStatus.Reserved] = new[] { TableStatus.Occupied, TableStatus.Available },
            [TableStatus.Maintenance] = new[] { TableStatus.Available, TableStatus.OutOfOrder },
            [TableStatus.OutOfOrder] = new[] { TableStatus.Maintenance, TableStatus.Available }
        };
        
        if (!validTransitions.ContainsKey(from) || !validTransitions[from].Contains(to))
        {
            throw new InvalidOperationException($"无法从状态 {from} 转换到 {to}");
        }
    }
}
```

### 服务层模式 (Service Layer Pattern)

```csharp
/// <summary>
/// 台球桌应用服务
/// </summary>
public class BilliardTableService : IBilliardTableService
{
    private readonly IBilliardTableRepository _repository;
    private readonly IValidator<CreateBilliardTableDto> _createValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<BilliardTableService> _logger;
    
    public BilliardTableService(
        IBilliardTableRepository repository,
        IValidator<CreateBilliardTableDto> createValidator,
        IMapper mapper,
        ILogger<BilliardTableService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// 创建台球桌
    /// </summary>
    public async Task<BilliardTableDto> CreateTableAsync(CreateBilliardTableDto dto)
    {
        _logger.LogInformation("开始创建台球桌，编号: {Number}", dto.Number);
        
        // 输入验证
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException("输入数据验证失败", validationResult.Errors);
        }
        
        // 业务规则验证
        await ValidateBusinessRulesAsync(dto);
        
        try
        {
            // 创建实体
            var table = new BilliardTable(
                dto.Number,
                dto.Type,
                new Money(dto.HourlyRate, "CNY"),
                new TableLocation(dto.LocationX, dto.LocationY, dto.Floor, dto.Zone),
                dto.HallId
            );
            
            // 保存到数据库
            var savedTable = await _repository.CreateAsync(table);
            
            _logger.LogInformation("成功创建台球桌，ID: {Id}, 编号: {Number}", 
                                  savedTable.Id, savedTable.Number);
            
            // 返回 DTO
            return _mapper.Map<BilliardTableDto>(savedTable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建台球桌失败，编号: {Number}", dto.Number);
            throw;
        }
    }
    
    /// <summary>
    /// 获取台球桌列表
    /// </summary>
    public async Task<PagedResult<BilliardTableDto>> GetTablesAsync(BilliardTableQuery query)
    {
        _logger.LogInformation("获取台球桌列表，页码: {Page}, 大小: {Size}", 
                              query.Page, query.PageSize);
        
        var result = await _repository.GetPagedAsync(
            page: query.Page,
            pageSize: query.PageSize,
            filter: BuildFilter(query),
            orderBy: BuildOrderBy(query.SortBy, query.SortDirection)
        );
        
        var dtos = _mapper.Map<List<BilliardTableDto>>(result.Items);
        
        return new PagedResult<BilliardTableDto>
        {
            Items = dtos,
            TotalItems = result.TotalItems,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
    
    private async Task ValidateBusinessRulesAsync(CreateBilliardTableDto dto)
    {
        // 检查台球桌编号是否重复
        var existingTable = await _repository.GetByNumberAsync(dto.Number, dto.HallId);
        if (existingTable != null)
        {
            throw new BusinessRuleException($"台球桌编号 {dto.Number} 已存在");
        }
        
        // 检查位置是否被占用
        var tableAtLocation = await _repository.GetByLocationAsync(
            dto.LocationX, dto.LocationY, dto.Floor, dto.HallId);
        if (tableAtLocation != null)
        {
            throw new BusinessRuleException("该位置已有台球桌");
        }
    }
    
    private Expression<Func<BilliardTable, bool>> BuildFilter(BilliardTableQuery query)
    {
        // 构建查询表达式
        Expression<Func<BilliardTable, bool>> filter = t => true;
        
        if (query.HallId.HasValue)
            filter = filter.And(t => t.HallId == query.HallId.Value);
            
        if (query.Status.HasValue)
            filter = filter.And(t => t.Status == query.Status.Value);
            
        if (query.Type.HasValue)
            filter = filter.And(t => t.Type == query.Type.Value);
            
        return filter;
    }
}
```

### 控制器模式 (Controller Pattern)

```csharp
/// <summary>
/// 台球桌管理 API
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class BilliardTablesController : ControllerBase
{
    private readonly IBilliardTableService _service;
    private readonly ILogger<BilliardTablesController> _logger;
    
    public BilliardTablesController(
        IBilliardTableService service,
        ILogger<BilliardTablesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// 获取台球桌列表
    /// </summary>
    /// <param name="query">查询参数</param>
    /// <returns>台球桌分页列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BilliardTableDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<BilliardTableDto>>>> GetTables(
        [FromQuery] BilliardTableQuery query)
    {
        var result = await _service.GetTablesAsync(query);
        return Ok(ApiResponse.Success(result));
    }
    
    /// <summary>
    /// 根据ID获取台球桌
    /// </summary>
    /// <param name="id">台球桌ID</param>
    /// <returns>台球桌详情</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BilliardTableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BilliardTableDto>>> GetTable(Guid id)
    {
        var table = await _service.GetByIdAsync(id);
        if (table == null)
        {
            return NotFound(ApiResponse.NotFound($"台球桌 {id} 不存在"));
        }
        
        return Ok(ApiResponse.Success(table));
    }
    
    /// <summary>
    /// 创建新台球桌
    /// </summary>
    /// <param name="dto">台球桌创建信息</param>
    /// <returns>创建的台球桌信息</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BilliardTableDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<BilliardTableDto>>> CreateTable(
        [FromBody] CreateBilliardTableDto dto)
    {
        var table = await _service.CreateTableAsync(dto);
        
        return CreatedAtAction(
            nameof(GetTable),
            new { id = table.Id },
            ApiResponse.Success(table, "台球桌创建成功")
        );
    }
    
    /// <summary>
    /// 更新台球桌状态
    /// </summary>
    /// <param name="id">台球桌ID</param>
    /// <param name="dto">状态更新信息</param>
    /// <returns>更新结果</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<BilliardTableDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BilliardTableDto>>> UpdateTableStatus(
        Guid id,
        [FromBody] UpdateTableStatusDto dto)
    {
        var table = await _service.UpdateStatusAsync(id, dto.Status, dto.Reason);
        return Ok(ApiResponse.Success(table, "状态更新成功"));
    }
}
```

## 错误处理模式 (Error Handling Patterns)

### 自定义异常类型

```csharp
/// <summary>
/// 业务规则异常
/// </summary>
public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }
    
    public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// 验证异常
/// </summary>
public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }
    
    public ValidationException(string message, IEnumerable<ValidationFailure> errors) 
        : base(message)
    {
        Errors = errors;
    }
}
```

### 全局异常处理中间件

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, 
                                   ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ValidationException ex => CreateValidationErrorResponse(ex),
            BusinessRuleException ex => CreateBusinessErrorResponse(ex),
            NotFoundException ex => CreateNotFoundResponse(ex),
            UnauthorizedAccessException ex => CreateUnauthorizedResponse(ex),
            _ => CreateInternalErrorResponse(exception)
        };
        
        context.Response.StatusCode = response.StatusCode;
        
        _logger.LogError(exception, "处理请求时发生异常: {Message}", exception.Message);
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Content));
    }
}
```

## 测试模式 (Testing Patterns)

### 单元测试示例

```csharp
[TestClass]
public class BilliardTableServiceTests
{
    private readonly Mock<IBilliardTableRepository> _mockRepository;
    private readonly Mock<IValidator<CreateBilliardTableDto>> _mockValidator;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<BilliardTableService>> _mockLogger;
    private readonly BilliardTableService _service;
    
    public BilliardTableServiceTests()
    {
        _mockRepository = new Mock<IBilliardTableRepository>();
        _mockValidator = new Mock<IValidator<CreateBilliardTableDto>>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<BilliardTableService>>();
        
        _service = new BilliardTableService(
            _mockRepository.Object,
            _mockValidator.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }
    
    [TestMethod]
    public async Task CreateTableAsync_ValidInput_ReturnsCreatedTable()
    {
        // Arrange
        var dto = new CreateBilliardTableDto
        {
            Number = 1,
            Type = TableType.Chinese8Ball,
            HourlyRate = 35.00m,
            LocationX = 10.5f,
            LocationY = 5.2f,
            Floor = 1,
            Zone = "A",
            HallId = Guid.NewGuid()
        };
        
        var validationResult = new ValidationResult();
        _mockValidator.Setup(v => v.ValidateAsync(dto, default))
                     .ReturnsAsync(validationResult);
        
        var createdTable = new BilliardTable(dto.Number, dto.Type, 
                                           new Money(dto.HourlyRate, "CNY"),
                                           new TableLocation(dto.LocationX, dto.LocationY, dto.Floor, dto.Zone),
                                           dto.HallId);
        
        _mockRepository.Setup(r => r.GetByNumberAsync(dto.Number, dto.HallId))
                      .ReturnsAsync((BilliardTable)null);
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<BilliardTable>()))
                      .ReturnsAsync(createdTable);
        
        var expectedDto = new BilliardTableDto { Id = createdTable.Id };
        _mockMapper.Setup(m => m.Map<BilliardTableDto>(createdTable))
                  .Returns(expectedDto);
        
        // Act
        var result = await _service.CreateTableAsync(dto);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedDto.Id, result.Id);
        
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<BilliardTable>()), Times.Once);
    }
    
    [TestMethod]
    public async Task CreateTableAsync_DuplicateNumber_ThrowsBusinessRuleException()
    {
        // Arrange
        var dto = new CreateBilliardTableDto { Number = 1, HallId = Guid.NewGuid() };
        var existingTable = new BilliardTable(1, TableType.Chinese8Ball, 
                                             new Money(35, "CNY"), 
                                             new TableLocation(1, 1, 1, "A"), 
                                             dto.HallId);
        
        _mockValidator.Setup(v => v.ValidateAsync(dto, default))
                     .ReturnsAsync(new ValidationResult());
        
        _mockRepository.Setup(r => r.GetByNumberAsync(dto.Number, dto.HallId))
                      .ReturnsAsync(existingTable);
        
        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => _service.CreateTableAsync(dto));
        
        Assert.AreEqual("台球桌编号 1 已存在", exception.Message);
    }
}
```

## 性能优化模式 (Performance Optimization Patterns)

### 缓存使用

```csharp
public class CachedBilliardTableService : IBilliardTableService
{
    private readonly IBilliardTableService _baseService;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    
    public async Task<BilliardTableDto> GetByIdAsync(Guid id)
    {
        var cacheKey = $"table:{id}";
        
        if (_cache.TryGetValue(cacheKey, out BilliardTableDto cachedTable))
        {
            return cachedTable;
        }
        
        var table = await _baseService.GetByIdAsync(id);
        
        if (table != null)
        {
            _cache.Set(cacheKey, table, _cacheExpiration);
        }
        
        return table;
    }
}
```

### 批量操作

```csharp
/// <summary>
/// 批量更新台球桌状态
/// </summary>
public async Task<List<BilliardTableDto>> UpdateMultipleStatusAsync(
    List<UpdateMultipleStatusDto> updates)
{
    var tableIds = updates.Select(u => u.TableId).ToList();
    var tables = await _repository.GetByIdsAsync(tableIds);
    
    var updatedTables = new List<BilliardTable>();
    
    foreach (var update in updates)
    {
        var table = tables.FirstOrDefault(t => t.Id == update.TableId);
        if (table != null)
        {
            table.UpdateStatus(update.Status, update.Reason);
            updatedTables.Add(table);
        }
    }
    
    await _repository.UpdateRangeAsync(updatedTables);
    
    return _mapper.Map<List<BilliardTableDto>>(updatedTables);
}
```

---

> 以上模式和约定应在所有代码生成和开发活动中严格遵循，确保代码质量和一致性。