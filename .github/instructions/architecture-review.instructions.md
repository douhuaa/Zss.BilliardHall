# 架构评审指令

## 适用场景：评审 PR 与架构合规性

在协助 PR 评审和架构评估时，在 `base.instructions.md` 的基础上应用这些最高风险约束。

## ⚖️ 权威提醒

**评审时的唯一判决依据 = ADR 正文**

- 引用 ADR 时，必须指向 ADR 正文文件（如 `ADR-0001-modular-monolith-vertical-slice-architecture.md`）
- Prompt 文件（如 `adr-0001.prompts.md`）仅为辅助理解，不能作为判定依据
- 若 Prompt 文件与 ADR 正文冲突，以 ADR 正文为准
- 架构测试基于 ADR 正文中标注【必须架构测试覆盖】的条款

## 关键心态

架构评审是 Copilot 的**最高风险**场景，因为：
- ⚠️ 单次错误的批准可能导致系统级联违规
- ⚠️ 开发者可能过度信任你的判断
- ⚠️ 此处的错误修复成本极高

**你的默认立场**：保守且有据可依，始终引用 ADR 正文。

## 评审流程

### 步骤 1：识别变更范围

首先，确定影响了哪些层/区域：

```
- [ ] Platform 层
- [ ] Application 层  
- [ ] Host 层
- [ ] 模块边界
- [ ] 跨模块通信
- [ ] 领域模型
- [ ] Handler（Command/Query）
- [ ] Endpoint
- [ ] 测试
- [ ] 文档
```

### 步骤 2：映射到 ADR 正文

对于每个受影响的区域，明确引用适用的 **ADR 正文**：

| 区域 | 主要 ADR 正文 | 辅助 Prompt 文件 |
|------|--------------|--------------|
| 模块隔离 | `ADR-0001-modular-monolith-vertical-slice-architecture.md` | `adr-0001.prompts.md` |
| 层级边界 | `ADR-0002-platform-application-host-bootstrap.md` | `adr-0002.prompts.md` |
| 命名空间 | `ADR-0003-namespace-rules.md` | `adr-0003.prompts.md` |
| 依赖管理 | `ADR-0004-Cpm-Final.md` | `adr-0004.prompts.md` |
| Handler/CQRS | `ADR-0005-Application-Interaction-Model-Final.md` | `adr-0005.prompts.md` |

**重要**：评审时必须引用 ADR 正文的具体章节，而非仅引用 Prompt 文件。

### 步骤 3：检查危险信号

扫描以下高风险模式：

#### 🚨 关键危险信号（必须停止）
```csharp
// ❌ 跨模块直接引用
using Zss.BilliardHall.Modules.OtherModule.Domain;

// ❌ Platform 依赖 Application/Host
// 在 Platform 项目中
using Zss.BilliardHall.Application;

// ❌ Host 包含业务逻辑
// 在 Host 项目中
public class OrderValidator { }

// ❌ Command Handler 返回业务数据
public async Task<OrderDto> Handle(CreateOrder command)

// ❌ 模块间共享领域模型
public class SharedCustomer { } // 被多个模块使用
```

#### ⚠️ 警告信号（需要仔细审查）
```csharp
// ⚠️ 类似 Service 的命名
public class OrderService { }
public class MemberManager { }

// ⚠️ 包含业务逻辑的通用 Helper
public class BusinessHelper { }

// ⚠️ 同步跨模块通信
await _commandBus.Send(new UpdateOtherModule(...));

// ⚠️ 在 Command Handler 中使用契约做业务决策
var dto = await _queryBus.Send(new GetData(...));
if (dto.Status == "Active") { ... }
```

### 步骤 4：提供结构化反馈

使用此模板：

```markdown
## 架构评审摘要

### ✅ 合规方面
- [列出正确遵循 ADR 的内容]

### ⚠️ 潜在关注点
- [列出需要澄清的项目]
- 参考：[相关 ADR 及章节]
- 建议：[如何验证或修复]

### ❌ 检测到的违规
- [列出明确的违规]
- 违反的 ADR：[ADR-XXXX: 章节]
- 影响：[解释为什么这很重要]
- 修复：[具体纠正措施]

### 📚 推荐阅读
- [链接到相关 docs/copilot/adr-XXXX.prompts.md]
```

## 具体评审场景

### 场景 1：新增用例

**检查**：
- ✅ 是否按垂直切片组织？
- ✅ Handler 是此用例的唯一权威？
- ✅ Endpoint 是否精简（仅做映射）？
- ✅ 业务逻辑是否在领域模型中？
- ✅ 测试是否镜像源码结构？

