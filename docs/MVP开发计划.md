# Zss.BilliardHall MVP 开发计划（6 周）

> **文档性质**：作战计划，每周更新  
> **目标**：用最小功能集验证架构可行性  
> **最后更新**：2026-01-18

---

## 一、MVP 目标与范围

### 1.1 核心用例（必须完成）

**用户故事：**
> 作为台球厅老板，我需要一个系统能够：
> 1. 登记会员信息
> 2. 开台计时
> 3. 结账时自动给会员增加积分
> 4. 查询会员积分

**系统范围：**
- ✅ 2 个模块：Members + Orders
- ✅ 10–15 个垂直切片
- ✅ 进程内消息通信
- ✅ Marten 存储
- ✅ 基础 REST API

**明确不做：**
- ❌ 会员卡充值
- ❌ 复杂计费规则
- ❌ 统计分析
- ❌ 后台任务
- ❌ 移动端
- ❌ 外部支付

---

## 二、6 周迭代计划

### Week 1：基础设施 + 会员注册

#### 目标
搭建最小可运行系统，完成第一个垂直切片

#### 任务清单

**Platform 层：**
- [ ] 配置 Wolverine（进程内消息）
- [ ] 配置 Marten（PostgreSQL）
- [ ] 配置 Serilog（结构化日志）
- [ ] 实现全局异常处理中间件

**Members 模块：**
- [ ] `RegisterMember` Command + Handler
- [ ] `MemberRegistered` Event
- [ ] `GetMemberById` Query + Handler
- [ ] Member 文档模型

**API：**
- [ ] `POST /api/members/register`
- [ ] `GET /api/members/{id}`

**测试：**
- [ ] 架构测试：模块隔离验证
- [ ] 单元测试：RegisterMemberHandler

#### 验收标准
```bash
# 注册会员
curl -X POST http://localhost:5000/api/members/register \
  -H "Content-Type: application/json" \
  -d '{"name":"张三","phone":"13800138000"}'

# 查询会员
curl http://localhost:5000/api/members/{id}
```

---

### Week 2：开台与结账

#### 目标
完成核心业务流程

#### 任务清单

**Orders 模块：**
- [ ] `StartSession` Command + Handler
- [ ] `SessionStarted` Event
- [ ] `EndSession` Command + Handler
- [ ] `SessionEnded` Event
- [ ] `PaymentCompleted` Event（关键）
- [ ] Session 文档模型
- [ ] Table 文档模型

**Members 模块（订阅事件）：**
- [ ] `PaymentCompletedHandler`：监听支付完成
- [ ] `AddPoints` Command + Handler：增加积分
- [ ] `PointsAdded` Event

**API：**
- [ ] `POST /api/orders/sessions/start`
- [ ] `POST /api/orders/sessions/{id}/end`
- [ ] `GET /api/members/{id}/points`

#### 验收标准
```bash
# 开台
curl -X POST http://localhost:5000/api/orders/sessions/start \
  -d '{"tableId":"T01","memberId":"..."}'

# 结账（自动触发积分增加）
curl -X POST http://localhost:5000/api/orders/sessions/{id}/end \
  -d '{"amount":100}'

# 验证积分已增加
curl http://localhost:5000/api/members/{memberId}/points
# 应返回：100 积分
```

---

### Week 3：最小计费引擎

#### 目标
实现按小时计费

#### 任务清单

**Orders 模块：**
- [ ] `PricingRule` 文档模型（固定时薪）
- [ ] `CalculateFee` 函数：根据时长计算费用
- [ ] `EndSession` Handler 集成计费逻辑

**Platform 层：**
- [ ] 时间工具类（计算时长）

**API：**
- [ ] `GET /api/orders/sessions/{id}/preview`：预览费用

#### 验收标准
```bash
# 开台时间：14:00
# 结账时间：16:30
# 时长：2.5 小时
# 时薪：40 元/小时
# 预期费用：100 元

curl http://localhost:5000/api/orders/sessions/{id}/preview
# 返回：{"amount": 100}
```

---

### Week 4：API 完善 + Swagger

#### 目标
暴露所有 MVP 功能的 HTTP 接口

#### 任务清单

**WebHost：**
- [ ] 配置 Swagger / OpenAPI
- [ ] 配置 CORS
- [ ] 健康检查端点（`/health`）

