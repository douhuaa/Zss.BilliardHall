# ADR-0121：契约（Contract）命名规范

**状态**：✅ 已采纳  
**级别**：结构层 / 命名约定  
**适用范围**：Platform.Contracts 和跨模块契约  
**生效时间**：2026-01-22  
**编号段**：ADR-120~129（命名规范与约定）

---

## 本章聚焦内容（Focus）

本 ADR 定义跨模块契约（Contract）的命名规范，明确：
- 契约接口的命名约定
- 契约 DTO 的命名约定
- 契约的组织方式
- 契约版本化策略（可选）

**不在本 ADR 范围内**：
- 模块内部 DTO 的命名（参见 ADR-0120）
- 领域事件的命名（待补充）
- 契约的具体内容设计（参见 ADR-0001）

---

## 术语表（Glossary）

| 术语 | 定义 |
|------|------|
| **Contract** | 模块间通信的数据载体，用于传递只读数据 |
| **Query Interface** | 查询服务接口，定义跨模块的只读查询操作 |
| **DTO** | Data Transfer Object，数据传输对象 |
| **Platform.Contracts** | 存放跨模块契约定义的共享项目 |
| **版本化** | 为契约添加版本标识，支持向后兼容演进 |

---

## 核心决策（Decision）

### 1. 契约位置规则

#### 1.1 契约定义位置
**决策**：跨模块契约必须定义在 `Platform.Contracts` 项目中。

```
src/
└── Platform/
    └── Contracts/
        ├── IContract.cs              # 契约标记接口
        ├── IQuery.cs                 # 查询接口标记
        ├── Members/                  # Members 模块契约
        │   ├── IMemberQueries.cs     # 会员查询接口
        │   └── MemberDto.cs          # 会员数据契约
        └── Orders/                   # Orders 模块契约
            ├── IOrderQueries.cs      # 订单查询接口
            └── OrderSummaryDto.cs    # 订单摘要契约
```

**组织规则**：
- 按**源模块**组织目录（`Members/`、`Orders/`）
- 每个模块的契约放在对应目录下
- 避免创建过深的嵌套结构

**✅ 允许**：
- 在 `Platform.Contracts/{模块名}/` 下定义该模块暴露的契约
- 跨模块查询接口（IQuery 实现）
- 跨模块数据传输对象（DTO）

**❌ 禁止**：
- 在模块项目中定义跨模块契约
- 在 `Platform.Contracts` 中定义业务逻辑
- 在 `Platform.Contracts` 中引用模块项目

### 2. 查询接口命名

#### 2.1 查询接口命名规则
**决策**：查询接口命名为 `I{实体}Queries`，使用复数形式。

```csharp
// ✅ 正确的查询接口命名
namespace Zss.BilliardHall.Platform.Contracts.Members;

public interface IMemberQueries : IQuery
{
    Task<MemberDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<MemberDto>> GetActiveMembersAsync();
    Task<MemberDto?> FindByEmailAsync(string email);
}

public interface IOrderQueries : IQuery
{
    Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid orderId);
    Task<IReadOnlyList<OrderSummaryDto>> GetMemberOrdersAsync(Guid memberId);
}
```

**命名规则**：
- 格式：`I{实体名}Queries`
- 使用 `Queries` 复数形式
- 必须继承 `IQuery` 标记接口
- 所有方法返回契约 DTO，不返回领域模型

#### 2.2 查询方法命名
**决策**：查询方法使用 `Get`、`Find`、`List` 等动词，异步方法加 `Async` 后缀。

```csharp
// ✅ 正确的查询方法命名
Task<MemberDto?> GetByIdAsync(Guid id);              // 按 ID 获取单个实体
Task<MemberDto?> FindByEmailAsync(string email);    // 按条件查找单个实体
Task<IReadOnlyList<MemberDto>> GetActiveMembersAsync();  // 获取列表
Task<PagedResult<MemberDto>> SearchMembersAsync(SearchCriteria criteria);  // 分页搜索
```

**动词使用规范**：
- `Get` - 按唯一标识获取，期望存在（不存在返回 null）
- `Find` - 按条件查找，可能不存在
- `List` - 获取列表，可能为空列表
- `Search` - 搜索，通常支持分页和过滤

**返回值规范**：
- 单个实体：`Task<{Dto}?>` （可为 null）
- 列表：`Task<IReadOnlyList<{Dto}>>` 或 `Task<PagedResult<{Dto}>>`
- 只读集合：使用 `IReadOnlyList`、`IReadOnlyCollection`

#### 2.3 反模式示例

```csharp
// ❌ 错误的查询接口命名
public interface IMemberQuery { }              // 应该用复数 Queries
public interface MemberQueries { }             // 缺少 I 前缀
public interface IMemberService { }            // 不要用 Service 后缀
public interface IMemberQueryService { }       // 过于啰嗦

// ❌ 错误的查询方法
Task<Member> GetByIdAsync(Guid id);            // 不应返回领域模型
void GetActiveMembersAsync();                  // 异步方法必须返回 Task
Task<MemberDto> GetByIdAsync(Guid id);         // 可能不存在应返回 nullable
```

