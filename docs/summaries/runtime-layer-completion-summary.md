# 运行层补全实施摘要

**日期**：2026-01-22  
**Issue**：运行层  
**描述**：补全运行层缺失

## 问题分析

项目采用 ADR-0002 定义的 Platform / Application / Host 三层启动体系，但初始实现仅包含：
- ✅ 基础项目结构（Platform、Application、Host.Web、Host.Worker）
- ✅ Bootstrapper 入口点（服务注册）
- ❌ **缺失**：运行时中间件管道和端点映射

具体缺失内容：
1. Platform 层没有中间件配置方法
2. Application 层没有端点映射方法
3. Web Program.cs 没有配置中间件管道

## 实施方案

### 1. Platform 层扩展

**文件**：`src/Platform/PlatformBootstrapper.cs`

**新增方法**：
```csharp
public static void ConfigureMiddleware(
    IApplicationBuilder app,
    IHostEnvironment environment)
```

**职责**：
- 配置平台层技术中间件
- 开发环境异常页面
- 预留健康检查和监控接入点

**符合 ADR-0002**：
- ✅ 不包含业务逻辑
- ✅ 不感知 Host 类型（仅使用 ASP.NET Core 通用接口）
- ✅ 专注技术能力

### 2. Application 层扩展

**文件**：`src/Application/ApplicationBootstrapper.cs`

**新增方法**：
```csharp
public static void ConfigureMiddleware(IApplicationBuilder app)
public static void MapEndpoints(IEndpointRouteBuilder endpoints)
```

**职责**：
- `ConfigureMiddleware`: 配置应用层中间件（认证、授权等）
- `MapEndpoints`: 注册应用端点（当前包含临时健康检查端点）

**符合 ADR-0002**：
- ✅ 定义"系统是什么"
- ✅ 不依赖 Host 具体类型
- ✅ 预留模块端点自动发现机制

### 3. Web Host 更新

**文件**：`src/Host/Web/Program.cs`

**新增调用**：
```csharp
// 配置中间件管道
PlatformBootstrapper.ConfigureMiddleware(app, app.Environment);
ApplicationBootstrapper.ConfigureMiddleware(app);

// 映射端点
ApplicationBootstrapper.MapEndpoints(app);
```

**符合 ADR-0002**：
- ✅ 仅调用 Bootstrapper 方法
- ✅ 不包含业务逻辑
- ✅ 保持简洁（18 行，远低于 30 行限制）

## 实施结果

### ✅ 架构合规性验证

```bash
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj
```

**结果**：84/84 测试通过
- ADR-0002 所有测试通过（17 项）
- 无架构违规

### ✅ 功能验证

**Web 主机**：
```bash
$ dotnet run --project src/Host/Web/Web.csproj
info: Now listening on: http://localhost:5000
```

**端点测试**：
```bash
$ curl http://localhost:5000/
{
  "application": "Zss.BilliardHall",
  "status": "Running",
  "timestamp": "2026-01-22T13:40:38.5874161Z"
}
```

**Worker 主机**：
```bash
$ dotnet run --project src/Host/Worker/Worker.csproj
info: Worker running at: 01/22/2026 13:39:44 +00:00
```

### 📊 变更统计

| 文件 | 新增行 | 修改行 | 总行数 |
|------|-------|-------|--------|
| PlatformBootstrapper.cs | +26 | 3 | 43 |
| ApplicationBootstrapper.cs | +38 | 3 | 57 |
| Program.cs (Web) | +7 | 0 | 18 |
| **总计** | **+71** | **6** | **118** |

## 架构影响分析

### 遵循的 ADR 原则

#### ADR-0002：三层启动体系
- ✅ Platform 层仅提供技术能力
- ✅ Application 层定义系统能力
- ✅ Host 层保持薄壳（仅编排）
- ✅ 单向依赖：Host → Application → Platform

#### 代码组织
- ✅ Bootstrapper 作为各层唯一装配入口
- ✅ 中间件配置由 Platform/Application 提供
- ✅ Program.cs 保持声明式，无业务逻辑

### 后续扩展点

当前实现为基础框架，预留以下扩展点：

#### Platform 层
- 健康检查端点注册
- 分布式追踪集成（OpenTelemetry）
- 结构化日志配置（Serilog）
- 指标收集（Prometheus）

#### Application 层
- 模块端点自动发现和注册
- 全局异常处理中间件
- 请求/响应日志
- 认证授权配置

#### 端点映射
- Wolverine.HTTP 集成
- 模块端点自动扫描
- API 版本控制
- OpenAPI/Swagger 集成

## 技术说明

### 为什么不在 Platform 中使用 ASP.NET Core 特定类型？

ADR-0002 明确规定：
> Platform 不得依赖 ASP.NET Core 类型以外的 Host 语义

当前使用的类型均来自 ASP.NET Core 框架：
- `IApplicationBuilder`：中间件管道构建
- `IEndpointRouteBuilder`：端点路由
- `IHostEnvironment`：环境信息

这些是框架级抽象，不绑定具体 Host 实例。

### Program.cs 行数控制

ADR-0002 建议：
> 超过 30 行 = 架构审查  
> Program.cs 中允许中间件，仅限 Platform / Application 提供

当前实现：
- 18 行（包括空行和注释）
- 仅包含 Bootstrapper 调用
- 无条件分支或业务逻辑

## 验收标准

- [x] Platform 层提供中间件配置方法
- [x] Application 层提供端点映射方法
- [x] Web 主机能够启动并响应请求
- [x] Worker 主机不受影响
- [x] 所有架构测试通过
- [x] Program.cs 保持简洁（< 30 行）
- [x] 无 ADR 违规
- [x] 符合三层依赖方向

## 总结

本次补全实现了完整的运行层框架，包括：

1. **Platform 层**：提供技术中间件配置能力
2. **Application 层**：提供业务中间件和端点映射能力  
3. **Host 层**：通过薄壳编排启动流程

所有实现严格遵循 ADR-0002 的三层启动体系约束，为后续功能扩展奠定了坚实基础。

---

**相关文档**：
- [ADR-0002：Platform / Application / Host 三层启动体系](../adr/constitutional/ADR-0002-platform-application-host-bootstrap.md)
- [架构指南](../architecture-guide.md)
