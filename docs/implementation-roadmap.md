# 实施路线图（Implementation Roadmap）

> 版本：v0.1  
> 最近更新：2025-09-21  
> 适用范围：支撑无人自助台球系统 V0.1 ~ V0.3 的分阶段实施。  
> 关联文档： [PRD](prd/PRD.md) · [Product Story](product-story.md) · [流程与状态](design/flow-and-states.md) · [数据库模型](design/database-schema.md) · [Tracking Plan](analytics/tracking-plan.md)

---

## 1. 阶段总览

| 阶段 | 时间粒度(建议) | 目标 | 核心退出标准 |
|------|----------------|------|--------------|
| Sprint 0 基线准备 | 1 周 | 明确 MVP 边界与技术基座 | 范围冻结、架构草图、DDL v1、环境可用 |
| V0.1 可用闭环 | 2~3 周 | 开台→计费→支付→关台最小闭环 (单门店/单价/无预约) | 20+ 连续会话稳定，P0 埋点准确率 ≥95% |
| V0.2 体验增强 | 3 周 | 预约、套餐/会员基础、告警→工单 | 预约使用率>30%，在线率统计可视，告警→工单闭环 |
| V0.3 增长与多门店 | 3 周 | 分摊支付、基础营销、 多门店隔离 | 分摊误差=0，多门店权限隔离正确 |

---

## 2. Sprint 0 基线准备（Week 1）

**目标：** 清晰最小范围 + 技术基础搭建 + 数据/事件契约冻结。

### 2.1 范围冻结与澄清

- V0.1 排除：预约 / 套餐 / 多门店 / 分摊支付 / 营销。
- 计费：固定单价 * `ceil(分钟)`；冻结金额 = 最终金额；无优惠叠加。
- 设备交互：灯控指令协议、心跳周期(30s)、失败重试策略。

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

## 3. V0.1 可用闭环（Week 2-4）

**目标：** 真/模拟设备完成 20+ 会话闭环；数据可追踪。

### 3.1 功能域

- 扫码开台：Idle 校验 → 创建 session → 灯控 On → 返回 `session_id`。
- 计时：结束时计算 `ceil((now - start)/60)`；写 `billing_snapshot`。
- 结束冻结：`requestClose` → 冻结金额 → 生成支付单。
- 支付确认：回调 → 幂等确认 → 灯灭 → 会话闭合。
- 灯控：抽象 `IDeviceCommandService`，失败重试指数回退。

### 3.2 状态机实现

- 并发控制：行锁/乐观锁防重复开台。
- 结束幂等：重复回调不改变最终状态。

### 3.3 数据与事件

- `billing_snapshot`：无优惠简化字段（原始分钟/单价/总额）。
- `event_log`：记录关键事件（含 request_id）。

### 3.4 心跳

- `POST /device/heartbeat` 写 `device_heartbeat`。
- 离线判断 = 连续 N(≥3) 周期缺失（V0.1 仅记录，不触发告警流程）。

### 3.5 埋点

- 验证字段完整、事件顺序与状态机一致。

### 3.6 质量

- 单元：状态迁移/计费/支付幂等/灯控重试策略。
- 集成：全链路模拟（FakeDevice + FakePaymentChannel）。
- 性能：并发 10 会话下 P95 开台 < 300ms。

### 3.7 可观测

- Metrics：`sessions_active_gauge`, `payment_success_total`, `heartbeat_delay_histogram`。
- Log → 事件 ID 关联。

### 3.8 退出标准

- 手工 20 次闭环成功率 100%。
- P0 埋点缺失率 <5%。
- 核心查询无全表扫描（Explain 验证）。

---

## 4. V0.2 体验增强（Week 5-7）

**目标：** 预约 / 套餐 / 基础告警→工单。

### 4.1 预约

- 冲突检测：时间窗锁定 (SELECT ... FOR UPDATE)。
- 定时任务：过期释放 → 事件 `reservation_expire`。
- 预约转会话：绑定 `reservation_id`。

### 4.2 套餐/会员

- `membership`：discount_rate 接入计费链。
- `package_consumption`：分钟优先抵扣策略（暂不混合金额）。
- 并发扣减：乐观锁 version + 校验任务。

### 4.3 告警与工单

- 规则：长时间无操作 / 心跳缺失。
- `alarm` 创建 → 人工确认 → `work_order` 生命周期。
- 统计 MTTA / MTTR 基础指标。