### 3. 契约 DTO 命名

#### 3.1 DTO 类命名规则
**决策**：契约 DTO 命名为 `{实体名}Dto`，使用业务语言。

```csharp
// ✅ 正确的 DTO 命名
namespace Zss.BilliardHall.Platform.Contracts.Members;

public record MemberDto : IContract
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record MemberSummaryDto : IContract
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public int TotalOrders { get; init; }
}

public record OrderSummaryDto : IContract
{
    public required Guid Id { get; init; }
    public required Guid MemberId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Status { get; init; }
}
```

**命名规则**：
- 格式：`{实体名}{限定词?}Dto`
- 必须实现 `IContract` 标记接口
- 使用 `record` 类型（不可变）
- 属性使用 `init` 或只读

**限定词使用**：
- 无限定词：完整实体信息（`MemberDto`）
- `Summary`：摘要信息（`MemberSummaryDto`）
- `Details`：详细信息（`MemberDetailsDto`）
- `Brief`：简要信息（`MemberBriefDto`）

#### 3.2 DTO 属性命名
**决策**：DTO 属性使用 PascalCase，语义清晰。

```csharp
// ✅ 正确的属性命名
public record MemberDto : IContract
{
    public required Guid Id { get; init; }         // 唯一标识
    public required string Name { get; init; }     // 业务属性
    public string? PhoneNumber { get; init; }      // 可选属性用 nullable
    public DateTime CreatedAt { get; init; }       // 时间戳
    public bool IsActive { get; init; }            // 布尔值用 Is/Has/Can 前缀
}
```

**属性规范**：
- 必填属性：使用 `required` 修饰符
- 可选属性：使用 nullable 类型 `?`
- 时间属性：使用 `DateTime`（不是 `DateTimeOffset`，除非需要时区）
- 布尔属性：使用 `Is`、`Has`、`Can` 前缀
- 集合属性：使用 `IReadOnlyList<T>` 或 `IReadOnlyCollection<T>`

#### 3.3 反模式示例

```csharp
// ❌ 错误的 DTO 定义
public class MemberDto { }                     // 应该用 record
public record MemberContract { }               // 不要用 Contract 后缀
public record Member { }                       // 应该加 Dto 后缀

// ❌ 错误的属性定义
public record MemberDto
{
    public Guid Id { get; set; }               // 应该用 init，不是 set
    public string name { get; init; }          // 应该用 PascalCase
    public List<string> Tags { get; init; }    // 应该用 IReadOnlyList
    public string? Name { get; init; }         // 必填属性不应该 nullable
}
```

### 4. 文件组织

#### 4.1 文件命名和位置
**决策**：契约文件按类型组织，文件名与类名一致。

```
Platform/Contracts/
├── IContract.cs                          # 标记接口
├── IQuery.cs                             # 查询标记接口
├── Members/                              # Members 模块契约
│   ├── IMemberQueries.cs                 # 查询接口
│   ├── MemberDto.cs                      # 主要 DTO
│   ├── MemberSummaryDto.cs               # 摘要 DTO
│   └── MemberDetailsDto.cs               # 详细 DTO
└── Orders/                               # Orders 模块契约
    ├── IOrderQueries.cs
    ├── OrderSummaryDto.cs
    └── OrderItemDto.cs
```

**组织规则**：
- 一个文件一个类型（接口或 DTO）
- 按源模块分目录
- 不创建过深的子目录

### 5. 契约版本化（可选）

#### 5.1 版本化命名
**决策**：需要版本化的契约可以使用版本号后缀。

```csharp
// ✅ 契约版本化
namespace Zss.BilliardHall.Platform.Contracts.Members.V1;

public record MemberDtoV1 : IContract
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}

namespace Zss.BilliardHall.Platform.Contracts.Members.V2;

public record MemberDtoV2 : IContract
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; }  // 属性重命名
    public string? NickName { get; init; }          // 新增属性
}
```

**版本化策略**：
- 使用命名空间分隔版本：`{模块}.V1`、`{模块}.V2`
- 使用版本号后缀：`MemberDtoV1`、`MemberDtoV2`
- 保持旧版本契约不变，避免破坏性变更

**何时版本化**：
- 破坏性变更（属性重命名、删除）
- 重大语义变更
- 长期支持的公开 API

**何时不需要版本化**：
- 添加新属性（向后兼容）
- 内部模块间的契约（可以直接修改）

---

## 与其他 ADR 关系（Related ADRs）

### 依赖关系
- **ADR-0001（模块化单体架构）** - 定义了契约的使用场景和规则
- **ADR-0003（命名空间规范）** - 定义了契约的命名空间规则

### 补充关系
- **ADR-0120（功能切片命名规范）** - 定义模块内部 DTO 的命名

---

## 快速参考表（Quick Reference）

### 命名模式速查

