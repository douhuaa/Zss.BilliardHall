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
# 推荐技术栈配置
backend:
  framework: "ASP.NET Core" # 或 Spring Boot
  database: "SQL Server" # 或 PostgreSQL
  orm: "Entity Framework Core" # 或 MyBatis
  api: "RESTful API + GraphQL"
  
frontend:
  framework: "React" # 或 Vue.js
  ui_library: "Ant Design" # 或 Element Plus
  state_management: "Redux Toolkit" # 或 Vuex/Pinia
  
mobile:
  framework: "React Native" # 或 Flutter
  
infrastructure:
  containerization: "Docker"
  orchestration: "Kubernetes"
  ci_cd: "GitHub Actions"
  monitoring: "Application Insights"
```

## 业务领域模型 (Business Domain Model)

### 核心实体 (Core Entities)

```typescript
// 台球厅核心业务实体
interface BilliardHall {
  id: string;
  name: string;
  address: Address;
  tables: BilliardTable[];
  rooms: PrivateRoom[];
  staff: Staff[];
  operatingHours: OperatingHours;
}

interface BilliardTable {
  id: string;
  number: number;
  type: TableType; // 中式黑八, 美式九球, 斯诺克
  status: TableStatus; // 空闲, 占用, 维护, 故障
  hourlyRate: Money;
  location: TableLocation;
}

interface Reservation {
  id: string;
  customerId: string;
  tableId: string;
  startTime: DateTime;
  duration: Duration;
  totalAmount: Money;
  status: ReservationStatus;
}
```

## 代码生成指南 (Code Generation Guidelines)

### 1. API 开发模式

当生成 API 相关代码时：

```csharp
// 控制器模式示例
[ApiController]
[Route("api/[controller]")]
public class BilliardTablesController : ControllerBase
{
    private readonly IBilliardTableService _service;
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<BilliardTableDto>>> GetTables(
        [FromQuery] BilliardTableQuery query)
    {
        var result = await _service.GetTablesAsync(query);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<BilliardTableDto>> CreateTable(
        [FromBody] CreateBilliardTableDto dto)
    {
        var result = await _service.CreateTableAsync(dto);
        return CreatedAtAction(nameof(GetTable), new { id = result.Id }, result);
    }
}
```

### 2. 数据库设计模式

```sql
-- 表结构设计原则
CREATE TABLE BilliardTables (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Number INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available',
    HourlyRate DECIMAL(10,2) NOT NULL,
    LocationX FLOAT,
    LocationY FLOAT,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_BilliardTables_Status (Status),
    INDEX IX_BilliardTables_Type (Type)
);
```

### 3. 前端组件模式

```tsx
// React 组件开发模式
interface BilliardTableCardProps {
  table: BilliardTable;
  onReserve: (tableId: string) => void;
  onStatusChange: (tableId: string, status: TableStatus) => void;
}

export const BilliardTableCard: React.FC<BilliardTableCardProps> = ({
  table,
  onReserve,
  onStatusChange
}) => {
  return (
    <Card className="billiard-table-card">
      <div className="table-header">
        <span className="table-number">台球桌 #{table.number}</span>
        <StatusBadge status={table.status} />
      </div>
      <div className="table-info">
        <p>类型: {table.type}</p>
        <p>价格: ¥{table.hourlyRate}/小时</p>
      </div>
      <div className="table-actions">
        {table.status === 'Available' && (
          <Button onClick={() => onReserve(table.id)}>
            预约
          </Button>
        )}
      </div>
    </Card>
  );
};
```

## 测试策略 (Testing Strategy)

### 单元测试模式
```csharp
[TestClass]
public class BilliardTableServiceTests
{
    private readonly Mock<IBilliardTableRepository> _mockRepository;
    private readonly BilliardTableService _service;
    
    public BilliardTableServiceTests()
    {
        _mockRepository = new Mock<IBilliardTableRepository>();
        _service = new BilliardTableService(_mockRepository.Object);
    }
    
    [TestMethod]
    public async Task CreateTableAsync_ValidInput_ReturnsCreatedTable()
    {
        // Arrange
        var dto = new CreateBilliardTableDto { /* ... */ };
        var expectedTable = new BilliardTable { /* ... */ };
        
        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<BilliardTable>()))
                      .ReturnsAsync(expectedTable);
        
        // Act
        var result = await _service.CreateTableAsync(dto);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedTable.Id, result.Id);
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

## 安全要求 (Security Requirements)

```csharp
// JWT 认证配置
[Authorize]
[ApiController]
public class SecureController : ControllerBase
{
    [HttpGet]
    [RequirePermission("billiard.tables.read")]
    public async Task<IActionResult> GetTables()
    {
        // 实现逻辑
    }
}
```

## 部署和运维 (Deployment & Operations)

```yaml
# Docker 配置示例
apiVersion: apps/v1
kind: Deployment
metadata:
  name: billiard-hall-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: billiard-hall-api
  template:
    spec:
      containers:
      - name: api
        image: billiard-hall-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
```

## 相关指令文件 (Related Instruction Files)

- [代码模式和约定](./.copilot/patterns/coding-patterns.md)
- [API 设计规范](./.copilot/patterns/api-patterns.md)
- [数据库设计规范](./.copilot/patterns/database-patterns.md)
- [测试指导](./.copilot/patterns/testing-patterns.md)
- [工作流模板](./.copilot/workflows/README.md)

---

> 此文档将随项目发展持续更新。所有代码生成都应遵循以上指南，确保一致性和质量。