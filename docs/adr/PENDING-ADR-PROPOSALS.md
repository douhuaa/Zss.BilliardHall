# 待落地 ADR 提案跟踪清单

**版本**：1.0  
**创建日期**：2026-01-24  
**状态**：Active  
**治理依据**：<a>ADR-0900 新增与修订流程</a>

---

## 概述

本文档跟踪所有待实施的 ADR 提案。每个提案必须按照 ADR-0900 流程完成"三位一体交付"：
1. ADR 正文（裁决型文档）
2. 架构测试（自动化验证）
3. Copilot 提示词（开发辅助）

**重要提醒**：
- ⚠️ 所有提案必须先创建 RFC，经过审批后方可实施
- ⚠️ 缺少任何一项交付物视为未完成，不得合并
- ⚠️ 必须更新相关索引文档和映射表

---

## 提案状态定义

| 状态 | 说明 |
|------|------|
| 📋 Proposed | RFC 已创建，等待审批 |
| ✅ Approved | 已批准，待实施 |
| 🚧 In Progress | 实施中 |
| ✓ Completed | 已完成三位一体交付并合并 |
| ❌ Rejected | 已拒绝 |
| ⏸ On Hold | 暂停（等待前置条件） |

---

## 运行时行为层（runtime/ADR-200~299）

### ADR-201：Command Handler 生命周期管理

**状态**：📋 Proposed  
**优先级**：High  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- Command Handler 的生命周期规范（Scoped/Transient/Singleton）
- 幂等性要求和验证机制
- 依赖注入约束（禁止的依赖类型）
- 资源释放标准（IDisposable 实现）
- 上下文污染防护（禁止共享状态）

**前置依赖**：无

**交付清单**：
- [ ] RFC 文档：`docs/adr/runtime/RFC-201-handler-lifecycle.md`
- [ ] ADR 正文：`docs/adr/runtime/ADR-201-handler-lifecycle-management.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0201_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0201.prompts.md`
- [ ] 更新索引：`docs/adr/runtime/README.md`

**关键约束示例**：
- Handler 必须注册为 Scoped 生命周期
- Handler 禁止依赖 Singleton 有状态服务
- Handler 必须在请求结束时自动释放资源
- Handler 禁止使用静态字段存储状态

---

### ADR-210：领域事件版本化与兼容性

**状态**：📋 Proposed  
**优先级**：High  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- 领域事件的版本命名规范
- 事件序列化格式约束（JSON Schema）
- 向后兼容性要求（添加/删除/修改字段规则）
- 旧事件的迁移适配机制
- 跨模块事件升级流程

**前置依赖**：ADR-120（领域事件命名规范）

**交付清单**：
- [ ] RFC 文档：`docs/adr/runtime/RFC-210-event-versioning.md`
- [ ] ADR 正文：`docs/adr/runtime/ADR-210-event-versioning-compatibility.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0210_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0210.prompts.md`
- [ ] 更新索引：`docs/adr/runtime/README.md`

**关键约束示例**：
- 破坏性变更必须创建新版本事件（如 OrderCreatedV2）
- 旧版本事件必须保持至少 2 个大版本
- 事件必须包含 SchemaVersion 属性
- 事件订阅者必须能处理所有活跃版本

---

### ADR-220：集成事件总线选型与适配规范

**状态**：📋 Proposed  
**优先级**：Medium  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- 事件总线技术选型标准（Wolverine/Kafka/RabbitMQ）
- 发布/订阅接口抽象
- 幂等性保障机制
- 同步/异步桥接规则
- 事务性发布（Outbox Pattern）

**前置依赖**：ADR-210

**交付清单**：
- [ ] RFC 文档：`docs/adr/runtime/RFC-220-event-bus-integration.md`
- [ ] ADR 正文：`docs/adr/runtime/ADR-220-event-bus-integration.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0220_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0220.prompts.md`
- [ ] 更新索引：`docs/adr/runtime/README.md`

