# ADR-210：领域事件版本化与兼容性

**状态**：Draft  
**级别**：运行时层  
**影响范围**：所有领域事件  
**生效时间**：待审批通过后

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### ADR-210.1：破坏性变更必须创建新版本事件

对领域事件的破坏性变更**必须**创建新版本的事件类型，原事件保持不变。

**破坏性变更定义**：
- ❌ 删除字段
- ❌ 修改字段类型
- ❌ 重命名字段
- ❌ 添加必需字段（无默认值）

**非破坏性变更**：
- ✅ 添加可选字段（有默认值）
- ✅ 添加新的可选属性

**版本命名**：
- ✅ `OrderCreated` → `OrderCreatedV2` → `OrderCreatedV3`
- ❌ `OrderCreated2`、`OrderCreated_New`（不规范）

### ADR-210.2：事件必须包含 SchemaVersion 属性

所有领域事件**必须**包含 `SchemaVersion` 属性标识事件版本。

**属性要求**：
```csharp
public record OrderCreated
{
    public string SchemaVersion { get; init; } = "1.0";
    // 其他属性...
}
```

**版本号规则**：
- ✅ 使用语义化版本号（Major.Minor）
- ✅ 破坏性变更递增 Major
- ✅ 兼容性变更递增 Minor

### ADR-210.3：旧版本事件必须保持至少 2 个大版本

旧版本事件类型**必须**保持至少 2 个大版本周期才能删除。

**保留策略**：
- ✅ V1 创建后，至少等到 V3 发布才能删除 V1
- ✅ 标记为 `[Obsolete]` 在删除前至少一个版本
- ❌ 禁止立即删除旧版本

**废弃流程**：
```csharp
// V2 发布时
[Obsolete("Use OrderCreatedV2 instead", false)]
public record OrderCreated { }

// V3 发布时
[Obsolete("Use OrderCreatedV2 instead", true)]  // 编译错误
public record OrderCreated { }

// V4 发布时可删除 V1
```

### ADR-210.4：事件订阅者必须处理所有活跃版本

事件订阅者（EventHandler）**必须**能够处理所有当前活跃版本的事件。

**处理策略**：
- ✅ 方式 1：为每个版本创建独立 Handler
- ✅ 方式 2：在 Handler 内部转换为统一版本
- ❌ 禁止仅处理最新版本而忽略旧版本

**示例**：
```csharp
// 方式 1：独立 Handler
public class OrderCreatedV1Handler : IEventHandler<OrderCreated> { }
public class OrderCreatedV2Handler : IEventHandler<OrderCreatedV2> { }

// 方式 2：统一处理
public class OrderCreatedHandler : 
    IEventHandler<OrderCreated>,
    IEventHandler<OrderCreatedV2>
{
    public async Task Handle(OrderCreated evt) =>
        await ProcessOrder(ConvertToV2(evt));
    
    public async Task Handle(OrderCreatedV2 evt) =>
        await ProcessOrder(evt);
}
```

### ADR-210.5：事件序列化必须向前兼容

事件序列化**必须**支持向前兼容（新代码能读取旧数据）。

**序列化要求**：
- ✅ 使用 JSON 作为默认序列化格式
- ✅ 忽略未知字段（反序列化时）
- ✅ 为新字段提供默认值
- ❌ 禁止抛出异常在遇到未知字段时

### ADR-210.6：事件版本异常必须降级处理不得中断消费

事件处理遇到版本异常时**必须**记录告警并降级处理，不得中断消费流程。

**容错策略**：
- ✅ 未识别 SchemaVersion → 记录 Warning，使用 Fallback Handler
- ✅ 反序列化失败 → 记录 Error，移入死信队列
- ✅ 语义不兼容 → 记录 Warning，按旧版本逻辑处理
- ❌ 禁止因版本异常导致消费者停止

**Fallback Handler 要求**：
```csharp
// 必须提供未知版本兜底处理
public class UnknownVersionEventHandler : IEventHandler<object>
{
    public async Task Handle(object evt)
    {
        _logger.LogWarning("Unknown event version: {EventType}", evt.GetType());
        // 移入死信队列或人工审查队列
    }
}
```

**生产事故容错原则**：
- 事件系统第一原则：**不死**
- 版本错误是数据问题，不是系统故障
- 必须允许系统在版本异常时继续运行

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 测试映射

| 规则编号 | 执行级 | 测试/手段 |
|---------|--------|----------|
| ADR-210.1 | L2 | Code Review |
| ADR-210.2 | L1 | `Events_Must_Have_SchemaVersion_Property` |
| ADR-210.3 | L2 | 人工审查 + 版本跟踪 |
| ADR-210.4 | L2 | Code Review + 集成测试 |
| ADR-210.5 | L3 | 集成测试验证 |
| ADR-210.6 | L2 | 集成测试 + Runtime 监控 |

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例**仅在以下情况允许**：

1. **首次版本化**：为现有事件添加版本控制
2. **数据迁移期**：大规模重构的过渡阶段
3. **外部系统约束**：第三方系统强制要求特定格式

### 破例要求（不可省略）

每个破例**必须**：

- 记录在 `docs/summaries/arch-violations.md`
- 提供迁移计划（不超过 3 个月）
- 标注影响的事件和订阅者

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **运行时层 ADR**
  * 修改需 Tech Lead/架构师审批
  * 需评估对现有事件的影响

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

- ✗ 事件命名规范（参见 ADR-120）
- ✗ 事件发布机制（参见 ADR-220）
- ✗ 事件存储策略
- ✗ 事件重放机制

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 相关 ADR
- ADR-120：领域事件命名规范
- ADR-220：集成事件总线选型与适配规范

### 技术资源
- [语义化版本规范](https://semver.org/lang/zh-CN/)
- [JSON Schema 文档](https://json-schema.org/)

### 实践指导
- 事件版本化详细示例参见 `docs/copilot/adr-0210.prompts.md`

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|-----|------|---------|--------|
| 1.0 Draft | 2026-01-24 | 初始版本 | GitHub Copilot |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
