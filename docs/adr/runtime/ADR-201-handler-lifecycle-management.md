# ADR-201：Handler 生命周期管理

> ⚖️ **本 ADR 定义 Command Handler 生命周期规则，确保线程安全和资源正确管理。**

**状态**：✅ Accepted  
**级别**：运行时层（Runtime Constraint）  
**适用范围**：所有 Command Handler 实现  
**生效时间**：待审批通过后  
**依赖 ADR**：ADR-0005（应用内交互模型）

---

## 聚焦内容（Focus）

- Handler 生命周期与执行上下文匹配规则
- Handler 依赖注入约束
- 静态字段和跨请求状态共享限制
- 资源释放要求
- 生命周期测试执法

---

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|-----|------|---------|
| Handler 生命周期 | DI 容器中 Handler 实例的存活周期（Scoped/Transient/Singleton） | Handler Lifetime |
| Request-driven | HTTP/gRPC 等请求驱动的执行上下文 | Request-driven Context |
| Message-driven | 消息队列/后台任务驱动的执行上下文 | Message-driven Context |
| Context-free | 无状态纯计算，无执行上下文依赖 | Context-free Handler |
| 有状态服务 | 包含可变字段的服务实例 | Stateful Service |
| 跨请求状态 | 在不同请求间共享的可变数据 | Cross-Request State |

---

## 决策（Decision）

### 生命周期与执行上下文匹配（ADR-201.1）【必须架构测试覆盖】

**规则**：
- Request-driven Handler（HTTP/gRPC）：
  - ✅ 必须注册为 Scoped 生命周期
  - ❌ 禁止 Singleton
  - ❌ 禁止 Transient（除非有性能证明）
  
- Message-driven / Background Handler（消息/后台）：
  - ✅ Scoped（推荐）：每个消息独立上下文
  - ✅ Transient（允许）：需在注释中说明原因
  - ❌ 禁止 Singleton
  
- Context-free Handler（无状态纯计算）：
  - ✅ 可使用 Transient
  - ✅ 必须在注释中标注 `// STATELESS-HANDLER`

**判定**：
- ❌ Request-driven Handler 注册为 Singleton
- ❌ Request-driven Handler 注册为 Transient 且无性能测试证明
- ❌ Message-driven Handler 注册为 Singleton
- ❌ Transient Handler 无 `// STATELESS-HANDLER` 注释
- ✅ 生命周期与执行上下文正确匹配

### 禁止依赖 Singleton 有状态服务（ADR-201.2）【必须架构测试覆盖】

**规则**：
- 允许的 Singleton 依赖：
  - ✅ 无状态服务（如 `ILogger<T>`、`IConfiguration`）
  - ✅ 线程安全的共享资源（如 `IOptions<T>`）
  - ✅ 工厂服务（用于创建 Scoped/Transient 实例）
  
- 禁止的 Singleton 依赖：
  - ❌ 包含可变字段的 Singleton 服务
  - ❌ 缓存状态但无线程安全保证的服务
  - ❌ 直接持有数据库连接的 Singleton 服务

**判定**：
- ❌ Handler 构造函数注入 Singleton 有状态服务
- ❌ Handler 依赖 Singleton 缓存服务且无线程安全保证
- ✅ Handler 仅依赖无状态 Singleton 或工厂

### 禁止静态字段存储状态（ADR-201.3）【必须架构测试覆盖】

**规则**：
- 禁止：
  - ❌ `static` 字段（除常量外）
  - ❌ `static` 属性
  - ❌ 静态集合（如 `static List<T>`）
  
- 允许：
  - ✅ `const` 常量
  - ✅ `static readonly` 不可变配置

**判定**：
- ❌ Handler 类声明 static 字段
- ❌ Handler 类声明 static 可变属性
- ✅ Handler 仅声明 const 或 static readonly

### 资源释放要求（ADR-201.4）

