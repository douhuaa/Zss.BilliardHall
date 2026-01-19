# ADR-004: BuildingBlocks 严格准入标准

## 状态

**接受**

## 上下文

在垂直切片架构中，我们需要明确哪些代码应该放入 `BuildingBlocks` 共享基础设施，哪些应该保留在模块内。

### 面临的问题

1. **BuildingBlocks 污染风险**：
   - 99% 的团队会在 BuildingBlocks 上失败
   - 容易变成"万能工具箱"，堆积大量不该共享的代码
   - 一旦放入 BuildingBlocks，就成为"祖宗牌位"，谁都不敢改

2. **过早抽象的陷阱**：
   - 看到 2 个模块有相似代码就立即抽取
   - 抽取的往往是"烂设计"，锁死未来演化空间
   - 业务一变，BuildingBlocks 反而成为枷锁

3. **ErrorCodes 的特殊风险**：
   - ErrorCodes 太好用，容易滥用
   - 一旦开始承载业务语义，就等于把业务规则偷偷搬进 BuildingBlocks

### 现实案例

**失败案例 1**：只有 2 个模块用就抽取
```csharp
// ❌ BuildingBlocks/Helpers/PriceCalculator.cs
public static class PriceCalculator
{
    public static decimal Calculate(TimeSpan duration) { }
}
```
结果：第 3 个模块需求不同，无法复用，反而增加认知负担。

**失败案例 2**：ErrorCodes 承载业务规则
```csharp
// ❌ BuildingBlocks/ErrorCodes.cs
ErrorCodes.Tables.CannotReserveAtNight  // 这是业务规则！
```
结果：业务规则分散在 BuildingBlocks 和模块中，难以追踪。

## 决策

**BuildingBlocks 必须同时满足以下 5 条严格标准，缺一不可。**

### 5 条准入铁律

1. **被 3 个以上模块真实使用**（不是"将来可能用"）
2. **跨模块不可避免**（不能通过消息通信解决）
3. **没有业务语义**（纯技术设施）
4. **不会频繁变更**（稳定的契约）
5. **抽象后修改成本真的降低**（隐含条件，最关键）

### 第 5 条隐含规则详解

> **残酷的真相**: "3 个模块使用" ≠ "值得抽象"

**判断标准**: 抽象之后，修改成本是否真的下降？

**示例分析**:

```csharp
// 场景：3 个模块都复制了同一段价格计算逻辑

// ❌ 错误：抽象后修改成本反而上升
// BuildingBlocks/PricingService.cs
public interface IPricingService
{
    decimal Calculate(TimeSpan duration, PricingContext context);
}
// 问题：
// - 业务变更时，需要同时考虑 3 个模块的兼容性
// - 任何修改都影响 3 个模块，回归测试成本高
// - 3 个模块的需求开始分化，接口变得臃肿

// ✅ 正确：保持独立，允许独立演化
// Modules/Billing/PriceCalculator.cs
internal static class PriceCalculator { }
// Modules/Sessions/PriceEstimator.cs
internal static class PriceEstimator { }
// Modules/Members/MembershipPricing.cs
internal static class MembershipPricing { }
// 优势：
// - 每个模块独立演化，互不影响
// - 业务变更只影响对应模块
// - 代码虽重复，但耦合度低
```

### ErrorCodes 特殊规则

**铁律**: ErrorCodes 只允许表达"失败类型"，不允许表达"业务决策原因"

**✅ 允许进入 BuildingBlocks**:
```csharp
ErrorCodes.Tables.NotFound           // 技术失败：资源不存在
ErrorCodes.Tables.InvalidStatus      // 技术失败：状态错误
ErrorCodes.Tables.Conflict           // 技术失败：并发冲突
ErrorCodes.Tables.Forbidden          // 技术失败：权限不足
```

**❌ 禁止进入 BuildingBlocks**（必须在模块内定义）:
```csharp
// ❌ 这些是业务规则，不是技术失败！
ErrorCodes.Tables.CannotReserveAtNight      // 业务决策：夜间不可预订
ErrorCodes.Members.MemberLevelTooLow        // 业务决策：会员等级不足
ErrorCodes.Billing.PromotionExpired         // 业务决策：促销已过期
```

