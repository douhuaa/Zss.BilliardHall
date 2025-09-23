# 埋点追踪方案（Tracking Plan）v0.1

## 1. 概述

本文档定义了自助台球系统的数据埋点策略，涵盖核心业务事件、技术指标采集与数据质量监控。

---

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

## 3. 事件属性详细定义

### 3.1 通用属性（所有事件）

```json
{
  "event_time": "2024-01-15T10:30:45.123Z",
  "user_id": "usr_abc123",
  "session_id": "sess_xyz789",
  "store_id": "store_001",
  "device_id": "dev_terminal_01",
  "app_version": "v1.2.3",
  "platform": "h5|miniprogram|admin",
  "correlation_id": "req_uuid_12345"
}
```

### 3.2 核心事件 Schema

**qr_scan**
```json
{
  "event_name": "qr_scan",
  "table_code": "TBL001-QR789",
  "store_id": "store_001",
  "source_channel": "wechat|alipay|direct",
  "scan_result": "success|invalid|expired"
}
```

**session_start**
```json
{
  "event_name": "session_start",
  "session_id": "sess_xyz789",
  "table_id": "tbl_001",
  "store_id": "store_001",
  "start_type": "immediate|reservation",
  "user_type": "member|guest",
  "reservation_id": "rsv_abc123"
}
```

**billing_frozen**
```json
{
  "event_name": "billing_frozen",
  "session_id": "sess_xyz789",
  "amount": 4500,
  "minutes": 90,
  "rules_applied_count": 2,
  "billing_rules": ["hourly_rate", "member_discount"]
}
```

**payment_success**
```json
{
  "event_name": "payment_success",
  "payment_id": "pay_xyz123",
  "amount": 4500,
  "channel": "wechat|alipay|balance",
  "coupon_used": true,
  "activity_tags": ["new_user", "weekend_promo"]
}
```

**heartbeat_receive**
```json
{
  "event_name": "heartbeat_receive",
  "device_id": "dev_table_01",
  "table_id": "tbl_001",
  "online": 1,
  "latency_ms": 45,
  "battery_level": 85,
  "signal_strength": -60
}
```

---

## 4. 采集技术实现

### 4.1 SDK 埋点

- **前端：** JavaScript SDK (H5/小程序)
- **后端：** .NET Core Middleware
- **设备：** HTTP POST /api/track

### 4.2 统一上报 API

```http
POST /api/track
Content-Type: application/json

{
  "events": [
    {
      "event_name": "session_start",
      "event_time": "2024-01-15T10:30:45.123Z",
      "properties": { /* 事件属性 */ }
    }
  ],
  "schema_version": "v1.0"
}
```

### 4.3 批量 & 实时上报

- **实时：** 支付、告警类关键事件
- **批量：** 页面浏览、心跳（5s 间隔聚合）

---

## 5. 事件校验与反作弊

- 重复事件：使用 `(event_name, user_id, session_id, event_time 秒级)` 幂等窗口。
- 设备心跳：延迟 > 阈值记 latency 异常字段。
- 异常扫码：同设备短时多次 `qr_scan` 聚合标注 `suspect_batch`。

---

## 6. 数据存储 & 处理

### 6.1 存储分层

- **原始层：** event_log 表（JSON 格式，保留 90 天）
- **清洗层：** 按事件类型分表存储（去重、格式化）
- **聚合层：** 按日/小时预聚合指标

### 6.2 实时计算

- **流处理：** 使用 Redis Streams 或 Kafka
- **指标窗口：** 1min / 5min / 1hour 滚动窗口

---

## 7. SDK 接入要求

### 7.1 前端 SDK

```javascript
// 初始化
ZssTracker.init({
  apiEndpoint: '/api/track',
  batchSize: 10,
  flushInterval: 5000
});

// 事件上报
ZssTracker.track('qr_scan', {
  table_code: 'TBL001-QR789',
  source_channel: 'wechat'
});
```

### 7.2 后端埋点

```csharp
// .NET Core 示例
public class TrackingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        
        _tracker.Track("api_request", new {
            path = context.Request.Path,
            method = context.Request.Method,
            status_code = context.Response.StatusCode,
            duration_ms = stopwatch.ElapsedMilliseconds
        });
    }
}
```

---

## 8. 数据质量监控

| 监控项 | 规则 | 告警 |
|--------|------|------|
| 事件吞吐 | 与基线相比 ±30% | 邮件/IM |
| 丢失率 | events_raw vs cleaned 差异 >1% | 告警 |
| 心跳采集间隔 | > 3*周期 | 告警 |
| 支付成功无结算 | payment_success 后 5 分钟无计费确认 | 告警 |

---

## 9. 后续迭代

### v0.2 规划
- 用户画像事件（偏好、行为路径）
- A/B 测试埋点
- 异常会话链路追踪

### v0.3 规划
- 实时个性化推荐事件
- 跨门店用户旅程分析
- 设备 IoT 指标扩展

---

*最后更新: 2024-01*  
*模式版本: v1.0*