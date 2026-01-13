# Aspire 项目结构

本目录包含台球厅管理系统的 .NET Aspire 编排项目和共享配置。

---

## 📁 项目结构

```
Aspire/
├── Zss.BilliardHall.Wolverine.AppHost/           # Aspire 编排主机
│   ├── AppHost.cs                                 # 资源定义和编排逻辑
│   └── Program.cs                                 # 入口点
│
├── Zss.BilliardHall.Wolverine.ServiceDefaults/   # 共享服务配置
│   ├── Extensions.cs                              # 扩展方法（服务发现、健康检查、OpenTelemetry）
│   └── README.md                                  # ServiceDefaults 使用说明
│
└── Zss.BilliardHall.Wolverine.ServiceDefaults.Tests/  # ServiceDefaults 单元测试
    └── ServiceDefaultsIntegrationTests.cs
```

---

## 🚀 快速开始

### 1. 启动 Aspire 应用

**Visual Studio / Rider**:
1. 设置 `Zss.BilliardHall.Wolverine.AppHost` 为启动项目
2. 按 F5 运行
3. 浏览器自动打开 Aspire Dashboard（`https://localhost:17001`）

**命令行**:
```bash
cd Zss.BilliardHall.Wolverine.AppHost
dotnet run
```

### 2. 查看 Dashboard

打开 `https://localhost:17001`，可以：
- 查看所有资源状态（服务、容器、数据库）
- 实时查看日志
- 查看分布式追踪（Traces）
- 监控性能指标（Metrics）

---

## 📦 项目说明

### Zss.BilliardHall.Wolverine.AppHost

**职责**: 定义和编排应用的所有资源。

**当前配置**:
```csharp
var postgres = builder
    .AddPostgres("postgres")                        // PostgreSQL 容器
    .WithDataVolume()                               // 数据持久化
    .WithLifetime(ContainerLifetime.Persistent);    // 容器持久化

var db = postgres.AddDatabase("billiard-hall-db"); // 数据库

builder.AddProject<Projects.Bootstrapper>("bootstrapper")
    .WithReference(db)                              // 注入连接字符串
    .WaitFor(db);                                   // 等待数据库就绪
```

**关键特性**:
- 🐘 PostgreSQL 容器（持久化数据卷）
- 🔗 自动注入连接字符串到服务
- ⏳ 依赖等待（确保数据库就绪后再启动服务）
- 📊 统一的监控面板（Aspire Dashboard）

### Zss.BilliardHall.Wolverine.ServiceDefaults

**职责**: 所有服务共享的基础设施配置。

**核心功能**:
- ✅ **服务发现**: 自动解析 `http+https://service-name` 到实际地址
- ✅ **健康检查**: `/health` 和 `/alive` 端点（仅开发环境）
- ✅ **OpenTelemetry**: 自动配置日志、指标、分布式追踪
- ✅ **HTTP 弹性**: 重试、断路器、超时策略

**使用方式**:
```csharp
// Program.cs（所有服务）
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults(); // 一行代码启用

var app = builder.Build();
app.MapDefaultEndpoints();    // 映射健康检查端点
app.Run();
```

**详细文档**: [README.md](./Zss.BilliardHall.Wolverine.ServiceDefaults/README.md)

### Zss.BilliardHall.Wolverine.ServiceDefaults.Tests

**职责**: ServiceDefaults 的单元测试和集成测试。

**测试覆盖**:
- 服务发现配置
- 健康检查注册
- OpenTelemetry 配置
- HTTP 客户端弹性

**运行测试**:
```bash
cd Zss.BilliardHall.Wolverine.ServiceDefaults.Tests
dotnet test
```

---

## 📚 相关文档

### 架构文档
- [Aspire 编排架构](../../../docs/03_系统架构设计/Aspire编排架构.md) - Aspire 核心概念和架构设计
- [技术选型](../../../docs/03_系统架构设计/技术选型.md) - 为什么选择 Aspire

