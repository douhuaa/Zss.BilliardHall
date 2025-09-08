# API 设计模式 (API Design Patterns)

## RESTful API 设计原则

### 1. 资源命名约定 (Resource Naming Conventions)

```http
# 好的 API 设计
GET    /api/v1/billiard-tables              # 获取台球桌列表
GET    /api/v1/billiard-tables/{id}         # 获取特定台球桌
POST   /api/v1/billiard-tables              # 创建台球桌
PUT    /api/v1/billiard-tables/{id}         # 完整更新台球桌
PATCH  /api/v1/billiard-tables/{id}         # 部分更新台球桌
DELETE /api/v1/billiard-tables/{id}         # 删除台球桌

# 子资源操作
GET    /api/v1/billiard-tables/{id}/reservations    # 获取台球桌的预约
POST   /api/v1/billiard-tables/{id}/reservations    # 为台球桌创建预约
PATCH  /api/v1/billiard-tables/{id}/status          # 更新台球桌状态

# 批量操作
POST   /api/v1/billiard-tables/batch                # 批量创建
PATCH  /api/v1/billiard-tables/batch                # 批量更新
```

### 2. HTTP 状态码使用规范

```csharp
/// <summary>
/// 标准 HTTP 状态码使用指南
/// </summary>
public static class HttpStatusCodes
{
    // 2xx 成功
    public const int OK = 200;              // 成功获取资源
    public const int CREATED = 201;         // 成功创建资源
    public const int ACCEPTED = 202;        // 请求已接受，异步处理
    public const int NO_CONTENT = 204;      // 成功，无返回内容
    
    // 4xx 客户端错误
    public const int BAD_REQUEST = 400;     // 请求参数错误
    public const int UNAUTHORIZED = 401;    // 未认证
    public const int FORBIDDEN = 403;       // 无权限
    public const int NOT_FOUND = 404;       // 资源不存在
    public const int CONFLICT = 409;        // 资源冲突
    public const int UNPROCESSABLE_ENTITY = 422;  // 验证失败
    public const int TOO_MANY_REQUESTS = 429;     // 请求限流
    
    // 5xx 服务器错误
    public const int INTERNAL_SERVER_ERROR = 500; // 服务器内部错误
    public const int SERVICE_UNAVAILABLE = 503;   // 服务不可用
}
```

### 3. API 版本控制策略

```csharp
// URL 路径版本控制 (推荐)
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class BilliardTablesController : ControllerBase
{
    [HttpGet]
    [MapToApiVersions("1.0", "2.0")]
    public async Task<ActionResult> GetTables()
    {
        // v1.0 和 v2.0 共享的逻辑
    }
    
    [HttpGet("advanced")]
    [MapToApiVersions("2.0")]
    public async Task<ActionResult> GetTablesAdvanced()
    {
        // 仅 v2.0 支持的功能
    }
}

// 请求头版本控制 (备选)
[ApiController]
[Route("api/[controller]")]
public class BilliardTablesController : ControllerBase
{
    [HttpGet]
    [ApiVersion("1.0")]
    public async Task<ActionResult> GetTablesV1()
    {
        // Accept: application/vnd.billiard.v1+json
    }
    
    [HttpGet]
    [ApiVersion("2.0")]
    public async Task<ActionResult> GetTablesV2()
    {
        // Accept: application/vnd.billiard.v2+json
    }
}
```

## 查询参数模式 (Query Parameter Patterns)

### 1. 分页查询

```csharp
/// <summary>
/// 分页查询基类
/// </summary>
public class PagedQuery
{
    /// <summary>
    /// 页码 (从1开始)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// 每页大小
    /// </summary>
    [Range(1, 100, ErrorMessage = "每页大小必须在1-100之间")]
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";
    
    /// <summary>
    /// 排序方向
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}

/// <summary>
/// 台球桌查询参数
/// </summary>
public class BilliardTableQuery : PagedQuery
{
    /// <summary>
    /// 台球厅ID过滤
    /// </summary>
    public Guid? HallId { get; set; }
    
    /// <summary>
    /// 状态过滤
    /// </summary>
    public TableStatus? Status { get; set; }
    
    /// <summary>
    /// 类型过滤
    /// </summary>
    public TableType? Type { get; set; }
    
    /// <summary>
    /// 编号范围过滤
    /// </summary>
    public int? NumberFrom { get; set; }
    public int? NumberTo { get; set; }
    
    /// <summary>
    /// 价格范围过滤
    /// </summary>
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    
    /// <summary>
    /// 关键字搜索
    /// </summary>
    public string Search { get; set; }
    
    /// <summary>
    /// 是否包含已删除记录
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;
}

// 使用示例
[HttpGet]
public async Task<ActionResult<ApiResponse<PagedResult<BilliardTableDto>>>> GetTables(
    [FromQuery] BilliardTableQuery query)
{
    var result = await _service.GetTablesAsync(query);
    return Ok(ApiResponse.Success(result));
}
```

