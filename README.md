# Zss.BilliardHall - 自助台球系统

基于 ABP Framework 的无人自助台球管理系统。

## 技术栈

- **框架**: ABP Framework 9.3.5
- **前端**: Blazor Web App (.NET 9)
- **后端**: ASP.NET Core Web API (.NET 9)
- **ORM**: Entity Framework Core 9.0
- **数据库**: PostgreSQL
- **测试框架**: xUnit

## 项目结构

```
Zss.BilliardHall/
├── src/
│   ├── Zss.BilliardHall.Domain.Shared/        # 共享的领域层定义（常量、枚举等）
│   ├── Zss.BilliardHall.Domain/               # 领域层（实体、领域服务）
│   ├── Zss.BilliardHall.Application.Contracts/ # 应用服务接口和 DTO
│   ├── Zss.BilliardHall.Application/          # 应用服务实现
│   ├── Zss.BilliardHall.EntityFrameworkCore/  # EF Core 数据访问层
│   ├── Zss.BilliardHall.HttpApi/              # HTTP API 控制器
│   ├── Zss.BilliardHall.HttpApi.Host/         # Web API 宿主项目
│   └── Zss.BilliardHall.Blazor/               # Blazor 前端应用
├── test/
│   └── Zss.BilliardHall.Application.Tests/    # 单元测试项目
└── doc/                                        # 项目文档

```

## ABP Framework 分层架构

本项目遵循 ABP Framework 的领域驱动设计（DDD）分层架构：

### Domain.Shared
包含常量、枚举和其他类型，可以在解决方案中的所有项目之间安全共享。

### Domain
包含实体、聚合根、领域服务、值对象、仓储接口和其他领域对象。

### Application.Contracts
包含应用服务接口和数据传输对象（DTO）。

### Application
包含应用服务的实现。

### EntityFrameworkCore
实现 EF Core 数据库配置、仓储实现和 DbContext。

### HttpApi
包含 API 控制器。

### HttpApi.Host
托管 Web API，包含 Startup 配置。

### Blazor
Blazor WebAssembly 或 Blazor Server 应用程序。

## 快速开始

### 前置要求

- .NET 9 SDK
- PostgreSQL 12 或更高版本
- ABP CLI（可选）

### 安装 ABP CLI（可选）

```bash
dotnet tool install -g Volo.Abp.Cli
```

### 数据库配置

1. 确保 PostgreSQL 已安装并运行
2. 在 `src/Zss.BilliardHall.HttpApi.Host/appsettings.json` 中配置数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=ZssBilliardHall;Username=postgres;Password=your_password"
  }
}
```

### 运行项目

#### 方式1：运行 API 和 Blazor（推荐用于开发）

```bash
# 运行 API Host
cd src/Zss.BilliardHall.HttpApi.Host
dotnet run

# 在另一个终端运行 Blazor
cd src/Zss.BilliardHall.Blazor
dotnet run
```

#### 方式2：使用 Docker Compose（生产环境）

```bash
docker-compose up -d
```

### 数据库迁移

```bash
cd src/Zss.BilliardHall.EntityFrameworkCore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 运行测试

```bash
cd test/Zss.BilliardHall.Application.Tests
dotnet test
```

## 开发指南

### 添加新实体

1. 在 `Domain` 项目中创建实体类
2. 在 `EntityFrameworkCore` 项目的 DbContext 中添加 DbSet
3. 创建并应用迁移

### 添加应用服务

1. 在 `Application.Contracts` 项目中定义接口和 DTO
2. 在 `Application` 项目中实现服务
3. 在 `HttpApi` 项目中创建控制器（可选，ABP 可以自动生成）

### Swagger API 文档

API 文档在开发模式下可访问：
- URL: `https://localhost:5001/swagger`

## 文档

更多详细文档请参考 `doc/` 目录：

- [项目概述](doc/01_项目概述/README.md)
- [系统架构设计](doc/03_系统架构设计/README.md)
- [数据库设计](doc/05_数据库设计/README.md)

## Git 工作流

本项目采用 **GitHub Flow** 分支模型：

1. `main` 分支：生产就绪代码
2. 功能分支：从 `main` 创建，命名格式 `feature/功能名称`
3. 完成后通过 Pull Request 合并回 `main`

## 许可证

[待定]

## 联系方式

[待定]