**关键约束示例**：
- 模块禁止直接依赖具体事件总线实现
- 必须通过 IEventBus 抽象接口发布事件
- 跨模块事件必须支持至少一次传递保证
- 禁止在同步流程中等待事件处理结果

---

## 静态结构层（structure/ADR-100~199）

### ADR-122：测试代码组织与命名规范

**状态**：📋 Proposed  
**优先级**：High  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- 测试项目目录结构标准
- 测试类命名规范（与源码一对一映射）
- 测试方法命名规范（Given_When_Then）
- 架构测试组织规则
- 集成测试组织规则
- Mock/Stub 命名规范

**前置依赖**：无

**交付清单**：
- [ ] RFC 文档：`docs/adr/structure/RFC-122-test-organization.md`
- [ ] ADR 正文：`docs/adr/structure/ADR-122-test-organization-naming.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0122_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0122.prompts.md`
- [ ] 更新索引：`docs/adr/structure/README.md`

**关键约束示例**：
- 测试目录必须镜像源码结构
- 测试类命名：`{ClassName}Tests`
- 架构测试必须位于：`tests/ArchitectureTests/ADR/`
- 测试方法必须使用：`MethodName_Scenario_ExpectedBehavior` 格式

---

### ADR-123：Repository 接口与分层命名规范

**状态**：📋 Proposed  
**优先级**：Medium  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- Repository 接口命名规范
- Repository 实现类命名规范
- Repository 方法命名约束
- Repository 位置约束（Domain vs Infrastructure）
- Mock Repository 命名规范

**前置依赖**：ADR-0001（模块化架构）

**交付清单**：
- [ ] RFC 文档：`docs/adr/structure/RFC-123-repository-naming.md`
- [ ] ADR 正文：`docs/adr/structure/ADR-123-repository-interface-layering.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0123_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0123.prompts.md`
- [ ] 更新索引：`docs/adr/structure/README.md`

**关键约束示例**：
- Repository 接口必须位于 Domain 层
- Repository 实现必须位于 Infrastructure 层
- 接口命名：`I{Aggregate}Repository`
- 实现命名：`{Aggregate}Repository`（不加 Impl 后缀）

---

### ADR-124：Endpoint 命名及参数约束规范

**状态**：📋 Proposed  
**优先级**：Medium  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- Endpoint 类命名规范
- HTTP 路由命名规范
- 请求/响应 DTO 命名规范
- Endpoint 方法参数约束
- Endpoint 与 Handler 映射规则

**前置依赖**：ADR-121（契约命名规范）

**交付清单**：
- [ ] RFC 文档：`docs/adr/structure/RFC-124-endpoint-naming.md`
- [ ] ADR 正文：`docs/adr/structure/ADR-124-endpoint-naming-constraints.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0124_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0124.prompts.md`
- [ ] 更新索引：`docs/adr/structure/README.md`

**关键约束示例**：
- Endpoint 类命名：`{UseCase}Endpoint`
- 请求 DTO：`{UseCase}Request`
- 响应 DTO：`{UseCase}Response`
- Endpoint 禁止包含业务逻辑
- 一个 Endpoint 只能调用一个 Command 或 Query

---

## 技术方案层（technical/ADR-300~399）

### ADR-301：集成测试环境自动化与隔离约束

**状态**：📋 Proposed  
**优先级**：High  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- TestContainers 使用规范
- 数据库隔离策略（每测试独立 Schema）
- 测试数据清理机制
- 并发测试支持
- CI/CD 环境集成

**前置依赖**：ADR-122（测试组织规范）

**交付清单**：
- [ ] RFC 文档：`docs/adr/technical/RFC-301-integration-test-automation.md`
- [ ] ADR 正文：`docs/adr/technical/ADR-301-integration-test-automation.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0301_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0301.prompts.md`
- [ ] 更新索引：`docs/adr/technical/README.md`