### 2. 过滤和搜索

```http
# 基础过滤
GET /api/v1/billiard-tables?status=Available&type=Chinese8Ball

# 范围过滤
GET /api/v1/billiard-tables?numberFrom=1&numberTo=10&priceFrom=30&priceTo=50

# 关键字搜索
GET /api/v1/billiard-tables?search=VIP

# 复合查询
GET /api/v1/billiard-tables?status=Available&search=VIP&sortBy=Number&sortDirection=Ascending

# 日期范围查询
GET /api/v1/reservations?startDate=2023-12-01&endDate=2023-12-31

# 包含关联数据
GET /api/v1/billiard-tables?include=Hall,Reservations
```

### 3. 字段选择

```http
# 选择特定字段 (减少数据传输)
GET /api/v1/billiard-tables?fields=id,number,status,hourlyRate

# 排除字段
GET /api/v1/billiard-tables?exclude=createdAt,updatedAt
```

## 响应格式标准化

### 1. 成功响应格式

```csharp
/// <summary>
/// API 统一响应包装器
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; }
    public string Version { get; set; }
    
    public static ApiResponse<T> Success(T data, string message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "操作成功"
        };
    }
}

/// <summary>
/// 分页结果包装器
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public PaginationInfo Pagination { get; set; }
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}
```

### 2. 错误响应格式

```csharp
/// <summary>
/// 错误响应包装器
/// </summary>
public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public ErrorDetails Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; }
    
    public static ErrorResponse Create(string code, string message, object details = null)
    {
        return new ErrorResponse
        {
            Error = new ErrorDetails
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }
}

public class ErrorDetails
{
    public string Code { get; set; }
    public string Message { get; set; }
    public object Details { get; set; }
    public List<ValidationError> ValidationErrors { get; set; }
}

public class ValidationError
{
    public string Field { get; set; }
    public string Message { get; set; }
    public object Value { get; set; }
}
```

## 数据传输对象 (DTO) 模式

### 1. 输入 DTO

```csharp
/// <summary>
/// 创建台球桌 DTO
/// </summary>
public class CreateBilliardTableDto
{
    /// <summary>
    /// 台球桌编号
    /// </summary>
    [Required(ErrorMessage = "台球桌编号不能为空")]
    [Range(1, 999, ErrorMessage = "台球桌编号必须在1-999之间")]
    public int Number { get; set; }
    
    /// <summary>
    /// 台球桌类型
    /// </summary>
    [Required(ErrorMessage = "台球桌类型不能为空")]
    [EnumDataType(typeof(TableType), ErrorMessage = "无效的台球桌类型")]
    public TableType Type { get; set; }
    
    /// <summary>
    /// 小时费率
    /// </summary>
    [Required(ErrorMessage = "小时费率不能为空")]
    [Range(0.01, 999.99, ErrorMessage = "小时费率必须在0.01-999.99之间")]
    public decimal HourlyRate { get; set; }
    
    /// <summary>
    /// X坐标
    /// </summary>
    [Required(ErrorMessage = "X坐标不能为空")]
    public float LocationX { get; set; }
    
    /// <summary>
    /// Y坐标
    /// </summary>
    [Required(ErrorMessage = "Y坐标不能为空")]
    public float LocationY { get; set; }
    
    /// <summary>
    /// 楼层
    /// </summary>
    [Range(1, 99, ErrorMessage = "楼层必须在1-99之间")]
    public int Floor { get; set; } = 1;
    
    /// <summary>
    /// 区域
    /// </summary>
    [StringLength(20, ErrorMessage = "区域名称不能超过20个字符")]
    public string Zone { get; set; }
    
    /// <summary>
    /// 所属台球厅ID
    /// </summary>
    [Required(ErrorMessage = "台球厅ID不能为空")]
    public Guid HallId { get; set; }
    
    /// <summary>
    /// 台球桌特性
    /// </summary>
    public List<string> Features { get; set; } = new List<string>();
}

/// <summary>
/// 更新台球桌 DTO
/// </summary>
public class UpdateBilliardTableDto
{
    /// <summary>
    /// 小时费率
    /// </summary>
    [Range(0.01, 999.99, ErrorMessage = "小时费率必须在0.01-999.99之间")]
    public decimal? HourlyRate { get; set; }
    
    /// <summary>
    /// 位置信息
    /// </summary>
    public UpdateLocationDto Location { get; set; }
    
    /// <summary>
    /// 台球桌特性
    /// </summary>
    public List<string> Features { get; set; }
}

/// <summary>
/// 状态更新 DTO
/// </summary>
public class UpdateTableStatusDto
{
    /// <summary>
    /// 新状态
    /// </summary>
    [Required(ErrorMessage = "状态不能为空")]
    [EnumDataType(typeof(TableStatus), ErrorMessage = "无效的状态")]
    public TableStatus Status { get; set; }
    
    /// <summary>
    /// 状态变更原因
    /// </summary>
    [StringLength(200, ErrorMessage = "原因不能超过200个字符")]
    public string Reason { get; set; }
}
```

