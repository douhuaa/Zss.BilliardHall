# 实施路线图（Implementation Roadmap）

## 1. 概述

本文档定义了自助台球系统的实施路线图，按阶段目标与退出标准进行组织。

---

## 2. Sprint 0 基线准备（Week 1）

**目标：** 清晰最小范围 + 技术基础搭建 + 数据/事件契约冻结。

### 2.1 范围冻结与澄清

- 明确 V0.1 功能边界：即时开台 + 计费 + 支付。
- 确认设备集成方式（API 模拟 vs 真实硬件）。
- 梳理关键业务流程与异常场景。

### 2.2 架构与技术选型

- Backend：ASP.NET Core (ABP) 分层（API / Domain / Infra）。
- DB：MySQL 或 PostgreSQL（需 JSON 支持）。
- 事件：抽象 EventBus 接口（初期可内存实现，留 MQ 插槽）。
- 设备网关：初期共进程模拟 `/device/*`，后续拆分。

### 2.3 数据 & DDL

- 仅保留 V0.1 所需表：`store, billiard_table, user, table_session, payment_order, billing_snapshot, device, device_heartbeat, event_log`。
- 生成 `schema.sql` v1（含主键/唯一/关键索引）。
- 采用 Migration 工具（EF Core Migrations/Flyway）。

### 2.4 埋点契约

- 实现 P0：`qr_scan, session_start, session_end_request, billing_frozen, payment_create, payment_success, heartbeat_receive`。
- 统一 `/track` API；事件 JSON Schema versioning。

### 2.5 环境与工程

- Docker Compose：DB + (可选) MQ。
- 健康检查：`/health` (db, cache, message stub)。
- 日志：结构化 + CorrelationId。

### 2.6 风险预埋

- 会话幂等：`session_token`。
- 支付：Sandbox + 回调消息幂等表。

### 2.7 退出检查

- 架构草图 (C4 L2) 评审。
- `schema.sql` 合并；Migration 可重复执行。
- README 更新启动说明。
- Backlog 估点完成。

---

## 3. V0.1 核心链路（Week 2-4）

**目标：** 开台→计费→支付 端到端可用；单门店场景验证。

### 3.1 核心 API 开发

- `POST /api/session/start` - 开台
- `POST /api/session/end` - 结束
- `POST /api/payment/callback` - 支付回调

### 3.2 状态机实现

- Table: Idle → InUse → Billing → Completed
- Session 生命周期管理

### 3.3 计费引擎

- 实时计费 `billing_snapshot` 更新
- 计费规则配置化

### 3.4 设备模拟

- 心跳上报
- 灯控命令响应

### 3.5 退出标准

- 端到端集成测试通过
- 并发测试：50 QPS 开台成功率 >95%
- 支付回调幂等性验证

---

## 4. V0.2 体验增强（Week 5-7）

**目标：** 预约 + 套餐 + 告警机制；运营数据可视化。

### 4.1 预约功能

- 时段预约与占用
- 预约过期处理

### 4.2 套餐与会员

- 套餐计费规则
- 会员积分机制

### 4.3 监控告警

- 设备离线告警
- 异常订单告警

### 4.4 报表初版

- 日营收统计
- 设备利用率

### 4.5 前端优化

- 开台流程优化
- 支付体验提升

### 4.6 退出标准

- 预约→开台转换正确 10/10 抽测。
- 套餐扣减与金额对账 20 条无误。
- 告警→工单链路 < 60s。
- 在线率报表生成。

---

## 5. V0.3 增长与多门店（Week 8-10）

**目标：** 多门店部署 + 营销功能 + 数据驱动运营。

### 5.1 多门店支持

- 门店隔离
- 跨店用户统一

### 5.2 营销功能

- 优惠券系统
- 新手礼包

### 5.3 数据分析

- 用户行为分析
- 收入构成分析

### 5.4 运维工具

- 配置管理界面
- 运维命令面板

---

## 6. Backlog 泳道种子

| 类别 | 高优先(V0.1) | 中 | 低 |
|------|---------------|----|----| 
| API | 开台/结束/支付回调 | 设备心跳 | 报表查询 |
| Domain | 状态机/计费 | 幂等服务 | 事件总线抽象 |
| Infra | Migration/日志/指标 | MQ 接入 | 缓存策略化 |
| Frontend | 开台/计时 UI | 支付优化 | 报表面板 |
| Device | Heartbeat 模拟 | 灯控失败重试 | OTA 升级占位 |
| Data | billing_snapshot 基础 | 日聚合任务 | 行为序列分析 |
| QA | 全链路集成测试 | 负载测试 | Chaos 注入 |

---

## 7. 风险与缓解

| 风险 | 概率 | 影响 | 缓解措施 |
|------|------|------|----------|
| 支付集成延期 | 中 | 高 | 提前 Sandbox 验证 |
| 设备硬件不稳定 | 高 | 中 | 模拟器 + 容错机制 |
| 并发计费错误 | 中 | 高 | 分布式锁 + 补偿机制 |

---

## 8. 指标阶段目标

| 阶段 | 成功开台率 | 支付成功率 | 设备在线率 | 响应时间(P95) |
|------|------------|------------|------------|---------------|
| V0.1 | >90% | >95% | >80% | <500ms |
| V0.2 | >95% | >98% | >90% | <300ms |
| V0.3 | >98% | >99% | >95% | <200ms |

---

## 9. 周节奏 (示例)

**Week 1 (Sprint 0)**
- Mon-Tue: 架构设计 + DDL
- Wed-Thu: 工程搭建 + CI
- Fri: 评审与调整

**Week 2-4 (V0.1)**
- 每周三: Demo + 迭代评审
- 每周五: 集成测试 + 部署

---

## 10. 任务模板示例

**用户故事：** 实现会话开始（即时开台）  
**验收标准：**  

- POST /api/session/start -> 200 + session_id  
- 球台状态 Idle→InUse 原子迁移  
- billing_snapshot 初次写入（原始分钟=0）  
- 事件 session_start 记录 event_log  
- 并发 2 次只成功 1 条（集成测试）  

---

## 11. 立即行动清单（Top 5）

1. 新建 `docs/architecture/` & C4 L2 草图 + ADR 模板。
2. 裁剪并生成 `db/schema.sql` v1。
3. 初始化后端项目 + Migration + HealthCheck。
4. 定义 `events/schema/*.json` P0 事件 Schema。
5. README 增加快速启动与分支/Commit 规范。

---

## 12. 后续可拓展（非本阶段）

- 微服务拆分（用户服务 / 支付服务）
- 事件溯源 (Event Sourcing)
- CQRS 读写分离
- 多租户 SaaS 架构
- IoT 设备管理平台

---

*最后更新: 2024-01*
*版本: v1.0*