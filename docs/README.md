# 文档总览

| 分类 | 文档 | 说明 |
|------|------|------|
| 产品概述 | [PRD](prd/PRD.md) | 产品需求骨架 |
| 产品故事 | [Product Story](product-story.md) | 用户与价值叙述 |
| 流程与状态 | [流程与状态](design/flow-and-states.md) | 主要流程与状态机 |
| 数据库 | [数据库模型](design/database-schema.md) | 概念模型与字段建议 |
| 初始 DDL | [schema.sql](../db/schema.sql) | MySQL DDL v1.0 (V0.1精简版) |
| 埋点方案 | [Tracking Plan](analytics/tracking-plan.md) | 事件与指标映射 |
| 实施路线图 | [Implementation Roadmap](implementation-roadmap.md) | 分阶段目标与退出标准 |
| **Sprint 0 新增** | | |
| 架构设计 | [C4 L2容器图](architecture/c4-level2-containers.md) | 系统架构与ADR |
| 事件Schema | [事件定义](../events/schema/README.md) | P0事件JSON Schema |
| Backlog估点 | [工作量估算](backlog-estimation.md) | Sprint与Story估时 |

## 后续建议

- [x] 将最初产品故事迁移为 `docs/product-story.md` 版本化管理
- [ ] 引入架构图（逻辑/部署/上下游）
- [ ] 增加 API 合同（OpenAPI 规范）
- [ ] 引入测试数据与数据字典

---
可继续拓展：API 合同 / 架构图 / 定价策略示例。