### 2. 输出 DTO

```csharp
/// <summary>
/// 台球桌详情 DTO
/// </summary>
public class BilliardTableDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public MoneyDto HourlyRate { get; set; }
    public TableLocationDto Location { get; set; }
    public List<string> Features { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // 关联数据 (可选)
    public BilliardHallSummaryDto Hall { get; set; }
    public List<ReservationSummaryDto> ActiveReservations { get; set; }
}

/// <summary>
/// 台球桌列表项 DTO (简化版本)
/// </summary>
public class BilliardTableListDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public MoneyDto HourlyRate { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? NextAvailableTime { get; set; }
}
```

## 请求验证模式

### 1. FluentValidation 验证器

```csharp
/// <summary>
/// 创建台球桌验证器
/// </summary>
public class CreateBilliardTableDtoValidator : AbstractValidator<CreateBilliardTableDto>
{
    private readonly IBilliardHallRepository _hallRepository;
    
    public CreateBilliardTableDtoValidator(IBilliardHallRepository hallRepository)
    {
        _hallRepository = hallRepository;
        
        RuleFor(x => x.Number)
            .GreaterThan(0)
            .WithMessage("台球桌编号必须大于0")
            .LessThanOrEqualTo(999)
            .WithMessage("台球桌编号不能超过999");
            
        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("无效的台球桌类型");
            
        RuleFor(x => x.HourlyRate)
            .GreaterThan(0)
            .WithMessage("小时费率必须大于0")
            .LessThanOrEqualTo(999.99m)
            .WithMessage("小时费率不能超过999.99");
            
        RuleFor(x => x.HallId)
            .NotEmpty()
            .WithMessage("台球厅ID不能为空")
            .MustAsync(HallExists)
            .WithMessage("指定的台球厅不存在");
            
        RuleFor(x => x.LocationX)
            .GreaterThanOrEqualTo(0)
            .WithMessage("X坐标不能为负数");
            
        RuleFor(x => x.LocationY)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Y坐标不能为负数");
            
        RuleFor(x => x.Zone)
            .MaximumLength(20)
            .WithMessage("区域名称不能超过20个字符");
    }
    
    private async Task<bool> HallExists(Guid hallId, CancellationToken cancellationToken)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);
        return hall != null && hall.Status == HallStatus.Active;
    }
}
```

### 2. 自定义验证特性

```csharp
/// <summary>
/// 营业时间验证特性
/// </summary>
public class BusinessHoursAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime dateTime)
        {
            var hour = dateTime.Hour;
            if (hour >= 8 && hour <= 23)
            {
                return ValidationResult.Success;
            }
        }
        
        return new ValidationResult("预约时间必须在营业时间内 (08:00-23:00)");
    }
}

/// <summary>
/// 台球桌编号唯一性验证
/// </summary>
public class UniqueTableNumberAttribute : ValidationAttribute
{
    public Type ServiceType { get; set; } = typeof(IBilliardTableService);
    
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var service = (IBilliardTableService)validationContext.GetService(ServiceType);
        var dto = (CreateBilliardTableDto)validationContext.ObjectInstance;
        
        if (value is int number)
        {
            var existingTable = service.GetByNumberAsync(number, dto.HallId).Result;
            if (existingTable != null)
            {
                return new ValidationResult($"台球桌编号 {number} 在该台球厅已存在");
            }
        }
        
        return ValidationResult.Success;
    }
}
```

