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

## 架构定义和代码模板 (Schemas & Templates)

### JSON 架构文件 (JSON Schema Files)

本项目使用机器可读的 JSON Schema 定义来确保代码生成的一致性和准确性。GitHub Copilot 应严格遵循这些架构规范：

#### 1. ABP 实体架构 (`schemas/abp-entities.json`)
- **用途**: 定义 ABP Framework 实体的标准结构、属性验证和业务方法
- **适用场景**: 创建新的领域实体、聚合根或值对象
- **使用示例**:
  ```markdown
  基于 schemas/abp-entities.json 架构，为台球厅管理系统创建一个 Customer 实体，
  包含会员编号、姓名、手机、邮箱、会员等级等属性，继承 FullAuditedEntity<Guid>，
  实现 IMultiTenant 接口，并添加升级会员等级的业务方法。
  ```

#### 2. 业务实体架构 (`schemas/entities.json`)
- **用途**: 定义核心业务实体的数据结构、约束条件和关联关系
- **适用场景**: 设计业务模型、数据库结构或 API 响应格式
- **使用示例**:
  ```markdown
  参考 schemas/entities.json 中的 BilliardTable 定义，创建台球桌管理功能，
  包含编号、类型、状态、小时费率、位置信息等属性，并实现状态变更业务逻辑。
  ```

#### 3. API 响应架构 (`schemas/api-responses.json`)
- **用途**: 标准化所有 API 响应格式，包括成功响应、错误处理和分页结构
- **适用场景**: 设计 RESTful API、处理异常响应或实现分页查询
- **使用示例**:
  ```markdown
  遵循 schemas/api-responses.json 规范，为 BilliardTable 实体创建完整的 CRUD API，
  包含标准的成功响应、验证错误响应和分页列表响应格式。
  ```

#### 4. Aspire 配置架构 (`schemas/aspire-config.json`)
- **用途**: 定义 .NET Aspire 应用编排的配置结构和部署参数
- **适用场景**: 配置微服务编排、设置基础设施依赖或部署容器化应用
- **使用示例**:
  ```markdown
  基于 schemas/aspire-config.json 架构，为台球厅系统配置 Aspire 应用编排，
  包含 MySQL 数据库、Redis 缓存、Jaeger 追踪和健康检查配置。
  ```

### 代码生成模板 (Code Generation Templates)

#### ABP 应用服务模板 (`templates/abp-application-service-template.md`)
- **用途**: 生成符合 ABP 框架规范的应用服务代码
- **包含内容**:
  - 接口定义模板
  - 服务实现模板
  - DTO 模板（输入/输出）
  - 权限验证和异常处理
  - 批量操作和统计功能
- **使用示例**:
  ```markdown
  使用 templates/abp-application-service-template.md 模板，为 Reservation 实体
  生成完整的应用服务，包含 CRUD 操作、状态变更、批量处理和统计报表功能。
  ```

### 架构使用最佳实践 (Best Practices)

1. **提示词结构化引用**:
   ```markdown
   # 推荐的提示词格式
   基于 schemas/{schema-file}.json 架构定义，
   使用 templates/{template-file}.md 模板，
   为 {业务场景} 生成 {功能描述}。
   
   具体要求：
   - 严格遵循 JSON Schema 规范
   - 继承指定的 ABP 基类
   - 实现必要的接口和验证
   - 包含完整的错误处理
   ```

2. **架构验证检查**:
   - 生成的代码必须符合对应的 JSON Schema
   - 实体属性必须包含必要的验证注解
   - API 响应必须遵循标准格式
   - Aspire 配置必须包含所有必需字段

3. **模板定制化使用**:
   - 根据具体业务需求调整模板变量
   - 保持模板结构的一致性
   - 添加必要的业务方法和验证逻辑

### 实际使用示例

详细的架构和模板使用示例请参考：[使用示例文档](./.copilot/usage-examples.md)

包含以下场景的完整示例：
- **实体创建** - 基于架构定义创建新的业务实体
- **API 设计** - 遵循响应格式标准设计 RESTful API  
- **应用服务** - 使用模板生成完整的应用服务代码
- **基础设施** - 配置 Aspire 服务编排和监控
- **端到端开发** - 组合使用多个架构和模板完成复杂功能

### 架构文件维护和更新

- **更新频率**: 当业务模型变更或发现新的最佳实践时更新
- **版本控制**: 重大架构变更需要更新版本号
- **验证机制**: 使用 JSON Schema 验证器确保架构正确性
- **文档同步**: 架构更新时同步更新相关文档和示例

### 优化建议和未来改进

详细的架构和模板优化建议请参考：[架构优化建议文档](./.copilot/optimization-recommendations.md)

主要改进方向包括：
- **架构重组**: 按功能分类重新组织架构文件结构
- **模板扩展**: 增加更多代码生成模板以覆盖常见开发场景
- **集成优化**: 改善与 GitHub Copilot 和开发工作流的集成
- **质量保证**: 建立架构版本控制和质量验证机制

## 相关指令文件 (Related Instruction Files)

- [ABP 代码模式和约定](./.copilot/patterns/coding-patterns.md) - ABP Framework 编码规范
- [ABP Application Service 设计规范](./.copilot/patterns/api-patterns.md) - Application Service 开发模式
- [ABP + MySQL 数据库设计规范](./.copilot/patterns/database-patterns.md) - EF Core + MySQL 设计模式
- [ABP 测试指导](./.copilot/patterns/testing-patterns.md) - ABP 测试基础设施使用
- [Blazor 组件开发模式](./.copilot/patterns/blazor-patterns.md) - Blazor Server + WASM 开发
- [Aspire 编排模式](./.copilot/patterns/aspire-patterns.md) - .NET Aspire 服务编排
- [ABP 工作流模板](./.copilot/workflows/README.md) - 开发工作流程

### 架构和模板文件 (Schemas & Templates)
- [ABP 实体架构](./.copilot/schemas/abp-entities.json) - ABP 实体定义架构
- [业务实体架构](./.copilot/schemas/entities.json) - 核心业务实体数据结构
- [API 响应架构](./.copilot/schemas/api-responses.json) - API 响应格式标准
- [Aspire 配置架构](./.copilot/schemas/aspire-config.json) - Aspire 编排配置架构
- [ABP 应用服务模板](./.copilot/templates/abp-application-service-template.md) - ABP 应用服务代码模板

### 指导和优化文档 (Guidelines & Optimization)
- [使用示例文档](./.copilot/usage-examples.md) - 架构和模板实际使用示例
- [架构优化建议](./.copilot/optimization-recommendations.md) - 架构和模板优化指南
- [架构验证脚本](./.copilot/validate-schemas.py) - JSON Schema 验证工具

---

> 此文档基于实际项目架构（ABP Framework 9.3.2 + .NET Aspire 9.4.1 + Blazor + MySQL）更新。所有代码生成都应遵循 ABP 框架约定和 Aspire 编排模式，确保一致性和质量。