**关键约束示例**：
- 集成测试必须使用 TestContainers 管理外部依赖
- 每个测试方法必须使用独立数据库 Schema
- 测试后必须自动清理资源
- 禁止依赖共享测试数据

---

### ADR-350：日志与可观测性标签与字段标准

**状态**：📋 Proposed  
**优先级**：Medium  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- 结构化日志字段规范
- CorrelationId 传播机制
- 分布式追踪标签标准
- 日志级别使用规范
- 敏感信息脱敏规则

**前置依赖**：ADR-340（日志监控约束）

**交付清单**：
- [ ] RFC 文档：`docs/adr/technical/RFC-350-logging-observability-standards.md`
- [ ] ADR 正文：`docs/adr/technical/ADR-350-logging-observability-standards.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0350_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0350.prompts.md`
- [ ] 更新索引：`docs/adr/technical/README.md`

**关键约束示例**：
- 所有日志必须包含 CorrelationId
- 禁止记录敏感信息（密码、Token）
- 日志字段必须使用标准命名（UserId 而非 user_id）
- 错误日志必须包含 ExceptionType 和 StackTrace

---

### ADR-360：CI/CD Pipeline 流程标准化

**状态**：📋 Proposed  
**优先级**：High  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师单人批准

**范围**：
- 分支合规校验规则
- 失败快照保存机制
- 自动部署触发条件
- 分支保护规则
- PR 必填项检查

**前置依赖**：ADR-0000（架构测试）

**交付清单**：
- [ ] RFC 文档：`docs/adr/technical/RFC-360-cicd-pipeline-standardization.md`
- [ ] ADR 正文：`docs/adr/technical/ADR-360-cicd-pipeline-standardization.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0360_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0360.prompts.md`
- [ ] 更新索引：`docs/adr/technical/README.md`

**关键约束示例**：
- 所有 PR 必须通过架构测试
- main 分支必须启用分支保护
- CI 失败必须保存测试快照
- 禁止跳过测试合并代码

---

## 治理与交付层（governance/ADR-900~999）

### ADR-930：代码审查与 ADR 合规自检流程

**状态**：📋 Proposed  
**优先级**：High  
**RFC**：待创建  
**审批要求**：Tech Lead/架构师大多数同意（治理层）

**范围**：
- PR Template 必填项规范
- Copilot 自检触发机制
- 架构测试集成要求
- 人工审查职责分工
- 审查通过标准

**前置依赖**：ADR-0900（ADR 流程）、ADR-0000（架构测试）

**交付清单**：
- [ ] RFC 文档：`docs/adr/governance/RFC-930-code-review-compliance.md`
- [ ] ADR 正文：`docs/adr/governance/ADR-930-code-review-compliance.md`
- [ ] 架构测试：`src/tests/ArchitectureTests/ADR/ADR_0930_Architecture_Tests.cs`
- [ ] Copilot 提示词：`docs/copilot/adr-0930.prompts.md`
- [ ] 更新索引：`docs/adr/governance/README.md`

**关键约束示例**：
- PR 必须填写变更类型（feat/fix/docs/refactor）
- 所有 ADR 相关 PR 必须经过 Copilot 自检
- 架构测试失败必须在 PR 中说明原因
- 至少一名责任人必须进行人工审查

---

## 实施优先级矩阵

| ADR | 优先级 | 复杂度 | 预估工期 | 建议顺序 |
|-----|--------|--------|----------|----------|
| ADR-201 | High | Medium | 1 周 | 1 |
| ADR-122 | High | Low | 3 天 | 2 |
| ADR-301 | High | High | 2 周 | 3 |
| ADR-360 | High | Medium | 1 周 | 4 |
| ADR-930 | High | Medium | 1 周 | 5 |
| ADR-210 | High | High | 2 周 | 6 |
| ADR-220 | Medium | High | 2 周 | 7 |
| ADR-123 | Medium | Low | 3 天 | 8 |
| ADR-124 | Medium | Low | 3 天 | 9 |
| ADR-350 | Medium | Medium | 1 周 | 10 |