### 4.4 扩展埋点

- `reservation_create / reservation_expire / alarm_raise / work_order_create / work_order_close / package_consume / membership_activate`。

### 4.5 报表

- `daily_store_stats` 物化视图：昨日营收 / 利用率 / 预约使用率 / 在线率。

### 4.6 退出标准

- 预约→开台转换正确 10/10 抽测。
- 套餐扣减与金额对账 20 条无误。
- 告警→工单链路 < 60s。
- 在线率报表生成。

---

## 5. V0.3 增长与多门店（Week 8-10）

**目标：** 分摊支付 / 多门店隔离 / 基础营销（首单 + 时段折扣）。

### 5.1 分摊支付

- 新表：`payment_split(user_id, session_id, share_amount, status)`。
- 模式：均分 / 按时长（记录参与 join_time）。
- 兜底：未支付份额到期 → 发起人补全。
- 并发：事务校验剩余未支付金额。

### 5.2 多门店 & 权限

- `user_store_role` 表；API 授权附带 store 过滤。
- Query Filter 强制 store 作用域。

### 5.3 营销首单 & 时段折扣

- `pricing_rule` 增加时段解析；首单判定逻辑 (`first_paid_session`)。
- `billing_snapshot.rule_applied` 保存顺序和解释字段。

### 5.4 指标 & 校验

- 拼单率、首单转化率、活动参与率聚合。
- 自动校验：`payment_success` 必有对应 `billing_snapshot`。

### 5.5 性能 & 扩展

- active_session 缓存 (Redis: `table:{id}:active_session`)。
- 心跳分表（月）策略落地。

### 5.6 退出标准

- 分摊支付 15 组测试金额误差=0。
- 多门店隔离抽测无越权。
- 折扣 + 首单叠加计费正确 10/10。

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

| 风险 | 描述 | 缓解 |
|------|------|------|
| 并发开台 | 多人同时扫码 | 行锁 + 幂等 token |
| 支付回调乱序 | 回调早到或重放 | 回调落库队列 + 幂等校验 |
| 设备心跳抖动 | 误判离线 | 滑动窗口 / 宽限倍数 |
| 套餐并发扣减 | 双重抵扣 | 乐观锁 + 审计对账任务 |
| 分摊支付超时 | 部分未支付 | 超时补全策略 + 定时扫描 |
| 定价复杂度膨胀 | 难测试 | 规则链模式 + Snapshot 明细 |
| 埋点漂移 | 前后端版本不一致 | JSON Schema + CI Diff |

---

## 8. 指标阶段目标

| 指标 | V0.1 | V0.2 | V0.3 |
|------|------|------|------|
| 闭环成功率 | ≥95% | ≥97% | ≥98% |
| 预约使用率 | - | ≥30% | ≥35% |
| 支付成功率 | ≥98% | ≥98% | ≥98% |
| 在线率采集覆盖 | 记录即可 | ≥95% | ≥97% |
| 套餐抵扣占比 | - | 10~15% | 15~20% |
| 分摊支付使用率 | - | - | ≥10% |
| 首单优惠触达率 | - | - | ≥80% |

---

## 9. 周节奏 (示例)

```text
W1  Sprint0 架构/DDL/契约
W2  Session + 状态机 + 计费核心
W3  支付整合 + 设备指令 + 埋点
W4  稳定性/集成测试 → 发布 V0.1
W5  预约 + 告警规则 + 套餐结构
W6  套餐扣减 + 工单闭环 + 指标报表
W7  加固/性能 → 发布 V0.2
W8  分摊支付域模型 + 多门店 RBAC
W9  营销首单/时段折扣 + 计费扩展
W10 指标验证/风控初验 → 发布 V0.3
```

---

## 10. 任务模板示例

**Story:** Implement Session Start (Immediate Open)  
**Acceptance:**  

- POST /api/session/start -> 200 + session_id  
- Table Idle→InUse 原子迁移  
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

- 定价/营销规则 DSL + 策略热更新。
- 实时监控面板（Grafana 仪表预设）。
- E2E (Playwright/Cypress) 自动化。
- DWD/DWS 数仓层建模与调度编排。
- 风控规则管理后台与在线评估。

---
> 如需英文版或拆分成 Jira Epic，可在本文件基础派生。