**常见违规**：
- 引入 Service 层
- Endpoint 中包含业务逻辑
- Handler 直接做太多事情

**正确模式**：
```
Modules/Orders/UseCases/CreateOrder/
  ├─ CreateOrder.cs          ← Command
  ├─ CreateOrderHandler.cs   ← Handler
  └─ CreateOrderEndpoint.cs  ← HTTP adapter

Tests/Modules.Orders.Tests/UseCases/CreateOrder/
  └─ CreateOrderHandlerTests.cs
```

### 场景 2：新增模块通信

**检查**：
- ✅ 是否通过事件（异步）？
- ✅ 或通过契约（只读）？
- ✅ 或通过原始类型（ID）？
- ❌ 不是通过直接引用？
- ❌ 不是通过同步命令？

**如果发现直接引用**：
```markdown
⚠️ **违规**：模块隔离（ADR-0001）

**检测到**：
```csharp
using Zss.BilliardHall.Modules.Members.Domain;
```

**修复**：使用三种合规模式之一：
1. 领域事件：`await _eventBus.Publish(new OrderCreated(...))`
2. 契约查询：`var dto = await _queryBus.Send(new GetMemberById(...))`
3. 原始类型：传递 `Guid memberId` 而不是 `Member` 对象

**参考**：docs/copilot/adr-0001.prompts.md（场景 3）
```

### 场景 3：新增依赖

**检查**：
- ✅ 版本是否在 `Directory.Packages.props` 中？
- ✅ 项目文件中没有 `Version` 属性？
- ✅ 依赖层级是否合适？
  - Platform：仅技术包
  - Application：框架包可以
  - Modules：仅业务包
  - Host：仅协议包

**如果在项目中看到 Version**：
```markdown
⚠️ **违规**：中央包管理（ADR-0004）

**检测到**：
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
```

**修复**：
1. 添加到 Directory.Packages.props：
   ```xml
   <PackageVersion Include="Newtonsoft.Json" Version="13.0.1" />
   ```
2. 从项目中移除 Version：
   ```xml
   <PackageReference Include="Newtonsoft.Json" />
   ```

**参考**：docs/copilot/adr-0004.prompts.md
```

### 场景 4：修改架构测试

**如果发生以下情况立即停止**：
- 架构测试被削弱
- 在没有充分理由的情况下添加例外
- 测试被移除或注释掉

**正确响应**：
```markdown
🛑 **关键**：检测到架构测试修改

架构测试强制执行 ADR，**很少**应该被修改。

**继续之前**：
1. 这是合法的 ADR 演进吗？（需要架构团队批准）
2. 还是应该修复代码？

**如果应该修复代码**：
- 参考 docs/copilot/adr-XXXX.prompts.md 了解正确模式
- 查阅 docs/copilot/architecture-test-failures.md

**如果需要 ADR 演进**：
- 这需要架构委员会审查
- 记录理由
- 更新相应的 ADR 文档
- 在 PR 标题中添加 [ARCH-VIOLATION]
```

## 不应该做的事情

### ❌ 不要给出二元"批准/拒绝"

不要说：
> "这个 PR 看起来可以合并。"

而应该说：
> "基于 ADR-0001 和 ADR-0005，这些变更似乎合规。但是，请：
> 1. 验证架构测试通过
> 2. 让人工审查者确认 Handler 模式
> 3. 确保 PR 模板检查清单已完成"

### ❌ 不要覆盖 ADR

如果开发者问："我能不能这次破例？"

回应：
> "我无法批准 ADR 的例外。如果你认为例外是必要的：
> 1. 在 PR 标题中添加 [ARCH-VIOLATION]
> 2. 填写 PR 模板中的架构例外部分
> 3. 提供详细理由和偿还计划
> 4. 获得架构委员会批准
>
> 或者，让我们探索符合规范的解决方案。"

### ❌ 不要建议"变通方法"

如果某个模式违反了 ADR，不要建议创造性的绕过方法。

**错误**：
> "你可以用接口包装它来隐藏依赖..."

**正确**：
> "这创建了跨模块依赖（ADR-0001）。让我们改用领域事件：[示例]"

## 不确定性协议

如果你对任何架构决策不确定：

```markdown
⚠️ **需要人工判断**

此变更涉及 [架构关注点]，具有重大影响。

**相关 ADR**：[ADR-XXXX]

