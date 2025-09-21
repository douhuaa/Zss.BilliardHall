# 埋点追踪方案（Tracking Plan）v0.1

> 目标：覆盖 MVP ~ V0.3 的核心行为，支持获客转化、留存、运营效率、营收、设备稳定性
> 5 大类指标。后续可迁移至数据平台 YAML/JSON 配置化。
>
> 相关文档： [PRD](../prd/PRD.md) · [流程与状态](../design/flow-and-states.md) ·
> [数据库模型](../design/database-schema.md)

## 1. 事件命名规范

- 格式：VerbObject / NounAction（英文小写下划线）如 `session_start`, `payment_success`。
- 必填字段：`event_name`, `event_time(ISO8601)`, `user_id(可匿名-1)`, `platform`, `store_id(可空)`。
- 匿名阶段：使用 `temp_user_id` + 设备指纹，注册后做关联绑定映射表。

## 2. 事件列表（核心）

| 优先级 | 事件名称 | 场景 | 关键属性 (Props) | 指标用途 |
|--------|----------|------|------------------|----------|
| P0 | page_view | 进入页面 | page, referrer, is_first_session | PV/UV, 漏斗起点 |
| P0 | qr_scan | 扫码入口 | table_code, store_id, source_channel | 扫码转化 |
| P0 | session_start | 开台成功 | session_id, table_id, store_id, start_type(immediate/reservation), user_type | 利用率、活跃 |
| P0 | session_end_request | 提交结束 | session_id, duration_seconds | 结束行为分析 |
| P0 | billing_frozen | 金额冻结 | session_id, amount, minutes, rules_applied_count | 计费准确性 |
| P0 | payment_create | 支付单创建 | payment_id, session_id, amount, channel, split_count | 转化漏斗 |
| P0 | payment_success | 支付成功 | payment_id, amount, channel, coupon_used, activity_tags | 收入结构 |
| P0 | reservation_create | 预约创建 | reservation_id, table_id, start_time, duration | 预约转化 |
| P0 | reservation_expire | 预约过期 | reservation_id, reason | 预约质量 |
| P0 | heartbeat_receive | 设备心跳 | device_id, table_id, online(1/0), latency_ms | 在线率 |
| P0 | alarm_raise | 告警触发 | alarm_id, category, level | 质量监控 |
| P0 | work_order_create | 工单创建 | work_order_id, alarm_id, priority | 响应效率 |
| P0 | work_order_close | 工单关闭 | work_order_id, duration_minutes | MTTR 指标 |
| P1 | marketing_expose | 活动曝光 | activity_id, position, user_segment | 活动覆盖 |
| P1 | marketing_join | 活动参与 | activity_id, action_type | 参与率 |
| P1 | coupon_receive | 领券 | coupon_id, source, remain_quota | 券拉新 |
| P1 | coupon_use | 券使用 | coupon_id, session_id, discount_value | 效果评估 |
| P1 | package_consume | 套餐抵扣 | package_id, minutes_used, value_deducted | 套餐占比 |
| P1 | membership_activate | 激活会员 | level, period_days | 会员渗透 |
| P2 | share_action | 分享行为 | channel, target(page/session) | 裂变传播 |
| P2 | ai_feedback_view | AI 反馈查看 | session_id, feature_type | 增值使用 |
| P2 | ai_feedback_purchase | AI 功能付费 | feature_type, amount | 增值收入 |

## 3. 属性字段规范

必填通用：

- user_id: LONG（未登录=-1）
- session_id: LONG（无则空）
- store_id: LONG
- table_id: LONG
- event_time: ISO8601 字符串（UTC）
- platform: 枚举 h5 / mini_program / admin / device

通用扩展字段：

- user_type: new / active / churn_risk / vip
- network: wifi / 5g / 4g / other
- app_version: 语义化版本

## 4. 指标映射

| 指标 | 计算逻辑 | 依赖事件 |
|------|----------|----------|
| 扫码→开台转化率 | session_start / qr_scan | qr_scan, session_start |
| 预约使用率 | (session_start with start_type=reservation) / reservation_create | reservation_create, session_start |
| 单台利用率 | SUM(session.duration)/可营业总时长 | session_start, session_end_request |
| 支付成功率 | payment_success / payment_create | payment_create, payment_success |
| 工单平均修复时间(MTTR) | AVG(work_order_close.duration_minutes) | work_order_create, work_order_close |
| 告警工单化率 | work_order_create / alarm_raise | alarm_raise, work_order_create |
| 套餐收入占比 | SUM(package_consume.value_deducted) / SUM(payment_success.amount) | package_consume, payment_success |
| 券转化率 | coupon_use / coupon_receive | coupon_receive, coupon_use |
| 活动参与率 | marketing_join / marketing_expose | marketing_expose, marketing_join |
| 新用户留存(D7) | D7 回访用户数 / D0 新用户数 | page_view, session_start |

## 5. 事件校验与反作弊

- 重复事件：使用 `(event_name, user_id, session_id, event_time 秒级)` 幂等窗口。
- 设备心跳：延迟 > 阈值记 latency 异常字段。
- 异常扫码：同设备短时多次 `qr_scan` 聚合标注 `suspect_batch`。

## 6. 数据流转架构（逻辑）

1. 前端 / 设备 → SDK / 网关 → Kafka topic (events_raw)
2. ETL 清洗：补充 user_segment、活动标签
3. 落地 DWD 层：分类型宽表（session/payment/device）
4. DWS 聚合：日活、利用率、营收、心跳在线率
5. ADS 指标：看板/报表/算法输入

## 7. SDK 接入要求

- H5/小程序：统一 js snippet，失败重试与本地缓存（IndexedDB/Storage）。
- 管理后台：重要操作事件（规则变更、价格更新）写审计。
- 设备端：心跳与异常合并打包减少带宽。

## 8. 数据质量监控

| 监控项 | 规则 | 告警 |
|--------|------|------|
| 事件吞吐 | 与基线相比 ±30% | 邮件/IM |
| 丢失率 | events_raw vs cleaned 差异 >1% | 告警 |
| 心跳采集间隔 | > 3*周期 | 告警 |
| 支付成功无结算 | payment_success 后 5 分钟无计费确认 | 告警 |

## 9. 后续迭代

- 引入用户行为 session 划分（30min idle）
- 增加路径分析（开台前操作序列）
- 引入埋点自动化校验（CI 对 schema 校对）

---
> 后续可导出为 Looker/Metabase 指标字典；也可接入埋点管理平台。