**优先级说明**：
- **High**：影响日常开发效率或系统稳定性，需优先实施
- **Medium**：提升代码质量和可维护性，可按需实施

**复杂度说明**：
- **Low**：主要是命名和组织规范，实施简单
- **Medium**：涉及生命周期、流程管理，需要一定设计
- **High**：涉及分布式、版本化、事务等复杂场景

---

## 实施流程（标准工作流）

每个 ADR 提案必须按以下步骤完成：

### 1. RFC 阶段
- [ ] 使用 RFC 模板创建提案文档
- [ ] 提交 GitHub Issue 进行讨论
- [ ] 根据反馈修订 RFC
- [ ] 获得审批（根据 ADR 层级要求）

### 2. 实施阶段
- [ ] 创建 ADR 正文（裁决型，150-400 行）
- [ ] 编写架构测试（至少覆盖所有 L1/L2 规则）
- [ ] 创建 Copilot 提示词（场景、反模式、诊断）
- [ ] 本地验证所有测试通过

### 3. 审查阶段
- [ ] 执行三位一体映射校验
- [ ] Copilot 自检通过
- [ ] 人工审查通过
- [ ] 更新所有相关索引

### 4. 归档阶段
- [ ] 合并到 main 分支
- [ ] 更新 ADR README 和导航
- [ ] 团队公告和培训（如需要）
- [ ] 关闭相关 RFC Issue

---

## 质量门禁（Quality Gates）

每个 ADR 交付必须通过以下检查：

### 文档质量
- [ ] 严格遵循 ADR 模板格式
- [ ] 规则数量：3-10 条
- [ ] 文档长度：< 400 行（裁决型 ADR）
- [ ] 无柔性词汇（"建议"、"推荐"）
- [ ] 所有规则标注执行级别（L1/L2/L3）

### 测试覆盖
- [ ] 每个 L1/L2 规则有对应测试
- [ ] 所有测试通过
- [ ] 测试命名清晰（描述被验证的规则）

### Copilot 提示词
- [ ] 包含 4-5 个触发场景
- [ ] 包含 3-5 个必须阻止的反模式
- [ ] 包含每个架构测试的失败诊断
- [ ] 包含快速参考卡片

### 索引更新
- [ ] 更新层级 README（如 runtime/README.md）
- [ ] 更新 docs/adr/README.md
- [ ] 更新 docs/copilot/README.md
- [ ] 验证所有链接有效

---

## 常见陷阱（避免重复错误）

基于 <a>PR 常见问题总结</a>，注意避免：

1. **ADR 过于冗长**：删除背景论证、过多示例、实现细节
2. **规则不可判定**：确保每条规则可自动验证
3. **跨层越权**：运行层不定义技术实现，技术层不定义业务规则
4. **缺失 Copilot 提示词**：三位一体缺一不可
5. **未更新索引**：创建文档后必须更新所有相关索引
6. **ADR 与工程标准混淆**：裁决规则放 ADR，最佳实践放工程标准

---

## 进度跟踪

**创建日期**：2026-01-24  
**最后更新**：2026-01-24  

| 阶段 | 已完成 | 总数 | 完成率 |
|------|--------|------|--------|
| RFC 创建 | 0 | 10 | 0% |
| RFC 审批 | 0 | 10 | 0% |
| ADR 实施 | 0 | 10 | 0% |
| 三位一体交付 | 0 | 10 | 0% |

---

## 参考资源

- <a>ADR-0900 新增与修订流程</a>
- <a>ADR 模板</a>
- <a>RFC 模板</a>
- <a>Copilot 提示词模板</a>
- <a>PR 常见问题总结</a>

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|--------|
| 1.0 | 2026-01-24 | 初始版本，创建 10 个 ADR 提案跟踪 | GitHub Copilot |

