# Copilot Code Review Instructions / 代码审查辅助指令

> Purpose (目的): 为本仓库的 AI 辅助代码审查（如 GitHub Copilot / 代码建议工具）提供统一上下文，保证建议符合项目规范、架构边界与安全要求。
>
> Scope (范围): C# 后端 (ABP + OpenIddict + EFCore + Aspire)、前端 (Nuxt SPA OIDC 集成)，以及通用 DevOps/配置文件。

---
## 1. Architecture & Layering / 架构与分层

Backend uses ABP layers:
- Domain: 纯领域模型与领域服务，不依赖外部基础设施；禁止直接引用 Http / EF Core 具体实现（除抽象仓储接口）。
- Application: 用例编排（DTO ↔ Domain），不放核心业务规则；禁止直接访问 DbContext（通过仓储接口）。
- HttpApi (Host / Controllers): 仅做 Transport 转换、授权、模型校验，不写业务逻辑。
- EntityFrameworkCore: EF Core 上下文、仓储实现、迁移；不要混入领域策略逻辑。
- DbMigrator / AppHost: 仅启动与编排；业务逻辑禁止进入。

Review Checklist (Layering):
- Domain 不引用 Application/HttpApi/EFCore 项目
- 新增服务若跨多个聚合，应重新审视聚合边界
- 不在 Controller 中写 if/loop 复杂规则（下沉 Application/Domain）
- 迁移脚本/上下文不携带演示/测试数据种子（业务种子单独 Contributor）

---
## 2. Naming & Style / 命名与风格

Follow `doc/06_开发规范/代码风格.md` (summarized):
- PascalCase: Classes / Interfaces / Public members; 接口前缀 I
- camelCase + `_` prefix for private fields
- Avoid 模糊命名: `DoSomething`, `Manager2`, `HelperX`
- 只在需要保护语义时使用 `var`（类型明显 / 匿名类型 / LINQ）
- Allman brace style; 强制使用 `async` 后缀 `Async`

Reject / 标记风险:
- 匈牙利命名、下划线 public 成员
- 过长方法 > 50 行（建议拆分）
- God Class（单类关注点过多，如 *Manager* 拥有 CRUD + 领域规则 + 整合外部）

---
## 3. Logging & Observability / 日志与可观测性

(临时规范，正式细则待 `日志规范.md` 完成)
- 使用 Serilog 结构化日志：`LogInformation("{Action} {Entity} {@Payload}", ...)`
- 不记录敏感值（密码 / Secret / Token）
- 失败路径必须包含：相关标识（EntityId, UserId, CorrelationId）
- 避免在高频循环中使用字符串拼接日志
- 在跨服务边界前（调用外部 API / MQ）记录关键上下文

---
## 4. Security Checklist / 安全检查清单

When reviewing changes, ensure:
- 未新增明文凭据（检查 appsettings / .env / docker compose）
- OIDC 授权配置未意外重新开启 password/client_credentials（若有需标注原因）
- 输入验证：公开 API 避免盲目接受复杂对象（必要时使用 DTO 白名单）
- 授权：新增 Endpoint 是否添加 `[Authorize]` 或显式 `[AllowAnonymous]`（后者需说明）
- 防止 N+1：仓储查询是否使用 Include/Select 投影而不是多次循环查询
- 不在日志或异常消息中输出个人隐私数据

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

---
## 7. EF Core & Data / 数据访问规范

- 禁止在 Application 层直接使用 DbContext（应走仓储）
- 迁移命名：`yyyyMMddHHmm_<summary>`
- 批量查询使用 `AsNoTracking()`（只读场景）
- 更新只跟踪需要的实体，不进行全表 `UpdateRange` 无选择
- 异常处理：数据库唯一约束 → 翻译成业务域错误而非直接抛内部异常

---
## 8. Frontend (Nuxt) Integration / 前端集成

- OIDC 仅使用 Authorization Code + PKCE（若发现 password 流代码需提示移除）
- `.env.example` 不放真实 Secret，`NUXT_SESSION_SECRET` 占位引导用户生成
- API Endpoint、Authority、Redirect URI 应配置化，不硬编码在组件
- 不在前端仓库提交生成产物（如 `.output` / `dist`）

---
## 9. Commit & Conventional Messages / 提交信息

Preferred types: `feat`, `fix`, `docs`, `refactor`, `perf`, `test`, `build`, `chore`, `security`.
Examples:
- `feat(table-session): 支持台球桌暂停与恢复`
- `fix(payment): 修正重复扣费 race condition`
- `security(oidc): 移除 password grant`

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
## 11. Pending Docs Placeholders / 待补文档占位

Below documents currently placeholders; interim rules above override ambiguity:
- `分层约束.md` → 已在本文件临时列出最小强约束
- `日志规范.md` → 使用 Serilog 结构化 + 敏感字段脱敏

Add TODO tags:
```
// TODO(layering): 若未来引入 Domain Events, 更新审查标准
// TODO(logging): 添加审计事件字段规范 (UserId, Action, ResourceId)
```
Must accompany an Issue reference once created.

---
## 12. Review Quick Checklist / 快速审查清单

(✓) 分层依赖方向正确
(✓) 没有明文/硬编码 Secret
(✓) 公共接口/DTO 修改有版本或兼容说明
(✓) 日志无敏感泄露，失败路径可追踪
(✓) 新逻辑有测试或声明测试豁免理由
(✓) 没有无意开启的 OIDC grant / CORS 过宽 `*`
(✓) Migration 命名合规且无示例/测试数据
(✓) 前端环境变量未提交真实值

---
## 13. English Summary (Condensed)

Use this section if AI requires English only context:
- Enforce clean layering (Domain isolated, no direct DbContext in Application, controllers thin)
- Structured Serilog logging; never log secrets
- Security: no plaintext credentials, only Authorization Code + PKCE for SPA
- Tests required for new logic; skip must be justified
- Keep PR small & single-purpose; reject noisy unrelated refactors
- Follow conventional commits; clear module scope
- EF Core: use AsNoTracking for read, migrations properly named

---
## 14. Updating This File / 更新策略

- 小改动 (补充条目) → 直接 PR 修改
- 结构性变更 → 需在 PR 描述写“Update Copilot Instructions”并说明动机
- 合并后记得同步在团队群/文档公告

---
## 15. Version / 版本

Current instructions version: 0.1.0 (initial)

Change Log (local to this file):
- 0.1.0: Initial creation with interim logging & layering rules

---

> 若 AI 建议违反任一硬性约束（安全/分层/命名），应优先提示开发者并拒绝直接生成不合规实现。
