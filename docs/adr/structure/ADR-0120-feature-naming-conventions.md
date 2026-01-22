# ADR-0120：功能切片命名规范

**状态**：✅ 已采纳  
**级别**：结构层 / 命名约定  
**适用范围**：所有模块的 Features 目录  
**生效时间**：2026-01-22  
**编号段**：ADR-120~129（命名规范与约定）

---

## 本章聚焦内容（Focus）

本 ADR 定义垂直切片（Vertical Slice）中各类文件和类型的命名规范，明确：
- Command / Query 的命名约定
- Handler 的命名约定
- Endpoint 的命名约定
- DTO 的命名约定
- 目录和文件的命名规范

**不在本 ADR 范围内**：
- 领域模型（Domain Model）的命名（由模块内部定义）
- 数据库实体的命名（由持久化策略决定）
- 契约（Contract）的命名（参见 ADR-0121）

---

## 术语表（Glossary）

| 术语 | 定义 |
|------|------|
| **Feature / Use Case** | 业务功能切片，代表一个完整的用户用例 |
| **Command** | 表达修改系统状态意图的消息 |
| **Query** | 表达查询数据意图的消息 |
| **Handler** | 处理 Command 或 Query 的执行者 |
| **Endpoint** | HTTP API 端点，负责请求/响应适配 |
| **DTO** | Data Transfer Object，数据传输对象 |

---

## 核心决策（Decision）

### 1. 功能切片目录命名

#### 1.1 目录结构模式
**决策**：每个用例对应一个独立目录，目录名使用 PascalCase 业务术语。

```
Features/
├── CreateMember/              # ✅ 使用业务动词 + 实体名
├── GetMemberById/             # ✅ 使用业务动词 + 实体名 + 限定条件
├── UpdateMemberProfile/       # ✅ 业务意图清晰
├── DeactivateMember/          # ✅ 使用业务术语，而非技术术语
└── ListActiveMembers/         # ✅ 查询类用例
```

**命名规则**：
- 使用**业务动词** + **业务实体名**
- 使用 **PascalCase** 格式
- 避免技术术语（如 CRUD）
- 名称应自解释用例意图

**✅ 推荐的业务动词**：
- **Command**：Create, Update, Delete, Activate, Deactivate, Cancel, Approve, Reject, Submit, Process
- **Query**：Get, List, Find, Search, Calculate, Generate

**❌ 避免的技术动词**：
- Save, Load, Insert, Select, Execute（这些是实现细节）

#### 1.2 反模式示例

```
Features/
├── Member/                    # ❌ 缺少动词，意图不明
├── CRUD/                      # ❌ 技术术语
├── MemberCRUD/                # ❌ CRUD 不是业务术语
├── create_member/             # ❌ 使用 snake_case
└── member-management/         # ❌ 过于宽泛，不是具体用例
```

### 2. Command 命名

#### 2.1 Command 类命名
**决策**：Command 类名 = 业务动词 + 业务实体名 + "Command"。

```csharp
// ✅ 正确的 Command 命名
public record CreateMemberCommand;         // 创建会员
public record UpdateMemberProfileCommand;  // 更新会员档案
public record ActivateMemberCommand;       // 激活会员
public record CancelOrderCommand;          // 取消订单
public record ApproveRefundCommand;        // 批准退款
```

**命名规则**：
- 格式：`{动词}{实体}Command`
- 使用业务语言表达意图
- 避免技术术语
- 动词使用**主动语态**

#### 2.2 Command 文件命名
**决策**：文件名 = 类名。

```
CreateMember/
└── CreateMemberCommand.cs     # ✅ 文件名与类名一致
```

#### 2.3 反模式示例

```csharp
// ❌ 错误的 Command 命名
public record MemberCommand;               // 缺少动词
public record SaveMemberCommand;           // 技术术语
public record MemberCreateCommand;         // 顺序错误
public record CreateCommand;               // 缺少实体名
public record CreateMemberDto;             // 不是 Command
```

### 3. Query 命名

#### 3.1 Query 类命名
**决策**：Query 类名 = 业务动词 + 业务实体名 + 限定条件（可选）+ "Query"。

```csharp
// ✅ 正确的 Query 命名
public record GetMemberByIdQuery;          // 按 ID 查询会员
public record ListActiveMembersQuery;      // 列出活跃会员
public record FindMembersByNameQuery;      // 按名称查找会员
public record SearchOrdersQuery;           // 搜索订单
public record CalculateMemberStatisticsQuery;  // 计算会员统计
```

