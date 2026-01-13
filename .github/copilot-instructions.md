# Copilot Code Review Instructions / 代码审查辅助指令

> Purpose (目的): 为本仓库的 AI 辅助代码审查（如 GitHub Copilot / 代码建议工具）提供统一上下文，保证建议符合项目规范、架构边界与安全要求。
>
> Scope (范围): C# 后端 (Wolverine + Marten/EFCore + Vertical Slice)、前端 (Nuxt SPA OIDC 集成)，以及通用 DevOps/配置文件。

---
## 1. Architecture: Vertical Slice / 架构：垂直切片

**核心原则**：100% 垂直切片架构，**拒绝传统分层**（无 Application/Domain/Infrastructure 分层）

Backend uses Wolverine + Vertical Slice:
- **一个 Use Case = 一个文件夹**：每个业务功能独立组织（Command + Handler + Endpoint + Validator + Event）
- **模块边界**：按业务能力划分模块（Members/Sessions/Billing/Payments/Devices），不是技术层
- **Handler 即 Application Service**：不再需要单独的 Service 层，Handler 是一等公民
- **跨模块通信**：
  - 同步调用：`IMessageBus.InvokeAsync()`（**仅限进程内模块**）
  - 异步事件：优先使用级联消息（Handler 返回值），避免显式 `PublishAsync`
  - **禁止**：Shared Service、跨模块直接数据库访问、跨服务使用 InvokeAsync
- **持久化**：Marten (文档数据库) 或 EF Core，通过 `IDocumentSession` 或 `DbContext` 注入到 Handler
- **BuildingBlocks 准入**：必须同时满足 5 条（3+ 模块真实使用、跨模块不可避免、无业务语义、稳定契约、**抽象后修改成本真的降低**）

**Vertical Slice 标准结构**：
```
Modules/Members/
├── RegisterMember/
│   ├── RegisterMember.cs           # Command (record)
│   ├── RegisterMemberHandler.cs    # Handler with [Transactional]
│   ├── RegisterMemberEndpoint.cs   # HTTP Endpoint with [WolverinePost]
│   └── RegisterMemberValidator.cs  # FluentValidation (optional)
├── TopUpBalance/
│   ├── TopUpBalance.cs
│   ├── TopUpBalanceHandler.cs
│   └── TopUpBalanceEndpoint.cs
├── Member.cs                       # 聚合根
├── MemberTier.cs                   # 枚举/值对象
└── MembersModule.cs                # Wolverine 模块扫描标记
```

Review Checklist (Vertical Slice):
- ✅ UseCase 文件夹包含 Command/Handler/Endpoint，不跨文件夹复用
- ✅ Handler 使用 `[Transactional]` 自动事务，无需手动 SaveChanges
- ✅ 跨模块通信通过事件（优先级联消息），不直接调用其他模块 Handler
- ✅ 跨服务通信使用事件或 HTTP，**禁止**跨服务使用 InvokeAsync
- ✅ Endpoint 只做映射，不写业务逻辑（逻辑在 Handler）
- ✅ 聚合根包含业务方法，不是贫血模型
- ✅ Handler 行数 ≤ 40 行（41-60 需 Review，> 60 禁止合并）
- ❌ 拒绝：创建 Shared.Core、Common.Services 等共享层
- ❌ 拒绝：Application/Domain/Infrastructure 分层结构
- ❌ 拒绝：Repository 接口（直接使用 IDocumentSession/DbContext）
- ❌ 拒绝：BuildingBlocks 中放业务规则（如 ErrorCodes.Tables.CannotReserveAtNight）

---
## 2. Naming & Style / 命名与风格

Follow `docs/06_开发规范/代码风格.md` (已更新至 v1.0.0):
- PascalCase: Classes / Interfaces / Public members; 接口前缀 I
- camelCase + `_` prefix for private fields
- Avoid 模糊命名: `DoSomething`, `Manager2`, `HelperX`
- 只在需要保护语义时使用 `var`（类型明显 / 匿名类型 / LINQ）
- Allman brace style; 强制使用 `async` 后缀 `Async`
- 时间统一使用 UTC（持久化 DateTime.UtcNow，展示层本地化）
- 公开异步方法优先接受 CancellationToken cancellationToken = default

Reject / 标记风险:
- 匈牙利命名、下划线 public 成员
- 过长方法 > 50 行（建议拆分）
- God Class（单类关注点过多，如 *Manager* 拥有 CRUD + 领域规则 + 整合外部）
- 使用 DateTime.Now 而非 DateTime.UtcNow（除展示/日志场景）
- 滥用 null-forgiving 操作符 `!`（需添加注释说明原因）

---
## 3. Logging & Observability / 日志与可观测性

参考 `docs/06_开发规范/日志规范.md`，关键要点：
- 使用 Serilog 结构化日志：`LogInformation("{Action} {Entity} {@Payload}", ...)`
- 标识字段统一：`{UserId}` `{TableId}` `{SessionId}` `{CorrelationId}`
- 不记录敏感值（密码 / Secret / Token / 完整手机号）
- 失败路径必须包含：相关标识与错误上下文
- 金额格式化使用 `:F2`，避免直接输出浮点内部表示
- 避免在高频循环中使用字符串拼接日志
- 在跨服务边界前（调用外部 API / MQ）记录关键上下文

