# 业务流程与状态机草稿

> 本文档为无人自助台球系统早期流程/状态机定义，后续评审后再稳定。使用 Mermaid 方便后续在文档或看板中渲染。

## 1. 球台状态机 (Table State Machine)

状态说明：

- Idle: 空闲
- Locked: 已被预约/锁定等待开始
- InUse: 使用中（计时中）
- PendingClose: 用户发起结束/待确认（或系统检测到长时间无动作）
- Closed: 已结束，等待结算完成（Session 归档）
- Maintenance: 维护中（不可被预约或开台）
- Error: 异常（心跳丢失 / 设备故障 / 传感器异常）

```mermaid
stateDiagram-v2
    [*] --> Idle
    Idle --> Locked: 预约创建
    Locked --> Idle: 预约过期释放
    Locked --> InUse: 到店开台
    Idle --> InUse: 即时开台
    InUse --> PendingClose: 用户主动结束 / 系统空台检测
    PendingClose --> Closed: 确认结束 & 计费冻结
    InUse --> Error: 设备异常
    PendingClose --> InUse: 取消结束
    Closed --> Idle: 结算完成
    Idle --> Maintenance: 人工设为维护
    Maintenance --> Idle: 维护完成
    Any --> Error: 心跳丢失/硬件告警
    Error --> Maintenance: 转维护工单
    Error --> Idle: 自愈恢复
```

## 2. 开台（即时）流程

```mermaid
sequenceDiagram
    participant U as 用户
    participant H5 as H5/小程序
    participant API as 后端API
    participant DEV as 设备网关
    participant BILL as 计费引擎

    U->>H5: 扫码进入球台页面
    H5->>API: 请求球台状态
    API-->>H5: 状态 Idle
    U->>H5: 点击开台
    H5->>API: CreateSession(tableId)
    API->>BILL: InitSession(tableId,userId,startTime)
    BILL-->>API: SessionCreated(sessionId)
    API->>DEV: 下发灯控开启
    DEV-->>API: Ack(灯已亮)
    API-->>H5: 开台成功(sessionId)
    H5-->>U: 展示计时与预估费用
```

## 3. 预约流程

```mermaid
sequenceDiagram
    autonumber
    participant U as 用户
    participant H5 as H5/小程序
    participant API as 后端
    participant SCHED as 调度器

    U->>H5: 选择时间段
    H5->>API: 校验冲突(tableId,start,end)
    API-->>H5: 可预约
    H5->>API: 创建预约
    API->>API: 写入预约表 (status=Locked)
    API-->>H5: 预约成功(预约编号)
    SCHED->>API: 定时任务扫描过期预约
    API->>API: 过期释放 (状态恢复 Idle)
```

## 4. 关台与计费流程

```mermaid
sequenceDiagram
    autonumber
    participant U as 用户
    participant H5 as H5/小程序
    participant API as 后端
    participant BILL as 计费引擎
    participant PAY as 支付网关

    U->>H5: 点击结束计费
    H5->>API: requestClose(sessionId)
    API->>BILL: Freeze(sessionId)
    BILL-->>API: 返回应付金额
    API-->>H5: 展示费用&支付方式
    U->>H5: 确认支付
    H5->>API: 创建支付单
    API->>PAY: 调起支付渠道
    PAY-->>API: 支付成功回调
    API->>BILL: Confirm(sessionId)
    BILL-->>API: SessionClosed
    API->>API: 更新台状态 Closed -> Idle
    API-->>H5: 支付成功(可分享)
```

## 5. 分摊支付（拼单）逻辑（概要）

```mermaid
flowchart TD
    A[主发起人发起结束] --> B{是否多人加入}
    B -- 否 --> C[直接生成单人支付单]
    B -- 是 --> D[生成待分摊账单草稿]
    D --> E[按参与时长或均分计算份额]
    E --> F[各用户确认支付]
    F --> G{全部支付?}
    G -- 否 --> H[未完成份额由发起人兜底]
    G -- 是 --> I[通知计费引擎确认]
```

## 6. 工单流程（简化）

```mermaid
sequenceDiagram
    autonumber
    participant MON as 监控/告警
    participant OPS as 维护员
    participant API as 后端

    MON->>API: 设备离线告警
    API->>API: 创建工单 (status=Open, priority=High)
    OPS->>API: 领取工单 (assign self)
    OPS->>API: 提交处理记录 (status=Processing)
    OPS->>API: 标记已修复 (status=Resolved)
    API->>MON: 验证心跳恢复
    API->>API: 自动关闭 (status=Closed)
```

## 7. 支付/结算状态流

```mermaid
stateDiagram-v2
    [*] --> INIT: 创建支付单
    INIT --> PENDING: 跳转渠道/待支付
    PENDING --> SUCCESS: 渠道回调成功
    PENDING --> FAILED: 渠道失败
    SUCCESS --> SETTLED: 后端结算（计费确认）
    FAILED --> CANCELED: 用户放弃/超时关闭
    SUCCESS --> REFUNDING: 申请退款
    REFUNDING --> REFUNDED: 完成退款
```

## 8. 计费计算顺序（逻辑图）

```mermaid
flowchart LR
    A[原始时长] --> B[分段规则处理]
    B --> C[基础金额]
    C --> D[时段/节假日调整]
    D --> E[活动减免]
    E --> F[套餐抵扣]
    F --> G[优惠券]
    G --> H[会员折扣]
    H --> I[应付金额]
```

## 9. 关键时序事件（Event List）

| 事件 | 触发来源 | 主体 | 用途 | 下游消费示例 |
|------|----------|------|------|--------------|
| TableHeartbeat | 设备网关 | 球台 | 在线监控 | 在线率统计、告警 |
| SessionStarted | 开台API | 台次会话 | 计费起点 | 计费引擎、增长埋点 |
| SessionEndedRequest | 用户操作 | 台次会话 | 进入冻结 | 支付流程 |
| BillingFrozen | 计费引擎 | 台次会话 | 产生金额 | 支付单生成 |
| PaymentCreated | 支付API | 支付单 | 渠道调起 | 风控、对账 |
| PaymentSucceeded | 支付回调 | 支付单 | 资金确认 | 结算、LTV 统计 |
| SessionClosed | 计费引擎 | 台次会话 | 会话归档 | 数据仓入仓 |
| AlarmRaised | 监控/检测 | 告警 | 运维响应 | 工单系统 |
| WorkOrderCreated | 系统/人工 | 工单 | 维护追踪 | KPI 统计 |

## 10. 后续补充

- 添加多门店跨表调度流程
- 增加风控逃费判定时序
- 分摊支付的并发与锁策略

---
> 若需生成 PNG / SVG，可以后续在 CI 中调用 Mermaid CLI (mmdc) 统一产出。

## 11. 相关文档导航

- [PRD](../prd/PRD.md)
- [数据库概念模型](database-schema.md)
- [埋点追踪方案](../analytics/tracking-plan.md)
