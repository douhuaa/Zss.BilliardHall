# ADR-0005：应用内交互模型极简判裁版

**状态**：✅ Final（仅保留裁决性规则，无“建议/经验”）  
**级别**：根约束（Runtime Constitutional Rule）  
**适用范围**：Application / Modules / Host  
**生效时间**：即刻

---

## 本章聚焦内容（Focus）

仅定义适用于全生命周期自动化裁决/阻断的**运行时交互约束**：

- Use Case + Handler = 最小业务决策单元
- Handler 职责、边界、合规输出
- 模块通信（同步/异步、契约形式）硬约束
- CQRS 强制分离与唯一权威
- 禁止共享领域模型、未审批同步、endpoint/handler 角色混淆

---

## 术语表（Glossary）

| 术语            | 定义             |
|---------------|----------------|
| Use Case      | 端到端业务用例        |
| Handler       | 业务用例的唯一决策实现    |
| Command/Query | 分别代表写/读单一职责    |
| CQRS          | 命令-查询职责分离      |
| 合约（Contract）  | 模块间只读通信对象      |
| 领域实体          | 业务内聚的复杂类型，不跨模块 |
| 模块间通信         | 只允许事件、合约、原始类型  |

---

## 极简裁决性规则列表

### Use Case 执行与裁决权

- 每个业务用例必须唯一 Handler，且该 Handler 拥有全部业务决策权
- Endpoint/Controller 仅允许做请求适配和 Handler 调用，禁止承载任何业务规则

### Handler 职责边界

- Handler 不得持有跨调用生命周期的业务状态
- Handler 禁止作为同步跨模块"粘合层"
- Handler 不允许返回或暴露领域实体作为出参

### 模块通信及同步/异步边界

- 模块内：允许同步方法调用
- 模块间：**默认只能异步通信（领域事件/集成事件）**
- 未经审批，禁止任何跨模块同步调用（如直接方法、同步仓储、接口依赖等）

### 通信契约（Contract）与领域模型隔离

- 模块间仅通过协定 DTO/事件通信，禁止直接引用/传递 Entity/Aggregate/VO
- 合约（Contract）只允许传递数据，不承载业务决策/行为

### CQRS & Handler 唯一性

- Command Handler 只允许执行业务逻辑并返回 void 或唯一标识
- Query Handler 只允许只读 DTO/投影返回
- Command/Query Handler 必须职责分离，不允许合并、混用

---

## 必测/必拦架构测试（Enforcement）

- Handler 必须无状态、无领域持久字段
- Endpoint 不得出现业务规则（有状态/决策分支/存储/调用其他模块等）
- Handler/Endpoint 不直接或间接依赖 ASP.NET Host 类型
- 跨模块同步调用自动检测并阻断
- Contracts 类型仅允许原始类型、DTO，不允许 Entity/VO
- 所有 Handler 均只在模块程序集，不允横向注册
- Query Handler 不允许写入操作，Command Handler 不允许查询/返回实体

所有规则一律用 NetArchTest、Roslyn Analyzer 或人工 Gate 强制，有一项违规视为架构违规。

---

## 极简检查清单（Checklist）

- [ ] 每个用例唯一 Handler 归属
- [ ] Endpoint 禁止写业务条件/分支/存储
- [ ] Handler/Endpoint 不依赖 ASP.NET 类型
- [ ] Handler 无持久状态
- [ ] 跨模块所有调用为异步事件/契约
- [ ] Contract 不含业务判断、行为方法
- [ ] Query/Command Handler 职责完全分离

---

## 约束关系与修订边界

- 本 ADR 仅列出不可破例自动化裁断项
- 所有治理建议、失败处理、Saga/补偿、最佳实践类内容须单独写 Guide/Rationale
- 修订仅允许缩减内容或提升自动裁决性，禁止添加“仅建议性”内容
- 审判权来自自动化检测，可扩充测试工具，但不得新增人工经验条款

---

## 依赖与相关ADR

- ADR-0001：静态模块切片与隔离宪法
- ADR-0002：启动体系与装配边界
- ADR-0003：命名空间与结构规范
- ADR-0004：依赖与包治理规则
- ADR-0000：自动化测试与 CI 验证

---

## 快速参考表

| 约束编号   | 约束描述                  | 测试/阻断方式            | 必须遵守 |
|--------|-----------------------|--------------------|------|
| 0005.1 | Use Case 唯一 Handler   | NetArchTest+人工     | ✅    |
| 0005.2 | Handler 无状态/无Entity字段 | NetArchTest        | ✅    |
| 0005.3 | 跨模块同步禁止               | NetArchTest+Roslyn | ✅    |
| 0005.4 | Endpoint/Handler 不含业务 | Roslyn+人工          | ✅    |
| 0005.5 | Contract 不含业务方法/决策    | Roslyn+人工          | ✅    |
| 0005.6 | Command/Query 职责分离    | NetArchTest+人工     | ✅    |

---

## 版本历史

| 版本  | 日期         | 变更说明             |
|-----|------------|------------------|
| 3.0 | 2026-01-23 | 精简为极简判裁版，仅保裁决性规则 |
| 2.0 | 2026-01-20 | 作为“经验/治理+裁决”混合版  |
| 1.0 | 2026-01-20 | 初始发布             |

---

## 附注

本文件禁止添加示例/建议/FAQ，仅维护自动化可判定的架构红线。

Guide/FAQ/Rationale 另行维护，不具裁决权力。