**Members API：**
- [ ] `GET /api/members`：列表查询
- [ ] `PUT /api/members/{id}`：更新会员信息

**Orders API：**
- [ ] `GET /api/orders/tables`：台桌列表
- [ ] `GET /api/orders/sessions?status=active`：进行中的台桌

**Platform：**
- [ ] 统一响应格式（成功/失败）
- [ ] 验证中间件（FluentValidation）

#### 验收标准
- 访问 `http://localhost:5000/swagger`
- 所有端点可测试
- 错误响应格式统一

---

### Week 5：架构测试补齐

#### 目标
确保架构约束被严格执行

#### 任务清单

**架构测试：**
- [ ] 验证 Members 不依赖 Orders
- [ ] 验证 Orders 不依赖 Members
- [ ] 验证模块内无 Repository/Service 命名
- [ ] 验证模块内无分层目录结构

**集成测试：**
- [ ] 测试完整流程：注册 → 开台 → 结账 → 积分增加
- [ ] 测试并发场景：同一会员多次开台

**Platform 测试：**
- [ ] 测试 Wolverine 消息路由
- [ ] 测试 Marten 文档存取

#### 验收标准
```bash
dotnet test
# 所有测试通过
# 架构约束测试全绿
```

---

### Week 6：整体验收与文档

#### 目标
MVP 可演示交付

#### 任务清单

**功能验收：**
- [ ] 端到端测试：模拟真实业务流程
- [ ] 性能测试：单机并发 100 请求
- [ ] 日志验证：关键操作有日志记录

**文档：**
- [ ] API 使用文档
- [ ] 部署文档（Docker Compose）
- [ ] 架构决策记录（ADR）

**演示准备：**
- [ ] 准备演示数据
- [ ] 准备演示脚本（Postman Collection）

#### 验收标准
- 可以向非技术人员演示完整流程
- 日志清晰，可追踪问题
- 代码通过架构测试

---

## 三、MVP 垂直切片清单

### 3.1 Members 模块（5 个切片）

| 切片名称 | Command/Query | Handler | Event | API |
|---------|--------------|---------|-------|-----|
| 会员注册 | RegisterMember | RegisterMemberHandler | MemberRegistered | POST /members/register |
| 查询会员 | GetMemberById | GetMemberByIdHandler | - | GET /members/{id} |
| 列表会员 | ListMembers | ListMembersHandler | - | GET /members |
| 更新会员 | UpdateMember | UpdateMemberHandler | MemberUpdated | PUT /members/{id} |
| 增加积分 | AddPoints | AddPointsHandler | PointsAdded | - |

---

### 3.2 Orders 模块（7 个切片）

| 切片名称 | Command/Query | Handler | Event | API |
|---------|--------------|---------|-------|-----|
| 开台 | StartSession | StartSessionHandler | SessionStarted | POST /sessions/start |
| 结账 | EndSession | EndSessionHandler | SessionEnded, PaymentCompleted | POST /sessions/{id}/end |
| 预览费用 | PreviewFee | PreviewFeeHandler | - | GET /sessions/{id}/preview |
| 查询台桌 | ListTables | ListTablesHandler | - | GET /tables |
| 查询会话 | GetSession | GetSessionHandler | - | GET /sessions/{id} |
| 查询活跃台桌 | ListActiveSessions | ListActiveSessionsHandler | - | GET /sessions?status=active |
| 创建台桌 | CreateTable | CreateTableHandler | TableCreated | POST /tables |

---

### 3.3 跨模块事件流

```
┌─────────────────────────────────────────────────┐
│          Orders Module                          │
│                                                 │
│  EndSessionHandler                              │
│    ↓                                            │
│  计算费用 → 生成 PaymentCompleted 事件           │
│                                                 │
└─────────────────────┬───────────────────────────┘
                      │
                      │ Wolverine 消息路由
                      ↓
┌─────────────────────────────────────────────────┐
│          Members Module                         │
│                                                 │
│  PaymentCompletedHandler                        │
│    ↓                                            │
│  触发 AddPoints Command                         │
│    ↓                                            │
│  AddPointsHandler → 更新积分 → PointsAdded       │
│                                                 │
└─────────────────────────────────────────────────┘
```

---

## 四、Platform 层 MVP 范围

