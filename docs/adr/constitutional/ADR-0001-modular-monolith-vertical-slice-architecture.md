# ADR-0001：模块化单体与垂直切片架构

**状态**：✅ Final（裁决型ADR）  
**版本**：4.0  
**级别**：架构约束 / 系统级 Contract  
**适用范围**：所有 Host、模块、各类测试、未来扩展子系统  
**生效时间**：即刻

---

## 本章聚焦内容（Focus）

仅定义适用于全生命周期自动化裁决/阻断的**模块隔离约束**：

- 模块按业务能力独立划分，物理隔离
- 垂直切片为最小业务组织单元
- 模块间禁止直接依赖与共享领域对象
- 模块通信仅通过领域事件、契约、原始类型
- 所有规则必须架构测试覆盖

---

## 术语表（Glossary）

| 术语            | 定义                             |
|---------------|--------------------------------|
| 模块化单体         | 单进程，按业务能力独立模块，物理分离             |
| 垂直切片          | 以单个业务用例为最小组织单元，端到端实现           |
| 契约（Contract）  | 只读数据 DTO，仅用于模块间信息传递            |
| 领域事件          | 描述业务事实，供其他模块异步订阅               |
| 横向分层          | Controller/Service/Repository 抽象 |
| 依赖隔离          | 模块内部不可被外部引用、反射或直接调用            |

---

## 决策（Decision）

### 模块定义与隔离（ADR-0001.1, 0001.2, 0001.7）

**规则**：
- 模块按业务能力独立划分（如 Members/Orders）
- 每模块 = 独立程序集 = 清晰目录边界
- 模块间禁止直接引用代码、类型、资源

**判定**：
- ❌ 模块引用其他模块类型
- ❌ 项目文件引用其他模块
- ❌ 命名空间不匹配模块边界
- ✅ 仅通过契约、事件、原始类型通信

### 垂直切片组织（ADR-0001.3, 0001.4）

**规则**：
- 用例（Use Case）为最小组织单元
- 每用例包含 Endpoint → Command/Query → Handler → 领域逻辑
- 禁止横向 Service 层

**判定**：
- ❌ Handler 不在 UseCases 命名空间
- ❌ 存在 *Service 类
- ✅ 每用例自包含完整切片

### 模块通信约束（ADR-0001.5, 0001.6）

**规则**：
- 模块间仅允许：领域事件、契约 DTO、原始类型（Guid/string/int）
- 契约不含业务决策字段或行为方法

**判定**：
- ❌ 直接依赖 Entity/Aggregate/VO
- ❌ 契约包含业务判断字段（如 CanRefund）
- ✅ 仅传递只读数据

---

## 快速参考表

| 约束编号       | 约束描述          | 测试方式                                         | 测试用例                                           | 必须遵守 |
|------------|---------------|----------------------------------------------|------------------------------------------------|------|
| ADR-0001.1 | 模块不可相互引用      | L1 - NetArchTest                             | Modules_Should_Not_Reference_Other_Modules     | ✅    |
| ADR-0001.2 | 项目文件禁止引用其他模块  | L1 - 项目文件扫描                                  | Module_Csproj_Should_Not_Reference_Other_Modules | ✅    |
| ADR-0001.3 | 垂直切片/用例为最小单元  | L2 - NetArchTest                             | Handlers_Should_Be_In_UseCases_Namespace       | ✅    |
| ADR-0001.4 | 禁止横向 Service 抽象 | L1 - NetArchTest                             | Modules_Should_Not_Contain_Service_Classes     | ✅    |
| ADR-0001.5 | 只允许事件/契约/原始类型通信 | L2 - 语义检查                                    | Contract_Rules_Semantic_Check                  | ✅    |
| ADR-0001.6 | Contract 不含业务判断字段 | L2/L3 - Roslyn分析 + 人工                        | Contract_Business_Field_Analyzer               | ✅    |
| ADR-0001.7 | 命名空间/目录强制隔离    | L1 - NetArchTest                             | Namespace_Should_Match_Module_Boundaries       | ✅    |

> **级别说明**：L1=静态自动化（ArchitectureTests），L2=语义半自动（Roslyn/启发式），L3=人工Gate

---

## 必测/必拦架构测试（Enforcement）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs` 强制验证：

- 模块不应相互引用（程序集级别）
- 模块项目文件不应引用其他模块
- 命名空间应匹配模块边界
- Handler 必须在 UseCases 命名空间
- 模块不应包含 Service 类
- 契约语义规则检查
- 契约业务字段分析

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] 模块按业务能力划分且物理完全隔离？
- [ ] 垂直切片以用例（UseCase）为唯一最小组织单元？
- [ ] 杜绝了横向 Service、领域模型共享、同步耦合？
- [ ] 模块间通信仅允许事件、契约和原始类型？
- [ ] 合约和事件定义无业务决策字段？
- [ ] 所有隔离规则有自动化测试覆盖？

---

## 依赖与相关ADR

| 关联 ADR   | 关系                |
|----------|-------------------|
| ADR-0000 | 自动化测试机制与执行分级      |
| ADR-0002 | 定义装配/启动方式         |
| ADR-0003 | 模块命名空间自动映射及目录防御规则 |
| ADR-0004 | 分层包依赖与 CPM        |
| ADR-0005 | 运行时交互/Handler职责   |

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 测试执行机制

**被依赖（Depended By）**：
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 基于模块隔离规则
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 基于模块边界定义
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 基于模块通信约束
- [ADR-120：领域事件命名约定](../structure/ADR-120-domain-event-naming-convention.md) - 基于模块通信机制
- [ADR-121：契约 DTO 命名与组织](../structure/ADR-121-contract-dto-naming-organization.md) - 基于模块通信机制
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-123：Repository 接口与分层命名规范](../structure/ADR-123-repository-interface-layering.md)
- [ADR-920：示例代码治理宪法](../governance/ADR-920-examples-governance-constitution.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 依赖管理补充
- [ADR-0006：术语与编号宪法](./ADR-0006-terminology-numbering-constitution.md) - 术语规范（注：ADR-0006 依赖本 ADR，此处为相关关系避免循环）
- [ADR-0008：文档编写与维护宪法](./ADR-0008-documentation-governance-constitution.md) - 文档治理

---

## 版本历史

| 版本  | 日期         | 变更说明       |
|-----|------------|------------|
| 4.0 | 2026-01-26 | 裁决型重构，移除冗余 |
| 3.2 | 2026-01-23 | 术语&执行等级补充  |
| 3.0 | 2026-01-20 | 分层体系与目录规则固化 |
| 1.0 | 初始         | 初版发布       |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（建议、最佳实践、详细示例）请查阅：
- [ADR-0001 Copilot Prompts](../../copilot/adr-0001.prompts.md)
- 工程指南（如有）
