---
adr: ADR-121
title: "契约（Contract）与 DTO 命名组织规范"
status: Final
level: Structure
version: "2.2"
deciders: "Architecture Board"
date: 2026-02-06
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "GitHub Copilot"
supersedes: null
superseded_by: null
---


# ADR-121：契约（Contract）与 DTO 命名组织规范

**适用范围**：所有模块（Modules）、跨模块数据传递、API 层、事件与命令 Query 消息  
## Focus（聚焦内容）

- 统一跨模块契约/DTO 命名规则，确保类型隔离和可演进性
- 规范契约目录组织和命名空间映射
- 定义版本管理策略，支持向后兼容和渐进式废弃
- 明确契约约束：只读、无业务逻辑、不包含领域模型
- 为架构测试、文档生成和工具链自动发现提供标准基础
- 严格遵守模块隔离原则，避免契约嵌入跨模块业务语义

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------------------|---------------------------------------|---------------------------|
| 契约（Contract）     | 跨模块数据传递的只读、版本化的数据 DTO，只用于信息传递       | Contract               |
| DTO              | 数据传输对象，用于在不同层次或模块间传递数据，不包含业务逻辑      | Data Transfer Object   |
| 模块内 DTO          | 仅在模块内部使用的 DTO，不对外暴露                 | Internal DTO           |
| 跨模块契约            | 在模块间传递的契约，必须严格遵守命名和组织规范             | Cross-Module Contract  |
| 契约版本             | 契约结构的版本标识（如 V2、V3），用于支持向后兼容和演进     | Contract Version       |
| 业务含义后缀           | 反映数据用途的后缀，如 `InfoDto`、`DetailContract` | Business Meaning Suffix |

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-121 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-121_<Rule>_<Clause>
> ```

---

### ADR-121.1：契约类型命名规范（Rule）

#### ADR-121.1.1 契约类型命名模式

所有跨模块契约必须遵循以下命名模式：

```
{AggregateRoot}[{BusinessMeaning}]{Dto|Contract}
```

- **{AggregateRoot}**：聚合根名称（单数、PascalCase）
- **{BusinessMeaning}**：可选业务含义（Info、Detail、Summary、List）
- **{Dto|Contract}**：固定后缀（必须二选一）

**✅ 正确示例**：

```csharp
// 基础契约
public record MemberDto(Guid MemberId, string UserName);
public record OrderContract(Guid OrderId, decimal TotalAmount);

// 带业务含义
public record MemberInfoDto(Guid MemberId, string UserName, string Email);
public record OrderDetailContract(Guid OrderId, IReadOnlyList<OrderItemDto> Items);

// 嵌套 DTO
public record OrderItemDto(Guid ProductId, string ProductName, int Quantity);
```

**❌ 错误示例**：

```csharp
public record MemberInfo(Guid MemberId);        // ❌ 缺少后缀
public record MemberData(Guid MemberId);        // ❌ 模糊名称
public record MemberEntity(Guid MemberId);      // ❌ Entity 保留给领域模型
```

#### ADR-121.1.2 属性命名规范

- 主键属性：`{AggregateRoot}Id`（如 `MemberId`、`OrderId`）
- 避免通用名称（`Id`、`Data`、`Value`），使用明确业务语义
- 集合属性使用复数（`Items`、`Orders`）

---

---

### ADR-121.2：目录与命名空间组织（Rule）

#### ADR-121.2.1 契约目录结构规范

契约组织支持三种方式：

**方式 1：Platform.Contracts（当前项目推荐）**

```
src/Platform/Contracts/
  Members/MemberInfoDto.cs
  Orders/OrderDetailContract.cs
```

**方式 2：模块内 Contracts**

```
src/Modules/Members/Contracts/
  MemberInfoDto.cs
```

**方式 3：独立 Contracts 程序集**

```
src/Contracts/
  Members/MemberInfoDto.cs
```

#### ADR-121.2.2 命名空间映射规范

契约命名空间必须与物理目录一致：

```csharp
// Platform.Contracts
namespace Zss.BilliardHall.Platform.Contracts.Members;
public record MemberInfoDto(...);

// 模块内 Contracts
namespace Zss.BilliardHall.Modules.Members.Contracts;
public record MemberInfoDto(...);
```

---

---

### ADR-121.3：契约内容约束（Rule）

#### ADR-121.3.1 不可变性约束

所有契约必须是只读的：

```csharp
// ✅ 使用 record（推荐）
public record MemberInfoDto(Guid MemberId, string UserName);

// ✅ 或使用 init-only
public class MemberInfoDto
{
    public required Guid MemberId { get; init; }
    public required string UserName { get; init; }
}

// ❌ 禁止可变属性
public class MemberInfoDto
{
    public Guid MemberId { get; set; }  // ❌
}
```

---

#### ADR-121.3.2 无业务逻辑约束

契约不得包含业务方法：

```csharp
// ✅ 允许：计算属性
public record OrderDetailContract(
    Guid OrderId,
    IReadOnlyList<OrderItemDto> Items
)
{
    public decimal TotalAmount => Items.Sum(i => i.Price);  // ✅
}

