# ADR-0002：Platform / Application / Host 三层启动体系

**状态**：✅ Final（裁决型ADR）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Host、模块、测试、未来子系统  
**生效时间**：即刻

---

## 本章聚焦内容（Focus）

仅定义适用于全生命周期自动化裁决/阻断的**三层装配约束**：

- Platform / Application / Host 职责分明
- 层级依赖方向：唯一单向依赖（Host → Application → Platform）
- 每层必须有唯一 Bootstrapper 入口
- Program.cs 极简化（≤30行）
- 所有规则必须架构测试覆盖

---

## 术语表（Glossary）

| 术语           | 定义                             |
|--------------|--------------------------------|
| Platform     | 技术基座，仅提供技术能力，不感知业务             |
| Application  | 应用装配层，定义"系统是什么"，聚合模块和用例        |
| Host         | 进程外壳，决定"怎么跑"，如 Web/Worker/Test |
| Bootstrapper | 唯一的装配入口，负责注册服务和配置              |
| 单向依赖         | Host → Application → Platform  |

---

## 决策（Decision）

### Platform 层约束（ADR-0002.1, 0002.2, 0002.3, 0002.4）

**规则**：
- 只提供通用技术能力（日志、追踪、异常、序列化）
- 不感知任何业务（不可访问 Application、Host、Modules）
- 必须有唯一入口 `PlatformBootstrapper.Configure`

**判定**：
- ❌ Platform 依赖 Application/Host/Modules
- ❌ Platform 缺少唯一 Bootstrapper 入口
- ✅ 仅包含技术基础设施

### Application 层约束（ADR-0002.5, 0002.6, 0002.7, 0002.8）

**规则**：
- 负责系统能力的装配和集成
- 禁止依赖 Host
- 必须有唯一入口 `ApplicationBootstrapper.Configure`
- 不包含 HttpContext 等 Host 专属类型

**判定**：
- ❌ Application 依赖 Host/Modules
- ❌ Application 使用 HttpContext
- ❌ Application 缺少唯一 Bootstrapper 入口
- ✅ 仅做模块装配和集成

### Host 层约束（ADR-0002.9, 0002.10, 0002.11, 0002.12, 0002.13）

**规则**：
- 唯一职责：调用 Platform、Application 的 Bootstrapper
- 决定进程模型，不包含任何业务逻辑
- Program.cs 保持极简（建议 ≤30 行）
- 项目命名为 `Zss.BilliardHall.Host.*`

**判定**：
- ❌ Host 依赖 Modules
- ❌ Host 包含业务类型
- ❌ Host 项目文件引用 Modules
- ❌ Program.cs 超过 30 行
- ❌ Program.cs 做了 Bootstrapper 以外的事
- ✅ 仅调用两个 Bootstrapper

### 三层依赖方向验证（ADR-0002.14）

**规则**：
- 完整的单向依赖链：Host → Application → Platform

**判定**：
- ❌ 任何反向依赖
- ✅ 严格的单向依赖流

---

## 快速参考表

| 约束编号        | 约束描述                            | 测试方式           | 测试用例                                                    | 必须遵守 |
|-------------|-------------------------------- |-----------------|---------------------------------------------------------|------|
| ADR-0002.1  | Platform 不应依赖 Application        | L1 - NetArchTest | Platform_Should_Not_Depend_On_Application               | ✅    |
| ADR-0002.2  | Platform 不应依赖 Host               | L1 - NetArchTest | Platform_Should_Not_Depend_On_Host                      | ✅    |
| ADR-0002.3  | Platform 不应依赖任何 Modules          | L1 - NetArchTest | Platform_Should_Not_Depend_On_Modules                   | ✅    |
| ADR-0002.4  | Platform 应有唯一 Bootstrapper 入口   | L1 - NetArchTest | Platform_Should_Have_Single_Bootstrapper_Entry_Point    | ✅    |
| ADR-0002.5  | Application 不依赖 Host             | L1 - NetArchTest | Application_Should_Not_Depend_On_Host                   | ✅    |
| ADR-0002.6  | Application 不依赖 Modules          | L1 - NetArchTest | Application_Should_Not_Depend_On_Modules                | ✅    |
| ADR-0002.7  | Application 应有唯一 Bootstrapper 入口 | L1 - NetArchTest | Application_Should_Have_Single_Bootstrapper_Entry_Point | ✅    |
| ADR-0002.8  | Application 不含 Host 专属类型         | L1 - NetArchTest | Application_Should_Not_Use_HttpContext                  | ✅    |
| ADR-0002.9  | Host 不依赖 Modules                 | L1 - NetArchTest | Host_Should_Not_Depend_On_Modules                       | ✅    |
| ADR-0002.10 | Host 不含业务类型                      | L1 - NetArchTest | Host_Should_Not_Contain_Business_Types                  | ✅    |
| ADR-0002.11 | Host 项目文件不应引用 Modules            | L1 - 项目文件扫描      | Host_Csproj_Should_Not_Reference_Modules                | ✅    |
| ADR-0002.12 | Program.cs 建议 ≤30行               | L1 - 文件扫描        | Program_Cs_Should_Be_Concise                            | ✅    |
| ADR-0002.13 | Program.cs 只应调用 Bootstrapper     | L1 - 语义检查        | Program_Cs_Should_Only_Call_Bootstrapper                | ✅    |
| ADR-0002.14 | 三层唯一依赖方向验证                       | L1 - NetArchTest | Verify_Complete_Three_Layer_Dependency_Direction        | ✅    |

> **级别说明**：L1=静态自动化（ArchitectureTests）

---

## 必测/必拦架构测试（Enforcement）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0002_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---

## 检查清单

- [ ] Platform 是否只提供技术能力，不依赖业务？
- [ ] Application 只做装配，无 Host/业务依赖？
- [ ] Host 仅负责装配和启动，无业务逻辑、无服务注册？
- [ ] Program.cs 是否极简、只调用 Bootstrapper？
- [ ] 每一层均有唯一 Bootstrapper 入口定义？
- [ ] 所有依赖方向只允许单向流动？

---

## 依赖与相关ADR

| 关联 ADR   | 关系            |
|----------|---------------|
| ADR-0000 | 自动化测试机制       |
| ADR-0001 | 模块组织与切片（配合定义） |
| ADR-0003 | 命名空间自动推导      |
| ADR-0004 | 包管理与依赖分层      |
| ADR-0005 | 运行时交互模型       |

---

## 版本历史

| 版本  | 日期         | 变更说明       |
|-----|------------|------------|
| 4.0 | 2026-01-26 | 裁决型重构，移除冗余 |
| 3.0 | 2026-01-22 | 结构升级、测试映射  |
| 2.0 | 2026-01-20 | 目录与依赖方向细化  |
| 1.0 | 初版         | 初始发布       |

---

## 附注

本文件禁止添加示例/建议/FAQ/背景说明，仅维护自动化可判定的架构红线。

非裁决性参考（目录结构、命名规范、详细示例）请查阅：
- [ADR-0002 Copilot Prompts](../../copilot/adr-0002.prompts.md)
- 工程指南（如有）