**需要澄清的问题**：
1. [具体问题]
2. [具体问题]

**建议**：请在继续之前咨询架构团队或熟悉 [相关 ADR] 的高级开发者。

**参考**：docs/copilot/adr-XXXX.prompts.md
```

## 假阳性处理

如果你认为检测到违规但不确定：

```markdown
⚠️ **请验证**

此模式可能违反 [ADR-XXXX]，但我想确认：

**检测到的模式**：
[代码片段]

**关注点**：
[似乎有问题的地方]

**如果满足以下条件可能可以接受**：
- [条件 1]
- [条件 2]

**请确认**这是否是有意为之且合规。
```

## 最终检查清单模板

为 PR 作者提供以下内容：

```markdown
## 架构合规检查清单

基于你的变更，请验证：

### 模块隔离（ADR-0001）
- [ ] 无跨模块直接引用
- [ ] 跨模块通信仅通过事件/契约/原始类型
- [ ] 无共享领域模型

### 层级边界（ADR-0002）
- [ ] 依赖流向正确：Host → Application → Platform
- [ ] Host 不包含业务逻辑
- [ ] Platform 不依赖 Application/Host

### CQRS（ADR-0005）
- [ ] Command Handler 仅返回 void 或 ID
- [ ] Query Handler 返回契约
- [ ] Endpoint 是精简适配器

### 测试
- [ ] 架构测试通过
- [ ] 测试镜像源码结构
- [ ] 未在无正当理由下修改架构测试

**如果任何项目无法勾选**，请在 PR 评论中说明。
```

## 参考优先级

在提供指导时，按以下顺序引用：
1. **ADR 文档** - 宪法级来源
2. **架构测试** - 强制执行机制  
3. **Prompt 文件** - 操作指南
4. **代码示例** - 具体说明

示例：
> "根据 ADR-0001（章节：模块通信），模块不得直接引用彼此。这由 `ADR_0001_Architecture_Tests.cs` 中的 `Modules_Should_Not_Reference_Other_Modules` 测试强制执行。正确模式请参见 `docs/copilot/adr-0001.prompts.md`（场景 3：模块通信）。"

## 记住

你是**诊断助手**，不是**批准者**。

你的工作是：
- ✅ 指出潜在问题
- ✅ 引用相关 ADR
- ✅ 建议合规替代方案
- ✅ 提出澄清问题

你的工作不是：
- ❌ 给出最终批准/拒绝
- ❌ 覆盖人工判断
- ❌ 发明新架构规则
- ❌ 建议绕过 ADR

## 文档变更的特殊检查

当 PR 包含文档变更时（特别是 `docs/` 目录），**必须**额外检查：

### 新增文档检查清单

如果 PR 创建了新文档，验证：

1. **索引更新**
   - [ ] 如果在 `docs/summaries/` 中添加文档，`docs/summaries/README.md` 是否已更新？
   - [ ] 如果添加 ADR，`docs/adr/README.md` 和相应类别的 README 是否已更新？
   - [ ] 如果添加 Copilot Prompt，`docs/copilot/README.md` 是否已更新？

2. **交叉引用完整性**
   - [ ] 新文档是否在所有相关索引中被引用？
   - [ ] 目录结构图是否已更新？
   - [ ] 快速导航表是否已更新？
   - [ ] 时间线表（如适用）是否已更新？
   - [ ] 统计数字是否已更新？

3. **链接有效性**
   - [ ] 新文档中的所有链接是否可用？
   - [ ] 被引用的文档是否添加了反向链接？

### 常见遗漏

**最常见的文档错误**：
```markdown
❌ 错误：创建 docs/summaries/xxx-summary.md 但未更新 docs/summaries/README.md
✅ 正确：同时更新文档和索引文件

❌ 错误：只在一个索引表中添加，遗漏其他表（如时间线、主题导航）
✅ 正确：更新所有相关的索引表和统计数字
```

### 文档检查示例

```markdown
⚠️ **文档索引缺失**

检测到新文档创建但索引未更新：

**检测到**：
- 新文件：`docs/summaries/new-summary.md`
- 未更新：`docs/summaries/README.md`

**需要更新的位置**：
1. 目录结构图（添加新文件行）
2. 文档列表表格（按类别添加）
3. 快速导航表（添加主题）
4. 时间线表（添加日期条目）
5. 统计数字（更新总数和分类计数）

**参考**：`.github/instructions/documentation.instructions.md`（更新索引文件章节）
```