### 4.1 必须实现

- **Wolverine 配置**
  - 进程内消息总线
  - Handler 自动发现
  - 事件发布/订阅
  
- **Marten 配置**
  - PostgreSQL 连接
  - 文档存储
  - 基础查询

- **日志**
  - Serilog 结构化日志
  - 控制台输出
  - 请求日志中间件

- **异常处理**
  - 全局异常捕获
  - 统一错误响应格式

---

### 4.2 不实现（延后到 Phase 2）

- ❌ Durable Messaging（消息持久化）
- ❌ Outbox Pattern
- ❌ Redis 缓存
- ❌ JWT 认证
- ❌ 外部消息队列（RabbitMQ）
- ❌ 事件溯源（Event Sourcing）

---

## 五、数据模型（MVP）

### 5.1 Members Schema

```sql
-- members.member
{
  "id": "uuid",
  "name": "string",
  "phone": "string",
  "registered_at": "timestamp",
  "total_points": "int"
}

-- members.loyalty_points (事件记录)
{
  "id": "uuid",
  "member_id": "uuid",
  "points": "int",
  "source": "string", -- "消费积分"
  "created_at": "timestamp"
}
```

---

### 5.2 Orders Schema

```sql
-- orders.table
{
  "id": "string", -- "T01"
  "name": "string", -- "1号台"
  "hourly_rate": "decimal", -- 40.00
  "status": "string" -- "available" | "occupied"
}

-- orders.session
{
  "id": "uuid",
  "table_id": "string",
  "member_id": "uuid",
  "start_time": "timestamp",
  "end_time": "timestamp?",
  "status": "string", -- "active" | "completed"
  "amount": "decimal?"
}
```

---

## 六、技术约束

### 6.1 MVP 期间禁止使用

- ❌ EF Core（只用 Marten）
- ❌ Redis（只用内存缓存）
- ❌ RabbitMQ（只用进程内消息）
- ❌ 复杂 LINQ 查询（保持简单）
- ❌ 多表 JOIN（模块间用事件）

---

### 6.2 代码规范

**命名：**
- Command：动词 + 名词（如 `RegisterMember`）
- Event：名词 + 过去式（如 `MemberRegistered`）
- Handler：`{Command/Query}Handler`

**Handler 签名：**
```csharp
// Command Handler
public static async Task<TEvent> Handle(TCommand cmd, IDocumentSession session)

// Event Handler
public static Task Handle(TEvent evt)

// Query Handler
public static async Task<TResult> Handle(TQuery query, IDocumentSession session)
```

---

## 七、风险与应对

### 风险 1：Wolverine 学习曲线

**应对：**
- 先实现 1 个简单切片（RegisterMember）
- 验证消息路由正常后再扩展
- 参考官方示例代码

---

### 风险 2：Marten 文档设计不当

**应对：**
- 保持文档结构扁平
- 避免深层嵌套
- 用事件记录替代复杂关系

---

### 风险 3：跨模块通信理解偏差

**应对：**
- 用最简单的场景验证（PaymentCompleted → AddPoints）
- 加日志跟踪消息流
- 写集成测试覆盖

---

## 八、每周检查点

### Week 1 检查点
- [ ] 能否成功注册会员并查询？
- [ ] 架构测试是否通过？
- [ ] 日志是否清晰？

### Week 2 检查点
- [ ] 能否完成开台 → 结账 → 积分增加？
- [ ] 跨模块事件是否正常路由？

### Week 3 检查点
- [ ] 计费逻辑是否正确？
- [ ] 能否预览费用？

### Week 4 检查点
- [ ] Swagger 是否可用？
- [ ] 所有 API 是否可测试？

### Week 5 检查点
- [ ] 架构测试是否全部通过？
- [ ] 集成测试是否覆盖主流程？

### Week 6 检查点
- [ ] 能否演示完整业务流程？
- [ ] 文档是否齐全？

---

## 九、MVP 后的 Phase 2 规划（简要）

**时间：** Week 7–12

**目标：** 生产就绪

**关键任务：**
- Durable Messaging（消息持久化）
- JWT 认证授权
- 会员卡充值
- 复杂计费规则
- 性能优化
- 监控告警

---

**文档结束**

**下一步行动：** 从 Week 1 的 RegisterMember 切片开始！
