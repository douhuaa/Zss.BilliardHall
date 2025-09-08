# 智慧台球厅管理系统 (Smart Billiard Hall Management System)

> 构建符合"机器可读优先、人机混合协作、流程自动化"目标的GitHub Copilot指令文件体系

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18.0-blue.svg)](https://reactjs.org/)
[![GitHub Copilot](https://img.shields.io/badge/GitHub-Copilot%20Optimized-green.svg)](https://copilot.github.com/)

## 项目概述 (Project Overview)

智慧台球厅管理系统是一个现代化的台球厅综合管理平台，采用先进的软件架构和开发模式，专门为 GitHub Copilot 优化设计，实现高效的人机协作开发。

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
- 🏗️ **现代架构** - 清洁架构、微服务、事件驱动
- 🚀 **高性能** - 缓存策略、数据库优化、异步处理
- 🔒 **安全可靠** - JWT 认证、数据加密、安全审计
- 🐳 **容器化部署** - Docker、Kubernetes 支持
- 📈 **可观测性** - 日志、监控、告警、追踪

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
│   │   ├── entities.json              # 业务实体架构
│   │   ├── api-responses.json          # API 响应格式
│   │   └── database-schema.json        # 数据库架构
│   ├── patterns/                       # 代码模式和约定
│   │   ├── coding-patterns.md          # 代码编写模式
│   │   ├── api-patterns.md             # API 设计模式
│   │   ├── database-patterns.md        # 数据库设计模式
│   │   ├── testing-patterns.md         # 测试模式
│   │   ├── frontend-patterns.md        # 前端开发模式
│   │   └── security-patterns.md        # 安全模式
│   ├── workflows/                      # 工作流和自动化
│   │   ├── README.md                   # 工作流说明
│   │   ├── development.md              # 开发工作流
│   │   ├── testing.md                  # 测试工作流
│   │   ├── deployment.md               # 部署工作流
│   │   └── maintenance.md              # 维护工作流
│   └── templates/                      # 代码生成模板
│       ├── controller-template.md       # 控制器模板
│       ├── service-template.md          # 服务层模板
│       ├── repository-template.md       # 数据访问模板
│       └── component-template.md        # 前端组件模板
└── src/                               # 源代码目录
    ├── Zss.BilliardHall.Domain/      # 域模型层
    ├── Zss.BilliardHall.Application/ # 应用服务层
    ├── Zss.BilliardHall.Infrastructure/ # 基础设施层
    └── Zss.BilliardHall.Api/         # API 层
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

- .NET 8.0 SDK
- Node.js 18.x
- SQL Server 2022 或 LocalDB
- Redis (可选，用于缓存)
- Docker Desktop (可选，用于容器化部署)

### 安装步骤 (Installation)

1. **克隆仓库**
   ```bash
   git clone https://github.com/douhuaa/Zss.BilliardHall.git
   cd Zss.BilliardHall
   ```

2. **自动化环境设置**
   ```bash
   # 使用自动化脚本设置开发环境
   chmod +x scripts/setup-dev.sh
   ./scripts/setup-dev.sh
   ```

3. **手动设置（可选）**
   ```bash
   # 安装后端依赖
   dotnet restore
   
   # 安装前端依赖
   npm install
   
   # 数据库迁移
   dotnet ef database update --project src/Zss.BilliardHall.Infrastructure
   
   # 启动后端服务
   dotnet run --project src/Zss.BilliardHall.Api
   
   # 启动前端服务
   npm run dev
   ```

4. **访问应用**
   - 🌐 前端应用: https://localhost:3000
   - 📖 API 文档: https://localhost:5001/swagger
   - 📊 健康检查: https://localhost:5001/health

## GitHub Copilot 使用指南 (Copilot Usage Guide)

### 代码生成示例 (Code Generation Examples)

#### 1. 创建新实体
```
// Copilot 提示词
基于 entities.json 架构为台球厅会员系统创建 Member 实体，包括会员等级、积分、有效期等属性
```

#### 2. 生成 API 控制器
```
// Copilot 提示词  
根据 controller-template.md 为 Member 实体创建完整的 RESTful API 控制器，包括 CRUD 操作和批量处理
```

#### 3. 创建前端组件
```
// Copilot 提示词
基于 component-template.md 创建会员管理的数据表格组件，支持搜索、分页、排序和导出功能
```

#### 4. 数据库设计
```
// Copilot 提示词
根据 database-patterns.md 为会员积分系统设计数据表结构，包括积分获取、消费、过期等业务逻辑
```

### 最佳实践 (Best Practices)

1. **使用结构化提示** - 引用具体的模板和模式文件
2. **提供业务上下文** - 描述具体的业务场景和需求
3. **遵循命名约定** - 使用项目定义的命名规范
4. **包含测试代码** - 要求生成对应的单元测试
5. **考虑错误处理** - 确保生成的代码包含适当的异常处理

## 项目结构 (Project Structure)

### 后端架构 (Backend Architecture)

```
src/
├── Zss.BilliardHall.Domain/           # 领域层
│   ├── Entities/                      # 领域实体
│   ├── ValueObjects/                  # 值对象
│   ├── Enums/                        # 枚举类型
│   ├── Interfaces/                   # 领域接口
│   └── Services/                     # 领域服务
├── Zss.BilliardHall.Application/      # 应用层
│   ├── DTOs/                         # 数据传输对象
│   ├── Services/                     # 应用服务
│   ├── Validators/                   # 验证器
│   ├── Mappers/                      # 对象映射
│   └── Queries/                      # 查询对象
├── Zss.BilliardHall.Infrastructure/   # 基础设施层
│   ├── Data/                         # 数据访问
│   ├── Repositories/                 # 仓储实现
│   ├── ExternalServices/             # 外部服务
│   ├── Caching/                      # 缓存实现
│   └── Configuration/                # 配置管理
└── Zss.BilliardHall.Api/             # API 层
    ├── Controllers/                  # 控制器
    ├── Middleware/                   # 中间件
    ├── Filters/                      # 过滤器
    ├── Models/                       # API 模型
    └── Extensions/                   # 扩展方法
```

### 前端架构 (Frontend Architecture)

```
frontend/
├── src/
│   ├── components/                   # 可复用组件
│   ├── pages/                        # 页面组件
│   ├── hooks/                        # 自定义 Hooks
│   ├── services/                     # API 服务
│   ├── stores/                       # 状态管理
│   ├── utils/                        # 工具函数
│   ├── types/                        # TypeScript 类型定义
│   └── styles/                       # 样式文件
├── public/                           # 静态资源
└── tests/                           # 测试文件
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
   # 运行单元测试
   dotnet test --filter "Category=Unit"
   
   # 运行集成测试
   dotnet test --filter "Category=Integration"
   
   # 代码质量检查
   dotnet format
   dotnet analyzer
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

- 🔄 **持续集成** - 自动构建、测试、代码分析
- 🚀 **持续部署** - 自动部署到各个环境
- 📊 **质量监控** - 代码覆盖率、性能指标
- 🔒 **安全扫描** - 依赖漏洞、代码安全检查

## API 文档 (API Documentation)

### 接口概览 (API Overview)

| 模块 | 端点 | 描述 |
|------|------|------|
| 台球厅管理 | `/api/v1/billiard-halls` | 台球厅 CRUD 操作 |
| 台球桌管理 | `/api/v1/billiard-tables` | 台球桌管理和状态监控 |
| 客户管理 | `/api/v1/customers` | 客户信息和会员管理 |
| 预约系统 | `/api/v1/reservations` | 预约创建、查询、管理 |
| 计费系统 | `/api/v1/billing` | 计费规则和支付管理 |
| 报表分析 | `/api/v1/reports` | 经营数据和分析报表 |

### 示例请求 (Example Requests)

#### 创建台球桌
```http
POST /api/v1/billiard-tables
Content-Type: application/json

{
  "number": 5,
  "type": "Chinese8Ball",
  "hourlyRate": 35.00,
  "locationX": 10.5,
  "locationY": 5.2,
  "floor": 1,
  "zone": "A",
  "hallId": "123e4567-e89b-12d3-a456-426614174000"
}
```

#### 查询台球桌列表
```http
GET /api/v1/billiard-tables?status=Available&type=Chinese8Ball&page=1&pageSize=10
```

#### 创建预约
```http
POST /api/v1/reservations
Content-Type: application/json

{
  "customerId": "987fcdeb-51d2-43a1-8765-123456789abc",
  "tableId": "123e4567-e89b-12d3-a456-426614174000",
  "startTime": "2023-12-01T14:00:00Z",
  "endTime": "2023-12-01T16:00:00Z",
  "notes": "VIP客户预约"
}
```

## 测试策略 (Testing Strategy)

### 测试金字塔 (Test Pyramid)

- **单元测试 (90% 覆盖率)** - 业务逻辑、实体、服务层测试
- **集成测试 (70% 覆盖率)** - API、数据库、外部服务集成测试
- **端到端测试 (关键流程)** - 用户场景和业务流程测试

### 运行测试 (Running Tests)

```bash
# 运行所有测试
./scripts/run-all-tests.sh

# 运行特定类型的测试
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Performance"

# 生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

## 部署指南 (Deployment Guide)

### Docker 部署 (Docker Deployment)

```bash
# 构建镜像
docker build -t billiard-hall-api:latest .

# 运行容器
docker run -d \
  --name billiard-hall-api \
  -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e Redis__ConnectionString="..." \
  billiard-hall-api:latest
```

### Kubernetes 部署 (Kubernetes Deployment)

```yaml
# k8s/deployment.yaml
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

## 监控和运维 (Monitoring & Operations)

### 健康检查 (Health Checks)

- `/health` - 应用整体健康状态
- `/health/ready` - 应用就绪状态
- `/health/live` - 应用存活状态

### 监控指标 (Monitoring Metrics)

- **性能指标** - 响应时间、吞吐量、错误率
- **业务指标** - 预约数量、收入统计、用户活跃度
- **基础设施指标** - CPU、内存、磁盘、网络使用情况

### 日志管理 (Log Management)

```json
{
  "timestamp": "2023-12-01T10:30:00Z",
  "level": "Information",
  "message": "创建台球桌预约",
  "properties": {
    "customerId": "123e4567-e89b-12d3-a456-426614174000",
    "tableId": "987fcdeb-51d2-43a1-8765-123456789abc",
    "duration": 120,
    "amount": 70.00
  },
  "requestId": "req-12345",
  "userId": "user-67890"
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

- 遵循 `.copilot/patterns/` 中定义的编码模式
- 使用 EditorConfig 和 .NET Format 保证代码格式一致
- 编写清晰的注释和文档
- 为新功能添加相应的测试

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

<div align="center">

**🎱 让 AI 助力台球厅管理现代化 🤖**

[开始使用](https://github.com/douhuaa/Zss.BilliardHall/blob/main/docs/getting-started.md) • 
[API 文档](https://api.billiard-hall.com/swagger) • 
[演示站点](https://demo.billiard-hall.com) • 
[视频教程](https://www.youtube.com/playlist?list=billiard-hall-tutorials)

</div>