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
  - 异步事件：`IMessageBus.PublishAsync()`  
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
- ✅ 跨模块通信通过事件（PublishAsync），不直接调用其他模块 Handler
- ✅ Endpoint 只做映射，不写业务逻辑（逻辑在 Handler）
- ✅ 聚合根包含业务方法，不是贫血模型
- ❌ 拒绝：创建 Shared.Core、Common.Services 等共享层
- ❌ 拒绝：Application/Domain/Infrastructure 分层结构
- ❌ 拒绝：Repository 接口（直接使用 IDocumentSession/DbContext）

---
## 2. Naming & Style / 命名与风格

Follow `doc/06_开发规范/代码风格.md` (已更新至 v1.0.0):
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

参考 `doc/06_开发规范/日志规范.md`，关键要点：
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
- `doc/03_系统架构设计/Wolverine模块化架构蓝图.md` → 完整架构实施指南（29KB）
- `doc/03_系统架构设计/Wolverine快速上手指南.md` → 5分钟上手教程
- `doc/03_系统架构设计/系统模块划分.md` → 6个核心模块定义
- `doc/04_模块设计/会员管理模块.md` → Members 模块完整示例（v3.0.0）
- `doc/04_模块设计/打球时段模块.md` → Sessions 模块 + Saga 示例（v2.0.0）
- `doc/04_模块设计/计费管理模块.md` → Billing 模块示例（v2.0.0）

### 11.1 Saga 使用指南

当业务流程跨越多个步骤、需要维护状态或涉及补偿逻辑时，使用 Wolverine Saga。

**典型场景**：
- 跨模块的长时间运行业务流程
- 需要等待外部事件的流程（如支付回调）
- 需要补偿/回滚的分布式事务

**TableSessionSaga 示例**（完整实现见 `doc/04_模块设计/打球时段模块.md`）：
```csharp
// 位置：Modules/Sessions/Sagas/TableSessionSaga.cs
public sealed class TableSessionSaga : Saga
{
    // Saga 状态
    public Guid SessionId { get; set; }
    public Guid TableId { get; set; }
    public Guid? BillId { get; set; }
    public SessionStatus Status { get; set; }
    
    // 步骤 1: 时段开始
    public void Handle(SessionStarted @event)
    {
        SessionId = @event.SessionId;
        TableId = @event.TableId;
        Status = SessionStatus.Active;
    }
    
    // 步骤 2: 时段结束 → 等待账单计算
    public void Handle(SessionEnded @event)
    {
        Status = SessionStatus.Ended;
    }
    
    // 步骤 3: 账单计算完成 → 等待支付
    public void Handle(BillCalculated @event)
    {
        BillId = @event.BillId;
    }
    
    // 步骤 4: 支付完成 → 时段完成
    public void Handle(PaymentCompleted @event)
    {
        if (@event.SessionId == SessionId)
        {
            Status = SessionStatus.Completed;
            Complete();  // 完成 Saga，自动清理状态
        }
    }
}
```

**Saga 配置**（在 Program.cs 中）：
```csharp
builder.Services.AddMarten(marten =>
{
    marten.Connection(connectionString);
    
    // 注册 Saga，使用 SessionId 作为唯一标识
    marten.Schema.For<TableSessionSaga>()
        .Identity(x => x.SessionId);
});
```

**最佳实践**：
- Saga 只存储必要的状态标识（ID、状态枚举），不存储完整业务对象
- 使用 `Complete()` 或 `MarkCompleted()` 显式结束 Saga
- Saga Handler 方法应保持幂等性
- 考虑添加超时处理（Wolverine 支持 Saga 超时）

Add TODO tags:
```
// TODO(wolverine): 若需添加 Saga，参考 TableSessionSaga 示例
// 详细文档：doc/04_模块设计/打球时段模块.md #section-saga
// 架构指南：doc/03_系统架构设计/Wolverine模块化架构蓝图.md #section-saga
```

### 11.2 FluentValidation 集成指南

所有接收外部输入的 Command/Query 都应该有 Validator，使用 FluentValidation 进行输入验证。

**安装依赖**：
```bash
dotnet add package Wolverine.Http.FluentValidation
```

**全局配置**（在 Program.cs 中）：
```csharp
builder.Host.UseWolverine(opts =>
{
    // 启用 FluentValidation 自动验证
    opts.UseFluentValidation();
    
    // 可选：配置验证失败响应格式
    opts.Policies.AutoApplyTransactions();
});
```

**Validator 示例**：
```csharp
// 位置：Modules/Members/RegisterMember/RegisterMemberValidator.cs
public sealed class RegisterMemberValidator : AbstractValidator<RegisterMember>
{
    public RegisterMemberValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("会员姓名不能为空")
            .MaximumLength(50).WithMessage("会员姓名不能超过50个字符");
        
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("手机号不能为空")
            .Matches(@"^1[3-9]\d{9}$").WithMessage("手机号格式不正确");
        
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrEmpty(x.Email));  // 可选字段
        
        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("初始余额不能为负数");
    }
}
```

