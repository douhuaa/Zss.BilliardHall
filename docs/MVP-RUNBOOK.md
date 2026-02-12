# Zss.BilliardHall MVP 运行指南

本文档说明如何在本地环境运行 Zss.BilliardHall MVP（最小可行产品）版本。

## 先决条件

### 必需
- **.NET 10 SDK** 或更高版本
- **PostgreSQL 15+** 数据库服务器

### 可选
- **Docker** (如果使用 Docker 运行 PostgreSQL)
- **User Secrets** 用于本地开发配置

---

## 快速开始

### 1. 启动 PostgreSQL 数据库

#### 方式 A：使用 Docker (推荐用于本地开发)

```bash
docker run --name postgres-zss \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_DB=zss_billiard_hall \
  -p 5432:5432 \
  -d postgres:16-alpine
```

#### 方式 B：使用本地安装的 PostgreSQL

确保 PostgreSQL 服务正在运行，并创建数据库：

```sql
CREATE DATABASE zss_billiard_hall;
```

### 2. 配置数据库连接字符串

#### 方式 A：使用默认配置（开发环境）

默认配置已经设置为：
```
Host=localhost;Port=5432;Database=zss_billiard_hall;Username=postgres;Password=postgres
```

如果您的数据库配置与默认值相同，可以跳过此步骤。

#### 方式 B：使用 User Secrets（推荐）

为 Web Host 配置：
```bash
cd src/Host/Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=zss_billiard_hall;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
```

为 Worker Host 配置：
```bash
cd src/Host/Worker
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=zss_billiard_hall;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
```

#### 方式 C：修改 appsettings.Development.json

编辑 `src/Host/Web/appsettings.Development.json` 和 `src/Host/Worker/appsettings.Development.json`，更新连接字符串。

⚠️ **警告**: 不要将敏感信息提交到版本控制！

---

## 运行应用程序

### 启动 Web Host

```bash
cd src/Host/Web
dotnet run
```

或从解决方案根目录：
```bash
dotnet run --project src/Host/Web/Web.csproj
```

**Web Host 将启动在**: `http://localhost:5000` 和 `https://localhost:5001`

### 启动 Worker Host

```bash
cd src/Host/Worker
dotnet run
```

或从解决方案根目录：
```bash
dotnet run --project src/Host/Worker/Worker.csproj
```

Worker 将在后台运行并每秒记录一次日志。

---

## 验证运行状态

### 健康检查端点

访问健康检查端点验证 Web Host 正常运行：

```bash
curl http://localhost:5000/health
```

预期响应：
```json
{
  "status": "healthy",
  "timestamp": "2026-02-12T16:00:00.000Z"
}
```

### 创建会员示例

```bash
curl -X POST http://localhost:5000/api/members \
  -H "Content-Type: application/json" \
  -d '{
    "name": "张三",
    "email": "zhangsan@example.com",
    "phoneNumber": "13800138000"
  }'
```

预期响应（201 Created）：
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 查询会员

使用创建时返回的 ID 查询会员：

```bash
curl http://localhost:5000/api/members/550e8400-e29b-41d4-a716-446655440000
```

预期响应（200 OK）：
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "张三",
  "email": "zhangsan@example.com",
  "phoneNumber": "13800138000",
  "createdAt": "2026-02-12T16:00:00.000Z"
}
```

---

## 构建与测试

### 构建整个解决方案

```bash
dotnet build
```

### 运行架构测试

```bash
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj
```

### 运行所有测试

```bash
dotnet test
```

---

## 架构概览

### 三层启动体系

遵循 **ADR-002** 定义的三层启动体系：

1. **Platform** (技术基座)
   - 提供日志、追踪、序列化等技术能力
   - 不感知业务领域

2. **Application** (应用装配层)
   - 配置 Wolverine + Marten
   - 通过 ModuleLoader 发现并加载业务模块

3. **Host** (进程外壳)
   - Web Host: ASP.NET Core Web 应用
   - Worker Host: 后台服务

### 模块注册机制

采用 **Marker Interface + 反射** 方式（B 方案）：

1. Host 显式提供模块程序集清单（在 Program.cs 中）
2. ApplicationBootstrapper 调用 ModuleLoader 加载模块
3. ModuleLoader 通过反射发现 `IModuleBootstrapper` 实现并调用其 `Configure` 方法

### 垂直切片架构

业务模块（如 Members）按用例组织：
```
Modules/Members/
├── Features/
│   ├── CreateMember/
│   │   ├── CreateMemberCommand.cs       # 命令
│   │   ├── CreateMemberCommandHandler.cs # Handler
│   │   ├── CreateMemberEndpoint.cs      # HTTP 端点
│   │   ├── CreateMemberResponse.cs      # 响应 DTO
│   │   └── Member.cs                    # 实体
│   └── GetMemberById/
│       ├── GetMemberByIdQuery.cs
│       ├── GetMemberByIdQueryHandler.cs
│       ├── GetMemberByIdEndpoint.cs
│       └── MemberDto.cs
├── MembersBootstrapper.cs               # 模块启动器
└── ModuleMarker.cs                      # 模块标记类
```

---

## 常见问题

### Q: 数据库连接失败？

**A**: 检查以下事项：
1. PostgreSQL 服务是否正在运行
2. 连接字符串是否正确
3. 数据库是否存在
4. 用户名和密码是否正确
5. 端口 5432 是否被占用

### Q: Marten 报错 "relation does not exist"？

**A**: 在开发环境，Marten 会自动创建表。如果手动删除了表，重启应用程序即可自动重建。

### Q: 架构测试失败？

**A**: 
1. 确保代码更改符合 ADR 约束
2. 查看失败测试的详细错误信息
3. 参考对应的 ADR 文档（docs/adr/）

### Q: 如何添加新模块？

**A**:
1. 在 `src/Modules/` 下创建新模块目录
2. 创建 `ModuleMarker.cs` 和 `XxxBootstrapper.cs`
3. 在 Host 的 Program.cs 中添加模块程序集到清单
4. 按垂直切片组织业务用例

---

## 相关文档

- [ADR-001: 模块化单体与垂直切片架构](../../docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [ADR-002: Platform / Application / Host 三层启动体系](../../docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md)
- [架构测试说明](../../src/tests/ArchitectureTests/README.md)

---

## 技术栈

- **.NET 10**: 应用程序框架
- **Wolverine**: CQRS/中介者模式实现
- **Marten**: PostgreSQL 文档数据库
- **Serilog**: 结构化日志
- **OpenTelemetry**: 可观测性（追踪、指标）