// ❌ 禁止：业务判断方法
public record MemberInfoDto(Guid MemberId, decimal Balance)
{
    public bool CanUpgrade() => Balance > 1000;  // ❌
}
```

---

#### ADR-121.3.3 不包含领域模型约束

契约只能包含原始类型和其他 DTO：

```csharp
// ✅ 正确
public record OrderDetailContract(
    Guid OrderId,                           // 原始类型
    IReadOnlyList<OrderItemDto> Items       // 嵌套 DTO
);

// ❌ 错误
public record OrderDetailContract(
    Guid OrderId,
    Order Order,              // ❌ 领域实体
    Member Member             // ❌ 领域实体
);
```

### 版本管理

#### ADR-121.4.1 版本命名规范

破坏性变更必须创建新版本（V2、V3）：

```csharp
// V1
public record MemberInfoDto(Guid MemberId, string UserName);

// V2（添加必需属性）
[Obsolete("Use MemberInfoDtoV2 instead. Removed after 2025-01-01.", false)]
public record MemberInfoDto(Guid MemberId, string UserName);

public record MemberInfoDtoV2(Guid MemberId, string UserName, string Email);
```

---

#### ADR-121.4.2 版本废弃策略

使用 `[Obsolete]` 标记旧版本，采用渐进式流程：

1. **阶段 1**：警告级别（`error: false`）
2. **阶段 2**：6 个月后升级为错误级别（`error: true`）
3. **阶段 3**：12 个月后移除旧版本

---

#### ADR-121.4.3 嵌套DTO版本演进

嵌套 DTO 独立版本管理：

```csharp
// 父契约 V2，子 DTO 也需升级
public record OrderDetailContractV2(
    Guid OrderId,
    IReadOnlyList<OrderItemDtoV2> Items  // 使用新版本
);

public record OrderItemDtoV2(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal DiscountRate  // 新增字段
);
```

---

---

### ADR-121.4：标记接口规范（Rule）

#### ADR-121.4.1 IContract接口使用规范（可选）

为支持工具和文档生成，契约可实现 `IContract`：

```csharp
namespace Zss.BilliardHall.Platform.Contracts;

public interface IContract
{
    string Version => "1.0";  // 可选版本属性
}

// 使用
public record MemberInfoDto(Guid MemberId, string UserName) : IContract
{
    public string Version => "1.0";
}
```

---

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-121 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-121.1.1** | L1 | ArchitectureTests 验证契约命名模式 | §ADR-121.1.1 |
| **ADR-121.1.2** | L1 | ArchitectureTests 验证属性命名规范 | §ADR-121.1.2 |
| **ADR-121.2.1** | L1 | ArchitectureTests 验证契约目录结构 | §ADR-121.2.1 |
| **ADR-121.2.2** | L1 | ArchitectureTests 验证命名空间映射 | §ADR-121.2.2 |
| **ADR-121.3.1** | L1 | ArchitectureTests 验证不可变性 | §ADR-121.3.1 |
| **ADR-121.3.2** | L1 | ArchitectureTests 验证无业务逻辑 | §ADR-121.3.2 |
| **ADR-121.3.3** | L1 | ArchitectureTests 验证不包含领域模型 | §ADR-121.3.3 |
| **ADR-121.4.1** | L1 | ArchitectureTests 验证版本命名规范 | §ADR-121.4.1 |
| **ADR-121.4.2** | L2 | Code Review 检查版本废弃流程 | §ADR-121.4.2 |
| **ADR-121.4.3** | L2 | Code Review 检查嵌套DTO版本一致性 | §ADR-121.4.3 |
| **ADR-121.4.1** | L3 | 文档审查 | §ADR-121.4.1 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署
- **L2（警告级）**：违规记录告警，需人工 Code Review 裁决
- **L3（人工级）**：需要架构师人工裁决


---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- 待补充

---

## Prohibited（禁止行为）


以下行为明确禁止：

- 待补充


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-005：应用内交互模型与执行边界](../constitutional/ADR-005-Application-Interaction-Model-Final.md) - 契约 DTO 基于 CQRS 模式
- [ADR-006：术语与编号宪法](../constitutional/ADR-006-terminology-numbering-constitution.md) - 命名约定遵循术语规范
- [ADR-003：命名空间与项目结构规范](../constitutional/ADR-003-namespace-rules.md) - 命名空间规范
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)

**被依赖（Depended By）**：
- [ADR-124：Endpoint 命名及参数约束规范](./ADR-124-endpoint-naming-constraints.md) - Endpoint 使用契约遵循命名规范

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-120：领域事件命名约定](./ADR-120-domain-event-naming-convention.md) - 同为命名规范

---

---

## References（非裁决性参考）


- 待补充


---

---

## History（版本历史）


| 版本  | 日期         | 变更说明   | 修订人 |
|-----|------------|--------|-------|
| 2.2 | 2026-02-06 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系。将原有规则智能分组为 5 个 Rule、11 个 Clause，并创建完整的 Enforcement 映射表 | Architecture Board |
| 1.0 | 2026-01-29 | 初始版本 | Architecture Board |
