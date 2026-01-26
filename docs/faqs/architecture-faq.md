# 模块化架构常见问题

> 📋 **根据 ADR-950 创建的 FAQ 文档**  
> **对应 ADR**：ADR-0001, ADR-0005  
> **最后更新**：2026-01-26

---

## 概述

本文档解答关于模块化单体架构和垂直切片架构的常见问题。

---

## 模块隔离

### Q: 为什么模块之间不能直接引用？

**A**: 模块直接引用会导致：
1. **耦合度过高**：修改一个模块会影响其他模块
2. **测试困难**：无法独立测试单个模块
3. **边界模糊**：业务职责不清晰
4. **扩展受限**：未来无法拆分为微服务

模块隔离是实现松耦合、高内聚的关键。

**参考 ADR**：[ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 第 2.1 节

---

### Q: 模块间如何通信？

**A**: 有三种合规方式：

1. **领域事件（异步）**：
   - 用于：通知其他模块某事已发生
   - 示例：订单创建后发布 `OrderCreatedEvent`
   
2. **契约查询（同步，只读）**：
   - 用于：获取其他模块的数据用于显示
   - 示例：查询会员信息显示在订单详情中
   
3. **原始类型（ID）**：
   - 用于：保存关联关系
   - 示例：订单中保存 `MemberId`（Guid）

**禁止**：
- ❌ 直接调用其他模块的 Command Handler
- ❌ 共享领域对象

**参考 ADR**：[ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 第 2.2 节

---

### Q: 什么时候应该创建新模块？

**A**: 当满足以下条件时考虑创建新模块：

1. **独立业务能力**：具有明确的业务边界（如会员、订单、计费）
2. **独立团队负责**：可以由不同团队独立开发维护
3. **不同变更频率**：不同的发布周期或变更速度
4. **不同的数据所有权**：拥有自己的数据和业务规则

**不应该**创建过多细粒度的模块，这会导致：
- 模块间通信复杂度增加
- 事务边界难以管理
- 开发效率降低

**参考 ADR**：[ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 第 2.1 节

---

## 垂直切片

### Q: 什么是垂直切片？与分层架构有什么区别？

**A**: 

**垂直切片**：
- 按业务用例组织（如 `CreateOrder`, `CancelOrder`）
- 每个用例包含从 API 到数据库的完整流程
- 代码组织：`Orders/UseCases/CreateOrder/`

**水平分层**（禁止）：
- 按技术职责组织（Controller, Service, Repository）
- 代码分散在不同层
- 修改一个功能需要改动多个层

**优势**：
- ✅ 业务逻辑集中，易于理解
- ✅ 修改影响范围小
- ✅ 易于测试和维护

**参考 ADR**：[ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 第 3.2 节

---

### Q: 垂直切片中如何避免代码重复？

**A**: 

1. **提取到领域模型**：
   - 多个用例共享的业务逻辑 → 领域对象方法
   - 示例：订单折扣计算 → `Order.ApplyDiscount()`

2. **提取到 BuildingBlocks**：
   - 跨模块的技术性代码 → `Platform/BuildingBlocks`
   - 示例：分页、验证、日志等

3. **不要过早提取**：
   - 等到至少 3 个用例需要相同逻辑时再提取
   - 避免过度抽象

**禁止**：
- ❌ 创建横向 Service 层
- ❌ 创建通用 Manager/Helper 类

**参考 ADR**：[ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md) - 第 3.3 节

---

## Handler 模式

### Q: Command Handler 为什么不能返回业务数据？

**A**: 

**原因**：
1. **职责分离**：Command 改变状态，Query 查询数据
2. **缓存友好**：Query 结果可以安全缓存
3. **性能优化**：Query 可以直接查数据库，跳过领域模型
4. **API 设计**：POST/PUT 返回 ID，GET 返回数据

**正确做法**：
```csharp
// Command Handler - 仅返回 ID
public async Task<Guid> Handle(CreateOrder command)
{
    var order = new Order(...);
    await _repository.SaveAsync(order);
    return order.Id;  // ✅ 仅返回 ID
}

// 需要数据时，发起独立的 Query
var orderId = await Send(new CreateOrder(...));
var orderDetails = await Send(new GetOrderDetails(orderId));
```

**参考 ADR**：[ADR-0005：应用内交互模型与执行边界](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md) - 第 2.1 节

---

### Q: 什么时候使用 Command，什么时候使用 Query？

**A**: 

**使用 Command 当**：
- ✅ 修改系统状态（创建、更新、删除）
- ✅ 触发业务逻辑和验证
- ✅ 需要发布领域事件
- ✅ 需要事务保证

**使用 Query 当**：
- ✅ 仅读取数据用于显示
- ✅ 不修改任何状态
- ✅ 可以跨模块边界查询（通过契约）
- ✅ 需要性能优化（如投影、缓存）

**参考 ADR**：[ADR-0005：应用内交互模型与执行边界](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md) - 第 2 节

---

## 故障排查

### Q: 架构测试失败：模块引用了其他模块，如何修复？

**A**: 

1. **确认引用类型**：
   ```
   ❌ 直接引用领域类型：Zss.BilliardHall.Modules.Members.Domain.Member
   ```

2. **选择合规方案**：
   - 需要通知 → 发布领域事件
   - 需要数据 → 使用契约（DTO）
   - 仅需 ID → 保存 Guid/string

3. **示例修复**：
   ```csharp
   // ❌ 错误
   using Zss.BilliardHall.Modules.Members.Domain;
   private readonly Member _member;
   
   // ✅ 正确
   using Zss.BilliardHall.Modules.Members.Contracts;
   private readonly MemberDto _memberInfo;  // 或
   private readonly Guid _memberId;
   ```

**参考 ADR**：[ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)

---

### Q: 如何验证我的代码符合架构约束？

**A**: 

运行以下测试：

```bash
# 1. 运行所有架构测试
dotnet test src/tests/ArchitectureTests/

# 2. 针对特定 ADR
dotnet test --filter "FullyQualifiedName~ADR_0001"

# 3. 本地验证脚本
./scripts/verify-all.sh
```

**常见测试**：
- `ADR_0001_*` - 模块隔离
- `ADR_0005_*` - Handler 模式和 CQRS

**参考**：
- [ADR-0000：架构测试与 CI 治理宪法](../adr/governance/ADR-0000-architecture-tests.md)
- [测试指南](../TESTING-GUIDE.md)

---

## 相关文档

- [ADR-0001：模块化单体与垂直切片架构](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0005：应用内交互模型与执行边界](../adr/constitutional/ADR-0005-Application-Interaction-Model-Final.md)
- [架构指南](../architecture-guide.md)
- [快速开始](../QUICK-START.md)

---

**维护**：Tech Lead  
**最后审核**：2026-01-26  
**状态**：✅ Active