**命名规则**：
- 格式：`{动词}{实体}{限定条件}Query`
- 常用动词：Get（单个）、List（列表）、Find（查找）、Search（搜索）、Calculate（计算）
- 限定条件用于区分同类查询

#### 3.2 反模式示例

```csharp
// ❌ 错误的 Query 命名
public record MemberQuery;                 // 缺少动词和限定条件
public record SelectMemberQuery;           // SQL 术语
public record GetQuery;                    // 缺少实体名
```

### 4. Handler 命名

#### 4.1 Handler 类命名
**决策**：Handler 类名 = Command/Query 名称 + "Handler"。

```csharp
// ✅ 正确的 Handler 命名
public class CreateMemberCommandHandler;        // 处理 CreateMemberCommand
public class GetMemberByIdQueryHandler;         // 处理 GetMemberByIdQuery
public class UpdateMemberProfileCommandHandler; // 处理 UpdateMemberProfileCommand
```

**命名规则**：
- 格式：`{CommandName}Handler` 或 `{QueryName}Handler`
- 一一对应：一个 Command/Query 对应一个 Handler
- Handler 的职责单一明确

#### 4.2 Handler 文件命名
**决策**：文件名 = 类名。

```
CreateMember/
├── CreateMemberCommand.cs
└── CreateMemberCommandHandler.cs          # ✅ 与类名一致
```

#### 4.3 反模式示例

```csharp
// ❌ 错误的 Handler 命名
public class MemberHandler;                // 过于宽泛
public class CreateMemberService;          // 使用 Service 后缀
public class MemberCommandHandler;         // 处理多个 Command
```

### 5. Endpoint 命名

#### 5.1 Endpoint 类命名
**决策**：Endpoint 类名 = 用例名称 + "Endpoint"。

```csharp
// ✅ 正确的 Endpoint 命名
public class CreateMemberEndpoint;         // 映射到 CreateMember 用例
public class GetMemberByIdEndpoint;        // 映射到 GetMemberById 用例
```

**命名规则**：
- 格式：`{UseCaseName}Endpoint`
- 与用例目录名保持一致
- 一个用例可以有多个 Endpoint（不同 HTTP 方法）

#### 5.2 Endpoint 文件命名
**决策**：文件名 = 类名。

```
CreateMember/
├── CreateMemberCommand.cs
├── CreateMemberCommandHandler.cs
└── CreateMemberEndpoint.cs                # ✅ 与类名一致
```

### 6. DTO 命名

#### 6.1 内部 DTO 命名
**决策**：模块内部 DTO 使用业务实体名 + "Dto" 后缀。

```csharp
// ✅ 正确的 DTO 命名
public record MemberDto;                   // 会员数据传输对象
public record OrderSummaryDto;             // 订单摘要数据传输对象
public record MemberStatisticsDto;         // 会员统计数据传输对象
```

**使用场景**：
- Query Handler 的返回值
- Endpoint 的响应数据
- 模块内部数据传递

#### 6.2 Request / Response 命名
**决策**：API 请求/响应对象使用业务动作 + "Request" / "Response"。

```csharp
// ✅ 正确的 Request/Response 命名
public record CreateMemberRequest;         // API 请求
public record CreateMemberResponse;        // API 响应
public record UpdateMemberProfileRequest;
public record MemberDetailsResponse;
```

**与 Command/Query 的区别**：
- **Request/Response**：HTTP API 层面的数据结构
- **Command/Query**：应用层面的意图表达

#### 6.3 DTO 文件位置
**决策**：DTO 文件放在使用它的用例目录中。

```
GetMemberById/
├── GetMemberByIdQuery.cs
├── GetMemberByIdQueryHandler.cs
├── GetMemberByIdEndpoint.cs
└── MemberDto.cs                           # ✅ DTO 放在用例目录中
```

**例外情况**：
- 如果 DTO 被多个用例共享，考虑放在模块的 `Contracts/` 目录
- 如果 DTO 需要跨模块使用，应该放在 `Platform.Contracts` 中

---

## 与其他 ADR 关系（Related ADRs）

### 依赖关系
- **ADR-0001（模块化单体架构）** - 定义了垂直切片的组织原则
- **ADR-0005（应用内交互模型）** - 定义了 Command/Query/Handler 的执行模型

### 补充关系
- **ADR-0121（契约命名规范）** - 定义跨模块契约的命名（待创建）

---

## 快速参考表（Quick Reference）

### 命名模式速查