---
## 4. Security Checklist / 安全检查清单

When reviewing changes, ensure:
- 未新增明文凭据（检查 appsettings / .env / docker compose）
- OIDC 授权配置未意外重新开启 password/client_credentials（若有需标注原因）
- 输入验证：Wolverine Endpoint 避免盲目接受复杂对象，使用 FluentValidation
- 授权：新增 Endpoint 是否添加 `[Authorize]` 或显式 `[AllowAnonymous]`（后者需说明）
- 防止 N+1：查询使用 Include/Select 投影而不是多次循环查询
- 不在日志或异常消息中输出个人隐私数据
- 业务异常使用统一 Code 格式：`<Area>:<Key>`（如 `Billing:TableUnavailable`）
  - ⚠️ **ErrorCodes 陷阱**：ErrorCodes 只表达"失败类型"（NotFound/InvalidStatus），不表达"业务决策原因"（CannotReserveAtNight）
  - 业务决策相关错误码必须在模块内定义，不放入 BuildingBlocks
- Handler 输入验证：使用 `UseFluentValidation()` 中间件或 Result 模式返回错误

---
## 5. PR Scope & Structure / PR 范围与结构

- 单一目的：避免混合“功能 + 重构 + 依赖升级”
- 尺寸建议：< 400 行 diff, < 10 文件；超出请提示拆分
- 包含必要验证说明：迁移/配置/安全影响是否覆盖
- 变更 public contract（DTO / 接口）需附兼容性说明
- 新增第三方包：说明必要性与最小替代评估

Reject if:
- 同时修改大量与主需求无关文件（噪音）
- 引入临时调试代码（Console.WriteLine / TODO 留空无解释）

---
## 6. Testing Expectations / 测试期望

For feature PR:
- Application / Domain 新增逻辑 → 对应单元测试或最少一个集成测试
- Bug 修复 → 附回归测试（失败先红后绿）
- 纯文档/配置变更 → 可注明“Tests: N/A”

Check:
- 测试命名：`MethodName_条件_期望()` / 语义化英文均可
- 不在测试中访问真实外部服务（使用 stub / in-memory）
- 断言精确（避免只断言非 null）
- Arrange / Act / Assert 清晰分段（可用空行或注释标识）
- 针对新增公共业务规则：至少 1 个"正常路径" + 1 个"异常/边界"用例

---
## 7. Data Access: Marten / EF Core / 数据访问规范

**Marten (推荐)**:
- Handler 直接注入 `IDocumentSession`，无需 Repository
- 使用 `[Transactional]` 自动事务 + Outbox（自动发布事件）
- 查询：`session.Query<T>().Where()` 或 `session.LoadAsync<T>(id)`
- 只读查询：默认已优化，无需额外标记
- 事件存储：聚合根修改后，事件自动持久化到 Outbox

**EF Core (混合使用)**:
- Handler 直接注入 `DbContext`（不创建 Repository）
- 迁移命名：`yyyyMMddHHmm_<summary>`
- 批量查询使用 `AsNoTracking()`（只读场景）
- 避免 N+1：用投影（`Select(new Dto { ... })`）或 `Include`
- 分页：先排序再分页；大页（>1000）考虑游标策略

**通用规范**:
- 异常处理：数据库唯一约束 → 翻译成业务域错误（返回 `Result.Fail`）
- 批量存在性校验用 `AnyAsync()` 而不是 `Count()`
- ❌ 拒绝：创建 IRepository<T>、IUnitOfWork 等抽象

---
## 8. Frontend (Nuxt) Integration / 前端集成

- OIDC 仅使用 Authorization Code + PKCE（若发现 password 流代码需提示移除）
- `.env.example` 不放真实 Secret，`NUXT_SESSION_SECRET` 占位引导用户生成
- API Endpoint、Authority、Redirect URI 应配置化，不硬编码在组件
- 不在前端仓库提交生成产物（如 `.output` / `dist`）

---
## 9. Commit & Conventional Messages / 提交信息

首选类型: `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`, `chore`, `security`.
示例:
- `feat(table-session): 支持台球桌暂停与恢复`
- `fix(payment): 修正重复扣费 race condition`
- `security(oidc): 移除 password grant`
使用范围 (scope) 明确模块（如 `table-session`, `payment`, `auth`）。
只用中文，但保持关键词英文，如 `fix`, `feat`。

Reject if:
- 提交信息为单字/模糊（`update`, `misc`, `tmp`）
- 批量无说明 squash 后仍不清晰

---
## 10. AI Suggestion Guardrails / AI 建议防护

When auto-generating code, enforce:
- 不创建未使用的 Helper / Util 类
- 避免过度抽象（抽象层 < 3 才考虑再提炼）
- 如果生成包含外部依赖，引导先讨论而非直接添加
- 不自动引入未批准的加密/安全库

