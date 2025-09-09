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
# 实际项目技术栈配置
backend:
  framework: "ABP Framework 9.3.2"
  base_framework: "ASP.NET Core 9.0"
  database: "MySQL"
  orm: "Entity Framework Core"
  api: "ABP Application Services + RESTful API"
  architecture: "Domain Driven Design (DDD)"
  
frontend:
  framework: "Blazor Server + Blazor WebAssembly"
  ui_library: "Blazorise + Bootstrap 5"
  icons: "FontAwesome"
  theme: "LeptonX Lite"
  
orchestration:
  platform: ".NET Aspire 9.4.1"
  service_discovery: "Microsoft.Extensions.ServiceDiscovery"
  resilience: "Microsoft.Extensions.Http.Resilience"
  
infrastructure:
  caching: "Redis"
  monitoring: "OpenTelemetry"
  health_checks: "ASP.NET Core Health Checks"
  containerization: "Docker (via Aspire)"
  logging: "Serilog"
  
abp_modules:
  - "Identity Management"
  - "Permission Management"
  - "Tenant Management"
  - "Audit Logging"
  - "Feature Management"
  - "Setting Management"
  - "Background Jobs"
  - "Blob Storage"
```

## 业务领域模型 (Business Domain Model)

### ABP 领域驱动设计架构

本项目采用 ABP Framework 的领域驱动设计 (DDD) 架构，包含以下核心业务实体：

- **BilliardHall**: 台球厅聚合根，管理台球桌和包间
- **BilliardTable**: 台球桌实体，支持多种类型和状态管理
- **Reservation**: 预约聚合根，处理客户预约业务
- **PrivateRoom**: 包间实体，提供高端服务

所有实体均继承 ABP 基类 (`FullAuditedEntity<Guid>` 或 `FullAuditedAggregateRoot<Guid>`) 并实现 `IMultiTenant` 接口以支持多租户架构。

详细的实体定义、枚举类型和业务规则请参考：[ABP 代码模式和约定](./.copilot/patterns/coding-patterns.md)

## 代码生成指南 (Code Generation Guidelines)

### 核心开发模式

本项目严格遵循 ABP Framework 的开发模式和约定：

1. **ABP Application Services**: 使用 ABP 的应用服务模式而非传统 MVC 控制器
   - 继承 ABP 基类和接口
   - 集成权限管理和多租户支持
   - 遵循 DDD 分层架构

2. **Entity Framework Core + MySQL**: 使用 ABP 的 EF Core 集成
   - ABP 实体配置约定
   - MySQL 特定的数据类型和索引优化
   - 多租户数据过滤

3. **Blazor 组件开发**: Blazor Server + WebAssembly 混合架构
   - 使用 Blazorise UI 组件库
   - ABP 权限和本地化集成
   - 响应式设计模式

4. **.NET Aspire 编排**: 云原生服务编排和监控
   - 服务发现和健康检查
   - OpenTelemetry 集成监控
   - Docker 容器化部署

### 详细开发模式参考

具体的代码实现模式和示例请参考专门的模式文档：

- **[ABP Application Service 模式](./.copilot/patterns/api-patterns.md)** - Application Services, DTOs, Domain Services 的完整实现示例
- **[ABP Entity Framework Core + MySQL 模式](./.copilot/patterns/database-patterns.md)** - DbContext 配置、实体映射、数据迁移的详细指导
- **[Blazor 组件开发模式](./.copilot/patterns/blazor-patterns.md)** - Blazor Server/WASM 组件、页面和服务的开发规范
- **[.NET Aspire 编排模式](./.copilot/patterns/aspire-patterns.md)** - 服务编排、配置管理、监控集成的实施指南
- **[ABP 测试模式](./.copilot/patterns/testing-patterns.md)** - 单元测试、集成测试、ABP 测试基础设施的使用方法

## 测试策略 (Testing Strategy)

本项目采用 ABP Framework 的测试基础设施，支持领域层、应用层和集成测试：

- **Domain Layer Tests**: 测试领域实体、聚合根、领域服务的业务逻辑
- **Application Layer Tests**: 测试应用服务的业务流程和 API 契约
- **Integration Tests**: 测试完整的端到端场景，包括数据库操作

详细的测试实现模式和基础设施使用请参考：[ABP 测试模式](./.copilot/patterns/testing-patterns.md)

## 错误处理模式 (Error Handling Patterns)

本项目采用 ABP Framework 的统一异常处理机制：

- **BusinessException**: 业务逻辑异常，包含错误代码和本地化消息
- **ValidationException**: 输入验证异常，自动处理 Data Annotations
- **AuthorizationException**: 权限验证异常，ABP 自动处理
- **Global Exception Handler**: 统一的异常处理中间件

## 性能优化指南 (Performance Guidelines)

### 关键优化策略

1. **数据库查询优化**
   - 使用适当的索引策略
   - 避免 N+1 查询问题 
   - 实施分页和排序

2. **缓存策略**
   - Redis 用于分布式缓存
   - ABP 内存缓存用于静态配置
   - CDN 用于静态资源

3. **API 设计优化**
   - ABP 动态 API 和版本控制
   - 响应压缩和请求限流
   - OpenTelemetry 性能监控

## ABP 权限和安全模式 (ABP Permissions and Security Patterns)

本项目集成 ABP Framework 的完整权限管理体系：

- **权限定义**: 层次化权限树结构
- **角色管理**: ABP Identity 集成的角色权限
- **多租户安全**: 数据隔离和权限控制
- **JWT 认证**: ABP 自动配置的 JWT 令牌系统

## .NET Aspire 编排和部署 (Aspire Orchestration & Deployment)

本项目使用 .NET Aspire 9.4.1 进行云原生应用编排：

- **服务编排**: 自动配置 MySQL、Redis 等基础设施服务
- **健康检查**: 集成 ASP.NET Core 健康检查和 OpenTelemetry 监控
- **服务发现**: 自动配置服务间通信和负载均衡
- **开发体验**: Aspire Dashboard 提供统一的开发和调试界面

## 相关指令文件 (Related Instruction Files)

- [ABP 代码模式和约定](./.copilot/patterns/coding-patterns.md) - ABP Framework 编码规范
- [ABP Application Service 设计规范](./.copilot/patterns/api-patterns.md) - Application Service 开发模式
- [ABP + MySQL 数据库设计规范](./.copilot/patterns/database-patterns.md) - EF Core + MySQL 设计模式
- [ABP 测试指导](./.copilot/patterns/testing-patterns.md) - ABP 测试基础设施使用
- [Blazor 组件开发模式](./.copilot/patterns/blazor-patterns.md) - Blazor Server + WASM 开发
- [Aspire 编排模式](./.copilot/patterns/aspire-patterns.md) - .NET Aspire 服务编排
- [ABP 工作流模板](./.copilot/workflows/README.md) - 开发工作流程
- [ABP 实体架构](./.copilot/schemas/abp-entities.json) - ABP 实体定义架构
- [Aspire 配置架构](./.copilot/schemas/aspire-config.json) - Aspire 编排配置架构

---

> 此文档基于实际项目架构（ABP Framework 9.3.2 + .NET Aspire 9.4.1 + Blazor + MySQL）更新。所有代码生成都应遵循 ABP 框架约定和 Aspire 编排模式，确保一致性和质量。