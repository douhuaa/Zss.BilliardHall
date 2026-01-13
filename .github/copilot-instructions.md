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
  - 同步调用：`IMessageBus.InvokeAsync()`
  - 异步事件：优先使用级联消息（Handler 返回值），避免显式 `PublishAsync`
  - **禁止**：Shared Service、跨模块直接数据库访问
- **持久化**：Marten (文档数据库) 或 EF Core，通过 `IDocumentSession` 或 `DbContext` 注入到 Handler

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
- ✅ Endpoint 只做映射，不写业务逻辑（逻辑在 Handler）
- ✅ 聚合根包含业务方法，不是贫血模型
- ❌ 拒绝：创建 Shared.Core、Common.Services 等共享层
- ❌ 拒绝：Application/Domain/Infrastructure 分层结构
- ❌ 拒绝：Repository 接口（直接使用 IDocumentSession/DbContext）

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
## 11. Architecture Documentation / 架构文档参考

请参考以下核心文档：

**架构设计**:
- `docs/03_系统架构设计/Wolverine模块化架构蓝图.md` → 完整架构实施指南（29KB）
- `docs/03_系统架构设计/Wolverine快速上手指南.md` → 5分钟上手教程
- `docs/03_系统架构设计/系统模块划分.md` → 6个核心模块定义

**模块示例**:
- `docs/04_模块设计/会员管理模块.md` → Members 模块完整示例（v3.0.0）
- `docs/04_模块设计/打球时段模块.md` → Sessions 模块 + Saga 示例（v2.0.0）
- `docs/04_模块设计/计费管理模块.md` → Billing 模块示例（v2.0.0）

**开发规范**:
- `docs/06_开发规范/Saga使用指南.md` → Wolverine Saga 完整使用指南（跨模块长事务编排）
- `docs/06_开发规范/FluentValidation集成指南.md` → FluentValidation 集成完整指南（输入验证最佳实践）
- `docs/06_开发规范/级联消息与副作用.md` → 级联消息与副作用实践指南（Handler 返回值、IO 分离）

### 11.1 Saga 使用速查

当业务流程跨越多个步骤、需要维护状态或涉及补偿逻辑时，使用 Wolverine Saga。

**快速判定**：
- ✅ 跨模块的长时间运行业务流程（如订单→支付→发货）
- ✅ 需要等待外部事件的流程（如支付回调）
- ✅ 需要补偿/回滚的分布式事务

**核心原则**：
- Saga 只存储必要的状态标识（ID、状态枚举）
- 使用 `Complete()` 显式结束 Saga
- Handler 方法保持幂等性
- 考虑超时处理

**详细指南**: 见 `docs/06_开发规范/Saga使用指南.md`（包含 TableSessionSaga 完整示例、配置、最佳实践）

Add TODO tags:
```
// TODO(wolverine): 若需添加 Saga，参考 Saga 使用指南
// 详细文档：docs/06_开发规范/Saga使用指南.md
// 模块示例：docs/04_模块设计/打球时段模块.md (TableSessionSaga 部分)
```

### 11.2 FluentValidation 集成速查

所有接收外部输入的 Command/Query 都应该有 Validator。

**快速配置**：
```csharp
// Program.cs
builder.Host.UseWolverine(opts => opts.UseFluentValidation());
```

**快速创建**：
```csharp
// 位置：与 Command 同文件夹
public sealed class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    public RegisterMemberValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone).Matches(@"^1[3-9]\d{9}$");
    }
}
```

**验证层级**：
- ✅ **Validator**: 简单验证（非空、格式、长度、范围）
- ✅ **Handler**: 复杂业务规则（库存、状态机、权限）
- ❌ **避免**: Validator 中执行重量级操作（外部 API、复杂查询）

**详细指南**: 见 `docs/06_开发规范/FluentValidation集成指南.md`（包含异步验证、条件验证、自定义规则、测试等）

Add TODO tags:
```
// TODO(validation): 添加 FluentValidation 验证器
// 详细文档：docs/06_开发规范/FluentValidation集成指南.md
// 快速上手：docs/03_系统架构设计/Wolverine快速上手指南.md (场景 1：带验证的 Command)
```

### 11.3 级联消息与副作用速查

Handler 应通过返回值驱动消息发布，将外部 IO 封装为副作用，而非在 Handler 中直接执行。

**级联消息（Cascading Messages）**：
- Handler 返回值自动被 Wolverine 视为需要发布的消息
- 在原始消息事务提交后自动发送
- 常见返回类型：单个事件、`(Result, Event?)` tuple、`OutgoingMessages`

**推荐模式**：
```csharp
[Transactional]
public async Task<(Result, BalanceToppedUp?)> Handle(
    TopUpBalance command,
    IDocumentSession session,
    CancellationToken ct)
{
    var member = await session.LoadAsync<Member>(command.MemberId, ct);
    if (member is null) return (Result.NotFound(...), null);
    
    member.TopUp(command.Amount);
    session.Store(member);
    
    // Wolverine 自动发布返回的事件
    return (Result.Success(), new BalanceToppedUp(...));
}
```

**副作用（Side Effects）**：
- 外部 IO（HTTP、短信、文件等）应封装为 `ISideEffect`
- 存储副作用：`IStorageAction<T>` / `UnitOfWork<T>`