| 类型 | 命名模式 | 示例 |
|------|---------|------|
| **目录** | `{动词}{实体}{限定条件}` | `CreateMember/` |
| **Command** | `{动词}{实体}Command` | `CreateMemberCommand` |
| **Query** | `{动词}{实体}{限定条件}Query` | `GetMemberByIdQuery` |
| **Handler** | `{Command/Query名}Handler` | `CreateMemberCommandHandler` |
| **Endpoint** | `{用例名}Endpoint` | `CreateMemberEndpoint` |
| **DTO** | `{实体}Dto` | `MemberDto` |
| **Request** | `{动作}Request` | `CreateMemberRequest` |
| **Response** | `{动作}Response` | `CreateMemberResponse` |

### 业务动词速查

| 类型 | 推荐动词 | 避免动词 |
|------|---------|---------|
| **Command** | Create, Update, Delete, Activate, Deactivate, Cancel, Approve, Reject, Submit, Process | Save, Insert, Execute |
| **Query** | Get, List, Find, Search, Calculate, Generate | Select, Load, Retrieve |

### 完整用例示例

```
Features/CreateMember/                     # 用例目录
├── CreateMemberCommand.cs                 # Command
├── CreateMemberCommandHandler.cs          # Handler
├── CreateMemberEndpoint.cs                # Endpoint
├── CreateMemberRequest.cs                 # API Request（可选）
└── CreateMemberResponse.cs                # API Response（可选）
```

---

## 示例与反模式

### ✅ 正确的命名示例

```csharp
// 用例：创建会员
namespace Zss.BilliardHall.Modules.Members.Features.CreateMember;

public record CreateMemberCommand
{
    public required string Name { get; init; }
    public required string Email { get; init; }
}

public class CreateMemberCommandHandler
{
    public async Task<Guid> Handle(CreateMemberCommand command)
    {
        // 处理逻辑
    }
}

public class CreateMemberEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/members", async (CreateMemberRequest request) =>
        {
            // 端点逻辑
        });
    }
}
```

### ❌ 错误的命名示例

```csharp
// ❌ 错误示例1：使用技术术语
public record SaveMemberCommand;           // 应该用 CreateMember 或 UpdateMember
public class MemberService;                // 应该是具体的 Handler

// ❌ 错误示例2：命名不一致
Features/CreateMember/
├── CreateMember.cs                        // 应该是 CreateMemberCommand.cs
├── Handler.cs                             // 应该是 CreateMemberCommandHandler.cs
└── Api.cs                                 // 应该是 CreateMemberEndpoint.cs

// ❌ 错误示例3：过于宽泛
public class MemberHandler;                // 应该是具体用例的 Handler
public record MemberQuery;                 // 缺少动词和限定条件
```

---

## 架构测试保障

本 ADR 的约束由以下测试保障：

1. **命名约定检查** - 验证 Command/Query/Handler/Endpoint 的命名符合规范
2. **文件组织检查** - 验证文件命名与类名一致
3. **用例完整性检查** - 验证每个用例目录包含必要的文件

> **注意**：本 ADR 是结构层 ADR，需要添加对应的架构测试实现。

---

## 常见问题（FAQ）

### Q: Command 和 Request 有什么区别？
**A:**
- **Command**：应用层的意图表达，代表业务操作
- **Request**：HTTP API 层的数据结构，可能包含 API 特定的字段（如 API 版本、追踪ID）
- 在简单场景下，可以直接使用 Command 作为 Request

### Q: 为什么 Handler 不能处理多个 Command？
**A:** 垂直切片架构的核心原则是**用例独立性**：
- 每个 Handler 专注于一个用例
- 避免横向抽象导致的耦合
- 便于独立演进和测试

### Q: DTO 应该放在哪里？
**A:** 根据使用范围决定：
1. **单个用例使用** - 放在用例目录中
2. **模块内多个用例共享** - 放在模块的 `Contracts/` 目录
3. **跨模块使用** - 放在 `Platform.Contracts` 中（参见 ADR-0001）

### Q: 如何命名复杂的查询？
**A:** 使用限定条件细化查询意图：
- `GetMemberByIdQuery` - 按 ID 查询
- `GetMemberByEmailQuery` - 按邮箱查询
- `ListActiveMembersQuery` - 列出活跃会员
- `SearchMembersByNameQuery` - 按名称搜索会员

---

## 参考文档

- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0005：应用内交互模型与执行边界](../constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [Architecture Guide](/docs/architecture-guide.md)

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|---------|
| 1.0 | 2026-01-22 | 初始版本，定义功能切片命名规范 |