---
## 11. Key Patterns / 关键模式

> 详细文档见 `docs/03_系统架构设计/Wolverine模块化架构蓝图.md` 和 `docs/06_开发规范/`

**Saga 使用（跨模块长事务）**:
- ⚠️ **心理刹车**: 犹豫时默认不用 Saga（重武器，误用会导致状态机地狱）
- 必须**全部满足** 3 条：跨模块 + 跨时间（> 1分钟）+ 需补偿
- 详见 `docs/06_开发规范/Saga使用指南.md`

**FluentValidation（输入验证）**:
- 所有外部输入 Command/Query 都应有 Validator
- Validator 做简单验证（非空、格式），Handler 做业务规则
- 详见 `docs/06_开发规范/FluentValidation集成指南.md`

**级联消息与副作用**:
- 优先用返回值级联消息，避免显式 `PublishAsync`
- 外部 IO 封装为 `ISideEffect`，不在 Handler 中直接调用
- Handler 是"决策者"不是"执行者"
- 详见 `docs/06_开发规范/级联消息与副作用.md`

---
## 12. Event Classification & Boundaries / 事件分类与边界

| 事件类型 | 范围 | 存放位置 | 可修改性 |
|---------|------|---------|---------|
| **Domain Event** | 模块内 | `Modules/{Module}/Events/` | ✅ 可自由修改 |
| **Module Event** | 跨模块 | `Modules/{Module}/PublicEvents/` | ⚠️ 需考虑消费者 |
| **Integration Event** | 跨服务 | `BuildingBlocks/Contracts/` | ❌ 只增不改 |

**Module Event**: 必须显式声明（`PublicEvents/` 文件夹或注释标记消费者）  
**Integration Event**: ❌ 不改字段含义、不删字段，✅ 只能加可选字段

```csharp
// ✅ V2: 新增可选字段
public record PaymentCompleted(Guid Id, decimal Amount, string? Currency = "CNY");
// ❌ 错误: 修改字段含义
public record PaymentCompleted(Guid Id, decimal TaxIncludedAmount); // 破坏兼容性！
```

---
## 13. Breaking Rules / 何时打破规则

**可破例场景**: 小模块（< 5 UseCase）、内部工具、管理后台 CRUD（60 行）、原型  
**破例铁律**: 写理由、评估影响、设还款计划、团队共识  
**绝对红线**: BuildingBlocks 放业务规则、跨服务 InvokeAsync、传统分层、Shared Service、破坏 Integration Event 兼容性

> **终极判断**: 破例后，三年后的团队是否更难维护？

---
## 14. Review Quick Checklist / 快速审查清单

**架构 & Handler**:
- (✓) Vertical Slice 结构、Handler ≤ 40 行（> 60 = 认知崩溃）
- (✓) Handler 用 [Transactional]、跨模块通过事件、跨服务禁 InvokeAsync
- (✓) 级联消息（返回值）、外部 IO 封装 ISideEffect
- (✓) Endpoint 只映射、聚合根有业务方法

**事件 & 错误码**:
- (✓) Module Event 显式声明（PublicEvents/）、Integration Event 只增不改
- (✓) ErrorCodes 只表达失败类型（非业务决策）
- (✓) BuildingBlocks 满足 5 条（含修改成本降低）、无业务规则
- (✓) Saga 满足 3 条铁律或避免使用

**安全 & 质量**:
- (✓) 无明文 Secret、日志无敏感数据、UTC 时间、CancellationToken
- (✓) 业务异常结构化 Code、AsNoTracking（只读 EF）
- (✓) 测试或豁免理由、无意外 OIDC grant/CORS

**拒绝**:
- (❌) 传统分层、Repository/UnitOfWork、Shared Service
- (❌) Handler 显式 PublishAsync、Handler 直接外部 IO
- (❌) BuildingBlocks 放业务规则、跨服务 InvokeAsync

---
## 15. English Summary

- **Vertical Slice**: NO layering, organize by Use Case
- **Handlers**: `[Transactional]`, max 40 lines (> 60 = collapse), cascading messages
- **Communication**: InvokeAsync **within process only**, NO cross-service
- **Events**: Domain (internal), Module (explicit PublicEvents/), Integration (immutable)
- **ErrorCodes**: Failure types only, NOT business decisions
- **BuildingBlocks**: 5 criteria (cost reduction), NO business rules
- **Saga**: Use only if ALL 3 met; default NO
- **Reject**: Layering, Repositories, Shared Services, explicit PublishAsync, direct IO

---
## 16. Version / 版本

**Current**: 1.2.0 (Wolverine + Vertical Slice + v1.2.0 强化)

**v1.2.0** (2026-01-13): 
- 🛡️ BuildingBlocks 第 5 条、ErrorCodes 陷阱、Module/Integration Event 规范、跨服务禁 InvokeAsync
- 💡 Saga 心理刹车、Handler 认知负债
- 📖 事件分类、破例机制、**压缩整理（489→270 行，-45%）**

---

> **核心**：100% 垂直切片，Handler 即 Application Service。
