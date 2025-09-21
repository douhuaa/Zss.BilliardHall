# 事件Schema目录（Event Schemas）

## P0 核心事件 (V0.1 必需)

| 事件名称 | Schema文件 | 描述 | 用途 |
|----------|------------|------|------|
| qr_scan | [qr_scan.json](qr_scan.json) | 用户扫码行为 | 扫码转化分析 |
| session_start | [session_start.json](session_start.json) | 开台成功 | 利用率、活跃度 |
| session_end_request | [session_end_request.json](session_end_request.json) | 提交结束请求 | 结束行为分析 |
| billing_frozen | [billing_frozen.json](billing_frozen.json) | 金额冻结 | 计费准确性 |
| payment_create | [payment_create.json](payment_create.json) | 支付订单创建 | 支付转化 |
| payment_success | [payment_success.json](payment_success.json) | 支付成功 | 收入确认 |
| heartbeat_receive | [heartbeat_receive.json](heartbeat_receive.json) | 设备心跳接收 | 设备监控 |

## Schema规范

### 通用字段

所有事件必须包含以下通用字段：

```json
{
  "event_type": "string", // 事件类型（固定值）
  "event_time": "ISO8601", // 事件发生时间（UTC）
  "user_id": "integer", // 用户ID（-1表示系统/未登录）
  "store_id": "integer", // 门店ID
  "platform": "enum" // 操作平台：h5/mini_program/admin/device
}
```

### 可选通用字段

```json
{
  "session_id": "integer", // 会话ID（如适用）
  "table_id": "integer", // 球台ID（如适用）
  "user_type": "enum", // 用户类型：new/active/churn_risk/vip
  "network": "enum", // 网络类型：wifi/5g/4g/other
  "app_version": "string" // 应用版本号
}
```

## 版本控制

- Schema版本：v1.0
- JSON Schema Draft: 07
- 向后兼容：新增字段可选，删除字段需要版本升级

## 验证工具

```bash
# 使用ajv验证事件数据
npx ajv validate -s events/schema/qr_scan.json -d event_data.json
```

## API契约

### 统一事件上报接口

```
POST /api/track
Content-Type: application/json

{
  "events": [
    { /* 符合schema的事件对象 */ }
  ]
}
```

### 响应格式

```json
{
  "success": true,
  "processed_count": 1,
  "failed_events": []
}
```

## 后续扩展（V0.2+）

- 批量事件上报
- 事件重试机制
- Schema版本协商
- 实时事件流