| 类型 | 命名模式 | 示例 |
|------|---------|------|
| **查询接口** | `I{实体}Queries` | `IMemberQueries` |
| **契约 DTO** | `{实体}{限定词}Dto` | `MemberDto`, `MemberSummaryDto` |
| **查询方法** | `{动词}{实体}{条件}Async` | `GetByIdAsync`, `FindByEmailAsync` |
| **版本化 DTO** | `{实体}DtoV{版本号}` | `MemberDtoV1`, `MemberDtoV2` |

### 查询方法动词速查

| 动词 | 含义 | 返回值 | 示例 |
|------|------|--------|------|
| `Get` | 按唯一标识获取 | `Task<{Dto}?>` | `GetByIdAsync(Guid id)` |
| `Find` | 按条件查找 | `Task<{Dto}?>` | `FindByEmailAsync(string email)` |
| `List` | 获取列表 | `Task<IReadOnlyList<{Dto}>>` | `ListActiveMembersAsync()` |
| `Search` | 搜索（分页） | `Task<PagedResult<{Dto}>>` | `SearchMembersAsync(criteria)` |

### DTO 限定词速查

| 限定词 | 含义 | 使用场景 | 示例 |
|--------|------|---------|------|
| 无 | 完整信息 | 标准查询 | `MemberDto` |
| `Summary` | 摘要信息 | 列表显示 | `MemberSummaryDto` |
| `Details` | 详细信息 | 详情页面 | `MemberDetailsDto` |
| `Brief` | 简要信息 | 引用显示 | `MemberBriefDto` |

---

## 示例与反模式

### ✅ 正确的契约定义

```csharp
namespace Zss.BilliardHall.Platform.Contracts.Members;

// 查询接口
public interface IMemberQueries : IQuery
{
    Task<MemberDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<MemberSummaryDto>> GetActiveMembersAsync();
    Task<MemberDto?> FindByEmailAsync(string email);
}

// 主要 DTO
public record MemberDto : IContract
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

// 摘要 DTO
public record MemberSummaryDto : IContract
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public int TotalOrders { get; init; }
}
```

### ❌ 错误的契约定义

```csharp
// ❌ 错误示例1：命名不规范
public interface IMemberQuery { }              // 应该用复数 Queries
public class MemberDto { }                     // 应该用 record
public record MemberContract { }               // 不要用 Contract 后缀

// ❌ 错误示例2：返回领域模型
public interface IMemberQueries
{
    Task<Member> GetByIdAsync(Guid id);        // 不应返回领域模型
}

// ❌ 错误示例3：可变 DTO
public class MemberDto
{
    public Guid Id { get; set; }               // 应该用 init，不是 set
    public string Name { get; set; }
}

// ❌ 错误示例4：包含业务逻辑
public record MemberDto
{
    public bool IsActive { get; init; }
    
    public bool CanPlaceOrder()                // 契约不应包含业务逻辑
    {
        return IsActive && CreditScore > 60;
    }
}
```

---

## 架构测试保障

本 ADR 的约束由以下测试保障：

1. **契约位置检查** - 验证跨模块契约定义在 `Platform.Contracts` 中
2. **契约命名检查** - 验证接口和 DTO 命名符合规范
3. **契约不可变性检查** - 验证 DTO 使用 `record` 和 `init` 属性
4. **契约纯粹性检查** - 验证契约不包含业务逻辑

> **注意**：本 ADR 是结构层 ADR，需要添加对应的架构测试实现。

---

## 常见问题（FAQ）

### Q: 契约 DTO 和模块内部 DTO 有什么区别？
**A:**
- **契约 DTO**：定义在 `Platform.Contracts`，用于跨模块通信，必须保持稳定
- **内部 DTO**：定义在模块内部，仅在模块内使用，可以自由修改

### Q: 什么时候需要版本化契约？
**A:** 当需要进行破坏性变更且旧版本仍需支持时：
- 属性重命名或删除
- 属性类型变更
- 重大语义变更
- 添加新属性是向后兼容的，通常不需要版本化

### Q: 查询接口可以返回领域模型吗？
**A:** **不可以**。查询接口必须返回契约 DTO：
- 领域模型属于模块内部实现
- 暴露领域模型会破坏模块隔离
- 契约 DTO 提供稳定的跨模块接口

### Q: 契约 DTO 可以包含嵌套对象吗？
**A:** 可以，但要**谨慎使用**：
- 嵌套对象也必须是契约 DTO
- 避免过深的嵌套结构
- 优先使用扁平结构或引用 ID

```csharp
// ✅ 可以嵌套契约 DTO
public record OrderSummaryDto : IContract
{
    public required Guid Id { get; init; }
    public required MemberBriefDto Member { get; init; }  // 嵌套契约
}

// ✅ 更好的方式：使用 ID 引用
public record OrderSummaryDto : IContract
{
    public required Guid Id { get; init; }
    public required Guid MemberId { get; init; }  // ID 引用
}
```

---

## 参考文档

- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0120：功能切片命名规范](ADR-0120-feature-naming-conventions.md)
- [Platform Contracts README](/src/Platform/Contracts/README.md)

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|------|------|---------|
| 1.0 | 2026-01-22 | 初始版本，定义契约命名规范 |