**高级验证示例**（异步验证、数据库查询）：
```csharp
public sealed class UpdateMemberPhoneValidator : AbstractValidator<UpdateMemberPhone>
{
    public UpdateMemberPhoneValidator(IDocumentSession session)
    {
        RuleFor(x => x.Phone)
            .NotEmpty()
            .MustAsync(async (phone, ct) =>
            {
                // 异步验证：检查手机号是否已被使用
                var exists = await session.Query<Member>()
                    .AnyAsync(m => m.Phone == phone, ct);
                return !exists;
            })
            .WithMessage("该手机号已被注册");
    }
}
```

**条件验证示例**：
```csharp
public sealed class StartSessionValidator : AbstractValidator<StartSession>
{
    public StartSessionValidator()
    {
        RuleFor(x => x.TableId)
            .NotEmpty().WithMessage("桌台ID不能为空");
        
        // 条件验证：散客必须提供姓名
        When(x => x.MemberId == null, () =>
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("散客必须提供姓名")
                .MaximumLength(50).WithMessage("姓名不能超过50个字符");
        });
        
        // 条件验证：会员不能提供散客姓名
        When(x => x.MemberId != null, () =>
        {
            RuleFor(x => x.CustomerName)
                .Empty().WithMessage("会员不应提供散客姓名");
        });
    }
}
```

**Validator 命名约定**：
- 文件名：`{Command/Query}Validator.cs`
- 类名：`{Command/Query}Validator`
- 位置：与 Command/Query/Handler 在同一 UseCase 文件夹

**验证失败处理**：
Wolverine 会自动拦截验证失败，返回 400 Bad Request，包含结构化错误信息：
```json
{
  "errors": {
    "Name": ["会员姓名不能为空"],
    "Phone": ["手机号格式不正确"]
  }
}
```

**最佳实践**：
- 简单验证（非空、格式、长度）在 Validator 中处理
- 复杂业务规则（如库存检查、状态机验证）在 Handler 中处理，使用 Result 模式返回
- 避免在 Validator 中执行重量级操作（如外部 API 调用）
- 使用 `When()` 进行条件验证，避免复杂的 if-else 逻辑

Add TODO tags:
```
// TODO(validation): 添加 Wolverine.Http.FluentValidation 验证器
// 参考示例：doc/04_模块设计/打球时段模块.md #StartSessionValidator
// 快速上手：doc/03_系统架构设计/Wolverine快速上手指南.md #section-validation
```

Must accompany an Issue reference once created.

---
## 12. Review Quick Checklist / 快速审查清单

(✓) Vertical Slice 结构正确（UseCase 文件夹）
(✓) Handler 使用 [Transactional] 自动事务
(✓) 跨模块通信通过事件，不直接调用
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

---
## 13. English Summary (Condensed)

Use this section if AI requires English only context:
- **Enforce Vertical Slice Architecture**: NO traditional layering (Application/Domain/Infrastructure), organize by Use Case folders
- **Wolverine Handlers**: Handler is the Application Service, use `[Transactional]` for auto-transactions + Outbox
- **Module Communication**: Use `IMessageBus.PublishAsync()` for events, `InvokeAsync()` for sync calls; NO Shared Services
- **Data Access**: Inject `IDocumentSession` (Marten) or `DbContext` (EF Core) directly into Handlers; NO Repository pattern
- Structured Serilog logging; never log secrets
- Security: no plaintext credentials, only Authorization Code + PKCE for SPA, use FluentValidation for input validation
- Tests required for new Handler logic; use in-memory Marten/EF Core
- Keep PR small & single-purpose; reject noisy unrelated refactors
- Follow conventional commits; clear module scope
- UTC time for persistence, localization at display layer
- CancellationToken support for async methods
- Business exceptions with Result pattern or structured codes (<Area>:<Key>)
- **Reject**: Creating Application Services, Repositories, UnitOfWork, Shared/Common layers

---
## 14. Updating This File / 更新策略

- 小改动 (补充条目) → 直接 PR 修改
- 结构性变更 → 需在 PR 描述写“Update Copilot Instructions”并说明动机
- 合并后记得同步在团队群/文档公告

---
## 15. Version / 版本

Current instructions version: 1.0.0 (Wolverine + Vertical Slice Architecture)

Change Log (local to this file):
- 0.1.0: Initial creation with ABP layering rules
- 0.2.0: Synchronized with 代码风格.md v1.0.0, added UTC/CancellationToken/business exception codes
- 1.0.0: **Major rewrite for Wolverine + Vertical Slice Architecture** - removed ABP layers, added Wolverine Handler patterns, Marten integration, module communication rules


---

> 若 AI 建议违反任一硬性约束（安全/垂直切片/命名），应优先提示开发者并拒绝直接生成不合规实现。
> **核心原则**：100% 垂直切片，拒绝传统分层，Handler 即 Application Service。