**推荐模式**：
```csharp
// 定义副作用
public class SendWelcomeSms : ISideEffect
{
    public async Task ExecuteAsync(ISmsClient smsClient, ...) { }
}

// Handler 返回副作用
[Transactional]
public async Task<(Result, SendWelcomeSms?)> Handle(...)
{
    // 业务逻辑
    return (Result.Success(), new SendWelcomeSms(...));
}
```

**核心原则**：
- Handler 是"决策者"，不是"执行者"
- 优先使用级联消息而非显式 `PublishAsync`
- 外部 IO 必须封装为副作用，不在 Handler 中直接调用
- 副作用类型必须是具体类（非接口）

**详细指南**: 见 `docs/06_开发规范/级联消息与副作用.md`（包含完整示例、测试策略、Code Review 清单）

Add TODO tags:
```
// TODO(cascading): 使用返回值级联消息，避免显式 PublishAsync
// TODO(side-effect): 外部 IO 封装为 ISideEffect
// 详细文档：docs/06_开发规范/级联消息与副作用.md
```

Must accompany an Issue reference once created.

---
## 12. Review Quick Checklist / 快速审查清单

(✓) Vertical Slice 结构正确（UseCase 文件夹）
(✓) Handler 使用 [Transactional] 自动事务
(✓) 跨模块通信通过事件，不直接调用
(✓) 优先使用级联消息（返回值），避免显式 `PublishAsync`
(✓) 外部 IO 封装为 ISideEffect，不在 Handler 中直接调用
(✓) 没有明文/硬编码 Secret
(✓) Endpoint 只做映射，逻辑在 Handler
(✓) 日志无敏感泄露，失败路径可追踪
(✓) 新逻辑有测试或声明测试豁免理由
(✓) 没有无意开启的 OIDC grant / CORS 过宽 `*`
(✓) 前端环境变量未提交真实值
(✓) 使用 UTC 时间进行持久化
(✓) 异步方法包含 CancellationToken 参数
(✓) 业务异常包含结构化 Code 或 Result.Fail
(✓) 查询使用 AsNoTracking（只读场景，EF Core）
(❌) 拒绝：Application/Domain/Infrastructure 分层
(❌) 拒绝：Repository/UnitOfWork 接口
(❌) 拒绝：Shared Service 跨模块调用
(❌) 拒绝：Handler 中显式 PublishAsync（应用级联消息）
(❌) 拒绝：Handler 中直接调用外部 IO（应封装为 ISideEffect）

---
## 13. English Summary (Condensed)

Use this section if AI requires English only context:
- **Enforce Vertical Slice Architecture**: NO traditional layering (Application/Domain/Infrastructure), organize by Use Case folders
- **Wolverine Handlers**: Handler is the Application Service, use `[Transactional]` for auto-transactions + Outbox
- **Module Communication**: Prefer cascading messages (return values) for events; use `InvokeAsync()` for sync calls; NO Shared Services
- **Cascading Messages**: Prefer return values over explicit `PublishAsync`; Handler returns events as tuple `(Result, Event?)` or `OutgoingMessages`
- **Side Effects**: Encapsulate external IO (HTTP, SMS, files) as `ISideEffect`; do NOT call external services directly in Handler
- **Data Access**: Inject `IDocumentSession` (Marten) or `DbContext` (EF Core) directly into Handlers; NO Repository pattern
- Structured Serilog logging; never log secrets
- Security: no plaintext credentials, only Authorization Code + PKCE for SPA, use FluentValidation for input validation
- Tests required for new Handler logic; use in-memory Marten/EF Core
- Keep PR small & single-purpose; reject noisy unrelated refactors
- Follow conventional commits; clear module scope
- UTC time for persistence, localization at display layer
- CancellationToken support for async methods
- Business exceptions with Result pattern or structured codes (<Area>:<Key>)
- **Reject**: Creating Application Services, Repositories, UnitOfWork, Shared/Common layers, explicit `PublishAsync` in Handlers, direct external IO calls in Handlers

---
## 14. Updating This File / 更新策略

- 小改动 (补充条目) → 直接 PR 修改
- 结构性变更 → 需在 PR 描述写“Update Copilot Instructions”并说明动机
- 合并后记得同步在团队群/文档公告

---
## 15. Version / 版本

Current instructions version: 1.1.0 (Wolverine + Vertical Slice Architecture + Cascading Messages & Side Effects)

Change Log (local to this file):
- 0.1.0: Initial creation with ABP layering rules
- 0.2.0: Synchronized with 代码风格.md v1.0.0, added UTC/CancellationToken/business exception codes
- 1.0.0: **Major rewrite for Wolverine + Vertical Slice Architecture** - removed ABP layers, added Wolverine Handler patterns, Marten integration, module communication rules
- 1.1.0: Added Cascading Messages & Side Effects guidelines (section 11.3) and updated quick checklist with cascading messages and side effects items


---

> 若 AI 建议违反任一硬性约束（安全/垂直切片/命名），应优先提示开发者并拒绝直接生成不合规实现。
> **核心原则**：100% 垂直切片，拒绝传统分层，Handler 即 Application Service。
