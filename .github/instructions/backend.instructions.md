# 后端开发指令

> ⚖️权威声明: 若本文档与 ADR 正文冲突，以 ADR 正文为准。
> 基于 base.instructions.md

## 适用范围
后端 / 业务逻辑开发

## 权威依据
- ADR-0001（模块化单体 / 垂直切片）
- ADR-0005（CQRS / Handler / Endpoint）

## 引用优先级
ADR 正文 > 架构测试 > Copilot Prompts

---

## 垂直切片硬约束（ADR-0001）

- ❌ 禁止 Service / Manager / Helper 承载业务逻辑
- ❌ 禁止跨用例共享业务逻辑
- ✅ 每个用例是完整切片

📌 ADR-0001 §3.2

---

## Handler 硬约束（ADR-0005）

### Command
- ✅ 返回 void / ID
- ❌ 返回 DTO
- ❌ 依赖 DTO 做业务判断

📌 ADR-0005 §2.1

### Query
- ✅ 返回 DTO
- ❌ 修改状态 / 发布事件

📌 ADR-0005 §2.2

---

## Endpoint 硬约束（ADR-0005）

- ❌ 业务逻辑
- ❌ 直接访问 DB / 领域模型
- ✅ 仅做映射与调度

📌 ADR-0005 §3

---

## 模块通信硬约束（ADR-0001）

- ✅ 事件 / 契约 / 原始类型
- ❌ 直接引用其他模块
- ❌ 同步跨模块命令

📌 ADR-0001 §2.2

---

## 危险信号（发现即 Blocked）

- using Modules.OtherModule
- *Service / *Manager
- Command 返回 DTO
- Query 修改状态
- Endpoint 含业务逻辑

处理方式：
- 标记 ⚠️ Blocked
- 引用 ADR 正文章节
- 引导查阅 prompts
- 不给实现代码