## API 文档模式

### 1. Swagger/OpenAPI 配置

```csharp
/// <summary>
/// Swagger 配置
/// </summary>
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "智慧台球厅管理系统 API",
                Version = "v1.0",
                Description = "台球厅管理系统的 RESTful API 接口文档",
                Contact = new OpenApiContact
                {
                    Name = "开发团队",
                    Email = "dev@billiard-hall.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });
            
            // 包含 XML 注释
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            
            // JWT 认证配置
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "请输入 JWT Token (格式: Bearer {token})",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            
            // 全局安全要求
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        return services;
    }
}
```

### 2. API 文档注释规范

```csharp
/// <summary>
/// 台球桌管理 API
/// </summary>
/// <remarks>
/// 提供台球桌的增删改查功能，包括:
/// - 台球桌基本信息管理
/// - 状态管理和实时更新
/// - 预约关联查询
/// - 批量操作支持
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class BilliardTablesController : ControllerBase
{
    /// <summary>
    /// 获取台球桌列表
    /// </summary>
    /// <param name="query">查询参数，支持分页、过滤、排序</param>
    /// <returns>台球桌分页列表</returns>
    /// <remarks>
    /// 获取台球桌列表，支持多种过滤条件:
    /// 
    /// **查询参数说明:**
    /// - `page`: 页码，从1开始，默认为1
    /// - `pageSize`: 每页大小，1-100之间，默认为20
    /// - `status`: 状态过滤 (Available, Occupied, Reserved, Maintenance, OutOfOrder)
    /// - `type`: 类型过滤 (Chinese8Ball, American9Ball, Snooker, Carom)
    /// - `search`: 关键字搜索，支持编号、区域搜索
    /// - `sortBy`: 排序字段 (Number, Type, Status, HourlyRate, CreatedAt)
    /// - `sortDirection`: 排序方向 (Ascending, Descending)
    /// 
    /// **示例请求:**
    /// ```
    /// GET /api/v1/billiard-tables?status=Available&type=Chinese8Ball&page=1&pageSize=10
    /// ```
    /// </remarks>
    /// <response code="200">成功返回台球桌列表</response>
    /// <response code="400">请求参数错误</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BilliardTableListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<BilliardTableListDto>>>> GetTables(
        [FromQuery] BilliardTableQuery query)
    {
        var result = await _service.GetTablesAsync(query);
        return Ok(ApiResponse.Success(result));
    }
}
```

## 性能优化模式

### 1. 缓存策略

```csharp
/// <summary>
/// 响应缓存配置
/// </summary>
[ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "hallId", "status" })]
[HttpGet]
public async Task<ActionResult> GetTables([FromQuery] BilliardTableQuery query)
{
    // 缓存5分钟，根据查询参数区分缓存
}

/// <summary>
/// 条件缓存
/// </summary>
[HttpGet("{id}")]
public async Task<ActionResult> GetTable(Guid id)
{
    var table = await _service.GetByIdAsync(id);
    if (table == null)
    {
        return NotFound();
    }
    
    // 设置 ETag 用于条件请求
    Response.Headers.ETag = $"\"{table.UpdatedAt:O}\"";
    
    return Ok(ApiResponse.Success(table));
}
```

### 2. 压缩和优化

```csharp
/// <summary>
/// 响应压缩配置
/// </summary>
public void ConfigureServices(IServiceCollection services)
{
    services.AddResponseCompression(options =>
    {
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json" });
    });
}
```

### 3. 限流保护

```csharp
/// <summary>
/// API 限流配置
/// </summary>
[EnableRateLimiting("ApiPolicy")]
[HttpPost]
public async Task<ActionResult> CreateTable([FromBody] CreateBilliardTableDto dto)
{
    // 创建台球桌的限流保护
}
```

---

> 以上 API 设计模式应在所有接口开发中严格遵循，确保 API 的一致性、可用性和可维护性。