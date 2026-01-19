# Architecture Decision Records (ADR)

> **目的**: 记录 Zss.BilliardHall 项目的关键架构决策，提供可追溯的决策历史和上下文。

## 什么是 ADR？

Architecture Decision Record (ADR) 是一种记录架构决策的轻量级方法。每个 ADR 描述一个具体的架构决策，包括：
- **上下文**: 为什么需要做决策
- **决策**: 做了什么决定
- **后果**: 决策带来的影响
- **替代方案**: 考虑过但未采纳的其他选择

## ADR 列表

### 核心架构原则

| ADR | 标题 | 状态 | 日期 |
|-----|------|------|------|
| [ADR-001](./ADR-001-采用垂直切片架构.md) | 采用垂直切片架构而非传统分层架构 | 接受 | 2026-01-19 |
| [ADR-002](./ADR-002-Handler即ApplicationService.md) | Handler 即 Application Service 的设计决策 | 接受 | 2026-01-19 |
| [ADR-003](./ADR-003-模块间消息通信.md) | 模块间通过消息通信，禁止共享服务 | 接受 | 2026-01-19 |
| [ADR-004](./ADR-004-BuildingBlocks准入标准.md) | BuildingBlocks 严格准入标准（5 条规则） | 接受 | 2026-01-19 |
| [ADR-005](./ADR-005-事件分类体系.md) | 事件分类体系（Domain/Module/Integration Event） | 接受 | 2026-01-19 |

### 通信与协作

| ADR | 标题 | 状态 | 日期 |
|-----|------|------|------|
| [ADR-006](./ADR-006-InvokeAsync使用限制.md) | InvokeAsync 仅限进程内使用的限制 | 接受 | 2026-01-19 |
| [ADR-007](./ADR-007-Saga使用三条铁律.md) | Saga 使用的三条铁律 | 接受 | 2026-01-19 |
| [ADR-008](./ADR-008-级联消息优先策略.md) | 级联消息优先于显式 PublishAsync | 接受 | 2026-01-19 |
| [ADR-009](./ADR-009-跨服务通信模式.md) | 跨服务通信模式选择 | 接受 | 2026-01-19 |

### 代码质量

| ADR | 标题 | 状态 | 日期 |
|-----|------|------|------|
| [ADR-010](./ADR-010-Handler行数限制.md) | Handler 行数限制（40/60/80 规则） | 接受 | 2026-01-19 |
| [ADR-011](./ADR-011-错误码规范.md) | Result<T> 错误码规范（Area:Key 格式） | 接受 | 2026-01-19 |
| [ADR-012](./ADR-012-ErrorCodes约束.md) | ErrorCodes 不承载业务语义的铁律 | 接受 | 2026-01-19 |
| [ADR-013](./ADR-013-IntegrationEvent兼容性.md) | Integration Event 向后兼容性要求 | 接受 | 2026-01-19 |

### 项目结构

| ADR | 标题 | 状态 | 日期 |
|-----|------|------|------|
| [ADR-014](./ADR-014-Solution结构设计.md) | Solution 结构设计 | 接受 | 2026-01-19 |
| [ADR-015](./ADR-015-模块标记设计.md) | 模块标记（Module Marker）设计 | 接受 | 2026-01-19 |
| [ADR-016](./ADR-016-模块黄金结构.md) | 单个模块的黄金结构 | 接受 | 2026-01-19 |

### 治理与例外

| ADR | 标题 | 状态 | 日期 |
|-----|------|------|------|
| [ADR-017](./ADR-017-架构规则破例机制.md) | 架构规则的破例机制和红线清单 | 接受 | 2026-01-19 |

## ADR 生命周期

```
提议 (Proposed)
    ↓
接受 (Accepted) ──→ 废弃 (Deprecated) ──→ 替代 (Superseded)
```

- **提议**: 初始提出，等待评审
- **接受**: 已评审并采纳
- **废弃**: 不再推荐，但可能仍在使用
- **替代**: 已被新的 ADR 取代

## 如何使用 ADR？

### 创建新的 ADR

1. 复制 [ADR-000-ADR模板.md](./ADR-000-ADR模板.md)
2. 按顺序编号（ADR-XXX）
3. 填写决策内容
4. 在本 README 中添加索引条目
5. 提交 Pull Request 评审

### 引用 ADR

在代码或文档中引用 ADR 时，使用以下格式：

```markdown
详见：[ADR-001: 采用垂直切片架构](../ADR/ADR-001-采用垂直切片架构.md)
```

或在代码注释中：

```csharp
// 遵循 ADR-010: Handler 行数限制
// 详见: docs/03_系统架构设计/ADR/ADR-010-Handler行数限制.md
```

### 更新 ADR

- 小修改（修正错别字、增加示例）：直接修改
- 大修改（改变决策）：创建新的 ADR 并标记旧 ADR 为"替代"

## 原则

1. **简洁明了**: 每个 ADR 聚焦一个决策
2. **包含上下文**: 解释为什么需要做决策
3. **记录权衡**: 说明利弊和替代方案
4. **保持不变**: 决策一旦接受，原 ADR 不修改（通过新 ADR 替代）
5. **易于查找**: 使用清晰的命名和索引

## 与架构蓝图的关系

- **架构蓝图**: 完整的架构视图和实践指南
- **ADR**: 蓝图中关键决策的详细记录和推理

ADR 提供"为什么"，蓝图提供"是什么"和"怎么做"。

## 参考资源

- [ADR GitHub Organization](https://adr.github.io/)
- [Documenting Architecture Decisions - Michael Nygard](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)

---

**维护者**: 架构团队  
**最后更新**: 2026-01-19