### 开发指南
- [ServiceDefaults 集成指南](../../../docs/06_开发规范/ServiceDefaults集成指南.md) - ServiceDefaults 详细使用说明
- [Aspire 本地开发指南](../../../docs/10_部署与运维/Aspire本地开发指南.md) - 本地环境搭建和调试

### 配置管理
- [Secrets 管理](../../../docs/08_配置管理/Secrets管理.md) - 数据库密码、API Key 管理策略

---

## 🛠️ 常见任务

### 添加新服务

1. 在 AppHost.cs 中注册服务：
   ```csharp
   builder.AddProject<Projects.MembersApi>("members-api")
       .WithReference(db)
       .WaitFor(db);
   ```

2. 服务项目中集成 ServiceDefaults：
   ```csharp
   builder.AddServiceDefaults();
   ```

### 添加新容器（如 Redis）

```csharp
var redis = builder
    .AddRedis("redis")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.Bootstrapper>("bootstrapper")
    .WithReference(db)
    .WithReference(redis)  // 注入 Redis 连接
    .WaitFor(db)
    .WaitFor(redis);
```

### 服务间通信

使用服务发现：

```csharp
// 注册 HttpClient
services.AddHttpClient("MemberService", client =>
{
    client.BaseAddress = new Uri("http+https://members-api"); // 自动解析
});

// 使用
var client = httpClientFactory.CreateClient("MemberService");
var member = await client.GetFromJsonAsync<Member>($"/api/members/{id}");
```

### 查看服务日志

1. 打开 Aspire Dashboard
2. Resources → 点击服务名称
3. 切换到 Console Tab

### 重启服务

Dashboard → Resources → 点击服务 → ⋮ → Restart

---

## 🐛 故障排查

### 问题：PostgreSQL 容器启动失败

**检查**:
1. Docker Desktop 是否运行
2. 查看容器日志：`docker logs <container-id>`
3. 检查端口占用：`netstat -ano | findstr :5432`

**解决**:
```bash
docker pull postgres:16  # 预拉取镜像
```

### 问题：服务无法连接数据库

**检查**:
1. AppHost 是否使用 `.WaitFor(db)`
2. Dashboard → Resources → postgres 状态是否 Running

**临时解决**:
Dashboard → Resources → bootstrapper → Restart

### 问题：健康检查返回 404

**原因**: 未调用 `app.MapDefaultEndpoints()`

**解决**: 在 Program.cs 中添加：
```csharp
app.MapDefaultEndpoints();
```

---

## 📊 监控和诊断

### Aspire Dashboard 功能

| Tab | 功能 | 使用场景 |
|-----|------|---------|
| **Resources** | 资源状态 | 查看所有服务、容器状态 |
| **Console** | 实时日志 | 查看服务输出 |
| **Structured Logs** | 结构化日志 | 按级别、时间筛选 |
| **Traces** | 分布式追踪 | 分析请求调用链 |
| **Metrics** | 性能指标 | 监控 CPU、内存、请求速率 |

### 诊断命令

```bash
# 查看 Aspire 容器
docker ps -a | grep aspire

# 查看数据卷
docker volume ls | grep aspire

# 验证健康检查
curl http://localhost:7001/health
curl http://localhost:7001/alive
```

---

## 🔗 外部资源

- [.NET Aspire 官方文档](https://learn.microsoft.com/dotnet/aspire/)
- [ServiceDefaults 模板](https://aka.ms/dotnet/aspire/service-defaults)
- [健康检查最佳实践](https://aka.ms/dotnet/aspire/healthchecks)
- [OpenTelemetry 集成](https://learn.microsoft.com/dotnet/aspire/fundamentals/telemetry)

---

## 版本信息

- **Aspire 版本**: 13.x
- **.NET 版本**: 10.0
- **最后更新**: 2024-01-15

---

**维护者**: 架构团队  
**问题反馈**: 提交 Issue 或联系架构团队