**正确做法**: 业务决策相关错误码在模块内定义
```csharp
// Modules/Tables/ErrorCodes.cs (模块内部)
internal static class TableErrorCodes
{
    public const string CannotReserveAtNight = "Tables:CannotReserveAtNight";
}
```

### 审核流程

**代码进入 BuildingBlocks 前，必须通过以下检查清单**:

- [ ] 提供 3 个真实使用场景（不是假设）
- [ ] 证明无法通过消息通信解决
- [ ] 确认不包含业务语义
- [ ] 评估变更频率（每月 < 1 次）
- [ ] **评估抽象后是否真的降低修改成本**（最关键）

## 后果

### 正面影响

1. **防止 BuildingBlocks 污染**：
   - 严格准入标准避免"万能工具箱"
   - 保持 BuildingBlocks 小而精

2. **模块独立演化**：
   - 允许适度重复，换取低耦合
   - 业务变更限制在模块内

3. **降低长期维护成本**：
   - 抽象有成本，只在真正必要时抽象
   - 避免错误的抽象锁死未来

### 负面影响

1. **代码重复增加**：
   - 相似逻辑可能在多个模块重复
   - 需要克制"复用洁癖"

2. **团队抵触情绪**：
   - 经验丰富的开发者习惯"DRY 原则"
   - 需要培训理解"适度重复好过错误抽象"

### 风险缓解

1. **建立共识**：
   - 培训团队理解 BuildingBlocks 污染风险
   - Code Review 严格执行 5 条准入标准

2. **定期审查**：
   - 季度审查 BuildingBlocks 内容
   - 识别使用率低或业务语义重的代码
   - 必要时将代码下沉到模块

3. **ErrorCodes 专项审查**：
   - PR 审查时重点检查 ErrorCodes 新增项
   - 问自己：这是"技术失败"还是"业务决策"？

## 替代方案

### 方案 A: 宽松准入（2 个模块即可）

**描述**: 降低准入标准到 2 个模块

**优点**: 更早复用代码，减少重复

**缺点**:
- BuildingBlocks 快速膨胀
- 2 个模块样本量不足以验证抽象合理性
- 容易抽取错误的设计

**为什么未采纳**: 经验表明，2 个模块不足以判断抽象价值，宁可保守。

### 方案 B: 无限制共享

**描述**: 不设准入标准，团队自行判断

**优点**: 灵活，减少限制

**缺点**:
- BuildingBlocks 必然变成垃圾场
- 团队各行其是，无法建立统一标准
- 维护成本快速上升

**为什么未采纳**: 自由度高但长期代价巨大，需要铁律约束。

### 方案 C: 完全禁止共享

**描述**: 不允许任何代码进入 BuildingBlocks

**优点**: 模块完全独立，零耦合

**缺点**:
- 过度保守，无法复用真正稳定的技术设施
- 重复代码过多，影响一致性（如 Result<T>）

**为什么未采纳**: 矫枉过正，真正通用的技术设施应该共享。

## 决策流程图

```
需要共享代码？
  ├─ 是否被 3+ 模块使用？
  │   ├─ 否 → 复制代码到各模块
  │   └─ 是 → 是否纯技术设施？
  │       ├─ 否 → 通过事件/命令通信
  │       └─ 是 → 是否稳定（月变更 < 1）？
  │           ├─ 否 → 保持在模块内
  │           └─ 是 → 抽象后修改成本降低？
  │               ├─ 否 → 保持在模块内
  │               └─ 是 → ✅ 可进入 BuildingBlocks
  └─ 否 → 保持在模块内
```

## 相关决策

- [ADR-001: 采用垂直切片架构](./ADR-001-采用垂直切片架构.md)
- [ADR-012: ErrorCodes 不承载业务语义的铁律](./ADR-012-ErrorCodes约束.md)
- [ADR-014: Solution 结构设计](./ADR-014-Solution结构设计.md)

## 参考资料

- [The Wrong Abstraction - Sandi Metz](https://sandimetz.com/blog/2016/1/20/the-wrong-abstraction)
- [Duplication is far cheaper than the wrong abstraction](https://www.codingblocks.net/podcast/why-duplication-is-cheaper-than-the-wrong-abstraction/)
- 《Wolverine 模块化架构蓝图》- 2.3 BuildingBlocks 防污染铁律

---

**日期**: 2026-01-19  
**决策者**: 架构团队  
**最后更新**: 2026-01-19
