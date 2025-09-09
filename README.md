# 智慧台球厅管理系统 (Smart Billiard Hall Management System)

> 构建符合"机器可读优先、人机混合协作、流程自动化"目标的GitHub Copilot指令文件体系

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![ABP Framework](https://img.shields.io/badge/ABP-9.3.2-red.svg)](https://abp.io/)
[![Blazor](https://img.shields.io/badge/Blazor-Server%20+%20WASM-blue.svg)](https://blazor.net/)
[![Aspire](https://img.shields.io/badge/.NET%20Aspire-9.4.1-orange.svg)](https://learn.microsoft.com/en-us/dotnet/aspire/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-blue.svg)](https://www.mysql.com/)
[![GitHub Copilot](https://img.shields.io/badge/GitHub-Copilot%20Optimized-green.svg)](https://copilot.github.com/)

## 项目概述 (Project Overview)

智慧台球厅管理系统是一个现代化的台球厅综合管理平台，基于 **ABP Framework 9.3.2** 和 **.NET Aspire 9.4.1** 构建，采用领域驱动设计 (DDD) 和云原生架构模式，专门为 GitHub Copilot 优化设计，实现高效的人机协作开发。

### 核心特性 (Core Features)

- 🎱 **台球桌管理** - 台球桌信息管理、状态监控、位置布局
- 📅 **预约系统** - 在线预约、时间冲突检测、自动计费
- 👥 **客户管理** - 会员系统、消费记录、等级管理
- 💰 **计费系统** - 灵活计费规则、多种支付方式
- 📊 **数据分析** - 经营报表、客户行为分析
- 🔐 **权限管理** - 角色权限、操作审计
- 📱 **移动端支持** - 响应式设计、移动应用

### 技术特色 (Technical Highlights)

- 🤖 **AI 驱动开发** - 专门为 GitHub Copilot 优化的代码结构
- 🏛️ **ABP Framework** - 领域驱动设计、多租户、权限管理
- 🌐 **Blazor 混合架构** - Server + WebAssembly 双模式支持
- ☁️ **.NET Aspire 编排** - 云原生服务发现、监控、弹性处理
- 🎨 **现代化 UI** - Blazorise + Bootstrap 5 + LeptonX Lite 主题
- 🔒 **企业级安全** - OpenIddict 认证、多租户隔离、权限控制
- 🐳 **容器化部署** - Aspire 托管、Docker 支持
- 📈 **可观测性** - OpenTelemetry 监控、健康检查、日志追踪

## GitHub Copilot 指令文件体系 (Copilot Instruction File System)

### 系统架构 (System Architecture)

```
.
├── .github/
│   └── copilot-instructions.md          # 主要 Copilot 指令文件
├── .copilot/                           # Copilot 指令文件系统
│   ├── README.md                       # 指令系统说明
│   ├── copilot.yml                     # 中央配置文件
│   ├── schemas/                        # 机器可读架构定义
│   │   ├── abp-entities.json          # ABP 实体架构
│   │   └── aspire-config.json         # Aspire 编排配置
│   ├── patterns/                       # 代码模式和约定
│   │   ├── coding-patterns.md          # ABP 代码编写模式
│   │   ├── api-patterns.md             # ABP Application Service 模式
│   │   ├── database-patterns.md        # ABP + MySQL 设计模式
│   │   ├── testing-patterns.md         # ABP 测试模式
│   │   ├── blazor-patterns.md          # Blazor 组件开发模式
│   │   └── aspire-patterns.md          # .NET Aspire 编排模式
│   ├── workflows/                      # 工作流和自动化
│   │   ├── README.md                   # 工作流说明
│   │   ├── development.md              # 开发工作流
│   │   ├── testing.md                  # 测试工作流
│   │   ├── deployment.md               # 部署工作流
│   │   └── maintenance.md              # 维护工作流
│   └── templates/                      # 代码生成模板
│       ├── abp-application-service-template.md  # ABP 应用服务模板
│       ├── service-template.md          # 领域服务模板
│       ├── repository-template.md       # 仓储模板
│       └── component-template.md        # Blazor 组件模板
└── src/                               # ABP 项目结构
    ├── Zss.BilliardHall.Domain.Shared/  # 共享领域
    ├── Zss.BilliardHall.Domain/        # 领域层
    ├── Zss.BilliardHall.Application.Contracts/  # 应用契约
    ├── Zss.BilliardHall.Application/   # 应用服务层
    ├── Zss.BilliardHall.EntityFrameworkCore/  # 数据访问层
    ├── Zss.BilliardHall.HttpApi/       # HTTP API 层
    ├── Zss.BilliardHall.HttpApi.Client/  # API 客户端
    ├── Zss.BilliardHall.Blazor/        # Blazor Server 主机
    ├── Zss.BilliardHall.Blazor.Client/ # Blazor WebAssembly 客户端
    ├── Zss.BilliardHall.DbMigrator/    # 数据库迁移工具
    ├── Zss.BilliardHall.AppHost/       # .NET Aspire 应用主机
    └── Zss.BilliardHall.ServiceDefaults/ # Aspire 服务默认配置
```

### 核心设计原则 (Core Design Principles)

#### 1. 机器可读优先 (Machine-Readable First)
- 📋 **结构化数据格式** - 使用 JSON Schema 定义所有数据结构
- 🏷️ **标准化命名约定** - 一致的命名规则，便于 AI 理解
- 📝 **详细的类型定义** - 完整的接口和数据类型规范
- 🔍 **上下文感知注释** - 提供丰富的上下文信息

#### 2. 人机混合协作 (Human-AI Collaboration)
- 🤝 **渐进式增强** - 支持人工干预和 AI 自动化的结合
- 📖 **可读性优先** - 代码既要机器友好，也要人类可读
- 🎯 **意图明确** - 清晰表达业务意图和设计决策
- 🔄 **迭代优化** - 支持持续改进和学习

#### 3. 流程自动化 (Process Automation)
- ⚙️ **自动化工作流** - CI/CD、测试、部署全自动化
- 🛠️ **代码生成** - 基于模板的自动代码生成
- 📊 **质量保证** - 自动化代码审查和质量检查
- 📈 **持续监控** - 自动化监控和告警机制

## 快速开始 (Quick Start)

### 环境要求 (Prerequisites)

- .NET 9.0 SDK
- MySQL 8.0 或更高版本
- Redis (用于缓存和分布式锁)
- Docker Desktop (可选，用于 Aspire 编排)
- Visual Studio 2022 或 JetBrains Rider (推荐)
- ABP CLI (可选，用于代码生成)

### 安装步骤 (Installation)

1. **克隆仓库**
   ```bash
   git clone https://github.com/douhuaa/Zss.BilliardHall.git
   cd Zss.BilliardHall
   ```

2. **使用 .NET Aspire 快速启动（推荐）**
   ```bash
   # 确保 Docker Desktop 正在运行
   # 运行 Aspire AppHost，将自动启动所有服务
   dotnet run --project src/Zss.BilliardHall.AppHost
   ```

3. **手动设置（可选）**
   ```bash
   # 安装依赖包
   dotnet restore
   
   # 配置数据库连接字符串（在 appsettings.json 中）
   # "Default": "Server=localhost;Database=BilliardHall;Uid=root;Pwd=yourpassword;"
   
   # 运行数据库迁移
   dotnet run --project src/Zss.BilliardHall.DbMigrator
   
   # 启动 Blazor 应用
   dotnet run --project src/Zss.BilliardHall.Blazor
   ```

4. **访问应用**
   - 🌐 Blazor 应用: https://localhost:7136
   - 📖 Swagger API 文档: https://localhost:7136/swagger
   - 📊 Aspire Dashboard: https://localhost:15888 (使用 Aspire 时)
   - 🩺 健康检查: https://localhost:7136/health-ui

## GitHub Copilot 使用指南 (Copilot Usage Guide)

### 代码生成示例 (Code Generation Examples)

#### 1. 创建新实体
```
// Copilot 提示词
基于 abp-entities.json 架构为台球厅会员系统创建 Member 实体，使用 ABP FullAuditedAggregateRoot 基类，包括会员等级、积分、有效期等属性，支持多租户
```

#### 2. 生成 ABP Application Service
```
// Copilot 提示词  
根据 abp-application-service-template.md 为 Member 实体创建完整的应用服务，包括权限控制、DTO 映射、分页查询和业务逻辑
```

#### 3. 创建 Blazor 组件
```
// Copilot 提示词
基于 blazor-patterns.md 创建会员管理的 Blazorise 数据表格组件，支持搜索、分页、排序和 CRUD 操作，使用 LeptonX Lite 主题
```

#### 4. 数据库设计
```
// Copilot 提示词
根据 database-patterns.md 为会员积分系统设计 MySQL 数据表结构，使用 ABP Entity Framework Core 配置，包括索引优化和多租户支持
```

#### 5. .NET Aspire 服务配置
```
// Copilot 提示词
基于 aspire-patterns.md 在 AppHost 中配置新的微服务，包括服务发现、健康检查、监控和弹性处理
```

### 最佳实践 (Best Practices)

1. **使用结构化提示** - 引用具体的 ABP 模板和模式文件
2. **提供业务上下文** - 描述具体的台球厅业务场景和需求
3. **遵循 ABP 约定** - 使用 ABP 框架的命名规范和架构模式
4. **包含权限控制** - 确保生成的代码包含 ABP 权限验证
5. **支持多租户** - 考虑多租户隔离和数据过滤
6. **添加相应测试** - 使用 ABP 测试基础设施编写测试

## 项目结构 (Project Structure)

### ABP 分层架构 (ABP Layered Architecture)

```
src/
├── Zss.BilliardHall.Domain.Shared/     # 共享领域
│   ├── Enums/                          # 枚举定义
│   ├── Consts/                         # 常量定义
│   └── Localization/                   # 本地化资源
├── Zss.BilliardHall.Domain/            # 领域层
│   ├── Entities/                       # 领域实体 (继承 ABP 基类)
│   ├── ValueObjects/                   # 值对象
│   ├── Services/                       # 领域服务
│   ├── Repositories/                   # 仓储接口
│   └── Events/                         # 领域事件
├── Zss.BilliardHall.Application.Contracts/  # 应用契约层
│   ├── DTOs/                          # 数据传输对象
│   ├── Services/                      # 应用服务接口
│   └── Permissions/                   # 权限定义
├── Zss.BilliardHall.Application/       # 应用层
│   ├── Services/                       # 应用服务实现
│   ├── AutoMapper/                     # 对象映射配置
│   └── Validators/                     # 输入验证器
├── Zss.BilliardHall.EntityFrameworkCore/  # 数据访问层
│   ├── EntityConfigurations/           # 实体配置
│   ├── Repositories/                   # 仓储实现
│   ├── Migrations/                     # 数据库迁移
│   └── BilliardHallDbContext.cs       # DbContext
├── Zss.BilliardHall.HttpApi/           # HTTP API 层
│   ├── Controllers/                    # ABP 自动 API 控制器
│   └── BilliardHallController.cs       # 自定义控制器
└── Zss.BilliardHall.Blazor/           # 表示层
    ├── Components/                     # Blazor 组件
    ├── Pages/                         # 页面组件
    ├── Menus/                         # 菜单配置
    └── BilliardHallComponentBase.cs   # 组件基类
```

### .NET Aspire 编排架构 (Aspire Orchestration)

```
src/
├── Zss.BilliardHall.AppHost/          # Aspire 应用主机
│   ├── AppHost.cs                     # 服务编排配置
│   └── appsettings.json               # Aspire 配置
├── Zss.BilliardHall.ServiceDefaults/  # 服务默认配置
│   ├── Extensions.cs                  # 通用服务配置
│   └── HealthChecks/                  # 健康检查实现
└── Infrastructure Services/            # 基础设施服务
    ├── MySQL Database                  # 数据库服务
    ├── Redis Cache                     # 缓存服务
    ├── Jaeger Tracing                  # 分布式追踪
    └── Prometheus Metrics              # 指标收集
```

## 开发工作流 (Development Workflow)

### 日常开发流程 (Daily Development Flow)

1. **创建功能分支**
   ```bash
   git checkout -b feature/member-management
   ```

2. **使用 Copilot 生成代码**
   - 基于模板生成实体、服务、控制器
   - 自动生成测试代码
   - 创建数据库迁移

3. **本地测试和验证**
   ```bash
   # 运行 ABP 单元测试
   dotnet test test/Zss.BilliardHall.Application.Tests
   
   # 运行 ABP 集成测试
   dotnet test test/Zss.BilliardHall.EntityFrameworkCore.Tests
   
   # 使用 ABP CLI 代码分析
   abp lint
   
   # .NET 代码格式化
   dotnet format
   ```

4. **提交代码**
   ```bash
   git add .
   git commit -m "feat: 添加会员管理功能"
   git push origin feature/member-management
   ```

5. **创建 Pull Request**
   - 自动触发 CI/CD 流水线
   - 代码审查和质量检查
   - 自动部署到测试环境

### 自动化流程 (Automated Workflows)

- 🔄 **持续集成** - GitHub Actions + ABP 自动化测试
- 🚀 **持续部署** - Aspire 编排自动部署到各环境
- 📊 **质量监控** - SonarQube 代码分析、测试覆盖率
- 🔒 **安全扫描** - Dependabot、CodeQL 安全检查
- 📈 **性能监控** - OpenTelemetry 指标、Aspire Dashboard

## API 文档 (API Documentation)

### 接口概览 (API Overview)

| 模块 | 端点 | 描述 |
|------|------|------|
| 台球厅管理 | `/api/app/billiard-halls` | ABP 应用服务自动 API |
| 台球桌管理 | `/api/app/billiard-tables` | 台球桌管理和状态监控 |
| 客户管理 | `/api/app/customers` | 客户信息和会员管理 |
| 预约系统 | `/api/app/reservations` | 预约创建、查询、管理 |
| 计费系统 | `/api/app/billing` | 计费规则和支付管理 |
| 身份管理 | `/api/identity` | ABP Identity 模块 API |
| 权限管理 | `/api/permission-management` | ABP 权限管理 API |
| 租户管理 | `/api/multi-tenancy` | ABP 多租户管理 API |

### 示例请求 (Example Requests)

#### 创建台球桌
```http
POST /api/app/billiard-tables
Content-Type: application/json
Authorization: Bearer {token}

{
  "number": 5,
  "type": 0,  // BilliardTableType.ChineseEightBall
  "hourlyRate": 35.00,
  "locationX": 10.5,
  "locationY": 5.2,
  "billiardHallId": "123e4567-e89b-12d3-a456-426614174000"
}
```

#### 查询台球桌列表 (支持 ABP 动态查询)
```http
GET /api/app/billiard-tables?Status=1&Type=0&MaxResultCount=10&SkipCount=0&Sorting=Number
Authorization: Bearer {token}
```

#### 创建预约
```http
POST /api/app/reservations
Content-Type: application/json
Authorization: Bearer {token}

{
  "customerId": "987fcdeb-51d2-43a1-8765-123456789abc",
  "billiardTableId": "123e4567-e89b-12d3-a456-426614174000",
  "startTime": "2023-12-01T14:00:00Z",
  "durationMinutes": 120,
  "notes": "VIP客户预约"
}
```

#### ABP 权限检查
```http
GET /api/permission-management/permissions?providerName=R&providerKey=admin
Authorization: Bearer {token}
```

## 测试策略 (Testing Strategy)

### ABP 测试基础设施 (ABP Test Infrastructure)

- **单元测试** - ABP 领域和应用服务测试，使用 ABP TestBase
- **集成测试** - EF Core + MySQL 集成测试，使用 ABP 测试容器
- **Web API 测试** - HTTP API 测试，包括权限和多租户验证
- **Blazor 组件测试** - bUnit 组件测试框架

### 运行测试 (Running Tests)

```bash
# 运行所有 ABP 测试
dotnet test

# 运行特定测试项目
dotnet test test/Zss.BilliardHall.Domain.Tests
dotnet test test/Zss.BilliardHall.Application.Tests  
dotnet test test/Zss.BilliardHall.EntityFrameworkCore.Tests
dotnet test test/Zss.BilliardHall.HttpApi.Tests

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"

# 使用 ABP CLI 运行测试
abp test
```

## 部署指南 (Deployment Guide)

### .NET Aspire 部署 (Aspire Deployment)

```bash
# 使用 Aspire 本地开发
dotnet run --project src/Zss.BilliardHall.AppHost

# 生成 Aspire 清单文件
dotnet run --project src/Zss.BilliardHall.AppHost -- --publisher manifest --output-path ../aspire-manifest.json

# 发布到 Azure Container Apps
azd provision
azd deploy
```

### Docker 容器化部署 (Container Deployment)

```bash
# 构建 ABP 应用镜像
dotnet publish src/Zss.BilliardHall.Blazor -c Release
docker build -t billiard-hall-blazor:latest -f src/Zss.BilliardHall.Blazor/Dockerfile .

# 运行容器
docker run -d \
  --name billiard-hall-blazor \
  -p 8080:8080 \
  -e ConnectionStrings__Default="Server=mysql;Database=BilliardHall;Uid=root;Pwd=yourpassword;" \
  -e ConnectionStrings__Redis="redis:6379" \
  billiard-hall-blazor:latest
```

### Kubernetes 部署 (Kubernetes Deployment)

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: billiard-hall-blazor
spec:
  replicas: 3
  selector:
    matchLabels:
      app: billiard-hall-blazor
  template:
    spec:
      containers:
      - name: blazor-app
        image: billiard-hall-blazor:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__Default
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
```

## 监控和运维 (Monitoring & Operations)

### 健康检查 (Health Checks)

- `/health-ui` - ABP 健康检查 UI 界面
- `/health` - 应用整体健康状态 (JSON)
- `/health/ready` - 应用就绪状态
- `/health/live` - 应用存活状态

### 监控指标 (Monitoring Metrics)

- **ABP 审计日志** - 用户操作、实体变更、异常记录
- **性能指标** - OpenTelemetry 追踪、响应时间、吞吐量
- **业务指标** - 预约数量、收入统计、用户活跃度
- **基础设施指标** - CPU、内存、数据库连接数

### Aspire 可观测性 (Aspire Observability)

- **Aspire Dashboard** - 统一监控面板 (https://localhost:15888)
- **分布式追踪** - Jaeger/OpenTelemetry 集成
- **日志聚合** - Serilog + Aspire 日志收集
- **指标收集** - Prometheus + Grafana 集成

### 日志管理 (Log Management)

```json
{
  "timestamp": "2023-12-01T10:30:00Z",
  "level": "Information", 
  "template": "创建台球桌预约 {ReservationId} 用户 {UserId}",
  "message": "创建台球桌预约 123e4567 用户 user-67890",
  "properties": {
    "tenantId": "tenant-123",
    "userId": "user-67890", 
    "reservationId": "123e4567-e89b-12d3-a456-426614174000",
    "billiardTableId": "987fcdeb-51d2-43a1-8765-123456789abc",
    "durationMinutes": 120,
    "totalAmount": 70.00,
    "auditInfo": {
      "creationTime": "2023-12-01T10:30:00Z",
      "creatorId": "user-67890"
    }
  },
  "requestId": "req-12345",
  "traceId": "trace-67890",
  "spanId": "span-abcde"
}
```

## 贡献指南 (Contributing)

### 贡献流程 (Contribution Process)

1. Fork 项目仓库
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 使用 GitHub Copilot 和项目模板生成代码
4. 添加测试并确保通过
5. 提交变更 (`git commit -m 'feat: 添加新功能'`)
6. 推送到分支 (`git push origin feature/amazing-feature`)
7. 创建 Pull Request

### 代码规范 (Code Standards)

- 遵循 `.copilot/patterns/` 中定义的 ABP 编码模式
- 使用 ABP CLI 和 ABP Suite 代码生成工具
- 遵循 ABP Framework 命名约定和架构模式
- 使用 EditorConfig 保证代码格式一致性
- 编写清晰的 XML 文档注释
- 为新功能添加相应的 ABP 测试

### 提交信息规范 (Commit Message Convention)

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

类型 (Types):
- `feat`: 新功能
- `fix`: Bug 修复
- `docs`: 文档更新
- `style`: 代码格式调整
- `refactor`: 代码重构
- `test`: 测试相关
- `chore`: 构建过程或辅助工具的变动

## 许可证 (License)

本项目基于 MIT 许可证开源 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 支持与反馈 (Support & Feedback)

- 📧 **邮件支持**: dev@billiard-hall.com
- 🐛 **问题报告**: [GitHub Issues](https://github.com/douhuaa/Zss.BilliardHall/issues)
- 💡 **功能建议**: [GitHub Discussions](https://github.com/douhuaa/Zss.BilliardHall/discussions)
- 📖 **文档**: [项目文档](https://docs.billiard-hall.com)

## 致谢 (Acknowledgments)

- 感谢 GitHub Copilot 团队提供的强大 AI 编程助手
- 感谢所有贡献者和社区成员的支持
- 特别感谢台球行业专家提供的业务指导

---



**🎱 让 AI 助力台球厅管理现代化 🤖**

[开始使用](https://github.com/douhuaa/Zss.BilliardHall/blob/main/docs/getting-started.md) • 
[API 文档](https://api.billiard-hall.com/swagger) • 
[演示站点](https://demo.billiard-hall.com) • 
[视频教程](https://www.youtube.com/playlist?list=billiard-hall-tutorials)