**规则**：
- 触发条件：
  - Handler 直接持有 `IDisposable` 资源（非通过 DI 注入）
  - Handler 持有文件流、网络连接等非托管资源
  
- 实现要求：
  - ✅ 实现 `IDisposable` 或 `IAsyncDisposable`
  - ✅ 实现标准 Dispose 模式
  - ✅ 在 Dispose 中释放所有资源
  - ✅ 标记为 `sealed` 或实现完整的继承 Dispose 模式

**注意**：通过 DI 注入的 Scoped/Transient 服务由容器自动释放，Handler 无需手动释放。

**判定**：
- ❌ Handler 持有 IDisposable 资源但未实现 Dispose
- ❌ Dispose 实现不完整或不符合标准模式
- ✅ Handler 正确实现 Dispose 模式

### 禁止跨请求共享状态（ADR-201.5）【必须架构测试覆盖】

**规则**：
- 禁止行为：
  - ❌ 使用 `ThreadStatic` 或 `AsyncLocal<T>` 存储跨请求状态
  - ❌ 将请求状态写入共享缓存而不隔离
  - ❌ 修改 Singleton 服务的状态

**判定**：
- ❌ Handler 使用 ThreadStatic 或 AsyncLocal 存储请求状态
- ❌ Handler 修改 Singleton 服务的可变状态
- ❌ Handler 将请求数据写入共享缓存无隔离
- ✅ Handler 状态隔离在 Scoped 实例内

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-201.1 | L1 | `Handlers_Must_Be_Registered_As_Scoped` (HTTP) / L2 (Background) |
| ADR-201.2 | L2 | Roslyn Analyzer + 人工审查 |
| ADR-201.3 | L1 | `Handlers_Must_Not_Have_Static_Fields` |
| ADR-201.4 | L2 | 人工审查（Code Review） |
| ADR-201.5 | L2 | 人工审查 + Runtime 监控 |

### 执行说明

**L1 测试**：
- 验证 Request-driven Handler 在 DI 容器中注册为 Scoped
- 检测 Handler 类中的 static 字段（排除 const/readonly 不可变类型）
- Background Handler 如使用 Transient 需注释说明

**L2 测试**：
- 通过 Code Review 检查 Singleton 依赖是否有状态
- 审查 IDisposable 实现的正确性
- 检查是否存在跨请求状态共享

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **性能关键路径**：经过基准测试证明 Transient 生命周期导致不可接受的性能损失
2. **遗留系统集成**：与外部 Singleton 服务强制集成
3. **框架限制**：第三方框架要求特定生命周期

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 指明 ADR-201 + 具体规则编号（如 ADR-201.1）
- 提供性能测试数据或技术说明
- 指定失效日期（不超过 6 个月）
- 给出归还计划

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **运行时层 ADR**
  * 修改需 Tech Lead/架构师审批
  * 需评估对现有 Handler 的影响
  * 必须更新相关架构测试

### 失效与替代

* 如有更优方案，可创建 ADR-20X 替代本 ADR
* 被替代后，本 ADR 状态改为 Superseded
* 必须在文档开头指向替代 ADR

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ Handler 的命名规范（参见 ADR-0005）
- ✗ Handler 的目录组织（参见 ADR-0001）
- ✗ Handler 的具体业务逻辑实现
- ✗ Query Handler 的生命周期（仅管 Command Handler）
- ✗ 幂等性的具体实现机制（将由 ADR-202 定义）

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-240：Handler 异常约束](ADR-240-handler-exception-constraints.md)

### 技术背景
- [Microsoft DI Lifetime 文档](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [IDisposable 模式](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose)

### 实践指导
- 详细的 Handler 生命周期实现示例参见工程标准文档
- 常见问题排查参见 `docs/copilot/adr-0201.prompts.md`

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 2.0 | 2026-01-25 | 重构为裁决型格式，添加决策章节 | GitHub Copilot |
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
