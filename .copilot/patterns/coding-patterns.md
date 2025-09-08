# 编码模式和约定 (Coding Patterns and Conventions)

## 总体原则 (General Principles)

### 1. 机器可读优先 (Machine-Readable First)
- 使用强类型定义
- 遵循一致的命名约定
- 提供详细的代码注释和文档
- 实现完整的数据验证

### 2. 人机协作优化 (Human-AI Collaboration Optimized)
- 清晰的代码结构和分层
- 易于理解的业务逻辑表达
- 丰富的上下文信息
- 标准化的错误处理

### 3. 自动化友好 (Automation-Friendly)
- 可测试的代码设计
- 配置外部化
- 日志和监控支持
- CI/CD 流水线兼容

## C# 后端编码规范

### 命名约定 (Naming Conventions)

```csharp
// 类名：PascalCase
public class BilliardTableService { }

// 接口名：以 I 开头的 PascalCase
public interface IBilliardTableRepository { }

// 方法名：PascalCase
public async Task<BilliardTable> CreateTableAsync(CreateBilliardTableDto dto) { }

// 属性名：PascalCase
public string Name { get; set; }

// 字段名：_camelCase (私有字段)
private readonly ILogger<BilliardTableService> _logger;

// 常量：UPPER_SNAKE_CASE
public const int MAX_TABLE_NUMBER = 999;

// 枚举：PascalCase
public enum TableStatus
{
    Available,
    Occupied,
    Reserved,
    Maintenance,
    OutOfOrder
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