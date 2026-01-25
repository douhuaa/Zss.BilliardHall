# ADR 实施指南

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。详见 [ADR 目录](README.md)。

**版本**：1.0  
**创建日期**：2026-01-24  
**状态**：Active  
**适用范围**：所有待实施的 ADR 提案

---

## 目标读者

本指南面向以下角色：

- **架构师**：规划和设计 ADR
- **Tech Lead**：审批和协调 ADR 实施
- **开发者**：具体实施 ADR
- **GitHub Copilot**：辅助 ADR 创建和验证

---

## 核心原则

### 1. 三位一体交付

根据 ADR-0000 和 ADR-0008，每个 ADR 应同时交付三个部分：

```
ADR 正文 + 架构测试 + Copilot 提示词 = 完整的治理体系
```

**反面案例**：
- ❌ "先提交 ADR，测试下周补"
- ❌ "Copilot 提示词不重要，可以省略"
- ❌ "测试写了但没通过，先合并吧"

**正确做法**：
- ✅ 同一个 PR 包含全部三个交付物
- ✅ 所有测试在本地和 CI 都通过
- ✅ Copilot 提示词覆盖所有主要场景

### 2. 裁决型而非说明型

ADR 是法律条文，不是教科书：

```markdown
❌ 错误写法（说明型）：
Handler 是处理业务逻辑的核心组件。当创建订单时，我们需要：
1. 验证用户信息
2. 检查库存
3. 创建订单记录
...（长篇大论）

✅ 正确写法（裁决型）：
ADR-201.1：Handler 必须注册为 Scoped 生命周期
- 执行级别：L1
- 架构测试：HandlersMustBeScoped
- 违规后果：CI 失败

ADR-201.2：Handler 禁止依赖 Singleton 有状态服务
- 执行级别：L2
- 架构测试：HandlersCannotDependOnStatefulSingletons
- 违规后果：编译期错误（Analyzer）
```

### 3. 可判定性

根据架构测试要求，每条规则应能够自动判定，避免模糊表达。

**对比示例**（左侧为模糊表达，右侧为 ADR 正文中的可判定表达）：

```
❌ 模糊："应该记录关键日志"  
✅ 可判定："所有 Handler 必须记录带 CorrelationId 的日志"

❌ 模糊："尽量使用异步"  
✅ 可判定："跨模块通信必须使用异步事件"

❌ 模糊："合理组织代码"  
✅ 可判定："测试类必须位于与源码镜像的目录结构"

❌ 模糊："避免过度耦合"  
✅ 可判定："模块禁止直接引用其他模块的命名空间"
```

**说明**：右侧示例展示了 ADR 正文应该如何编写（使用明确的"必须"/"禁止"），本指南文档本身不使用裁决性语言。

---

## 实施流程（详细步骤）

### 阶段 1：RFC 准备（1-2 天）

#### 1.1 确定 ADR 编号和层级

使用决策树：

```
问题 1：这个决策影响系统根基吗？
├─ 是 → 宪法层（0001~0099）【极少新增，需架构委员会】
└─ 否 → 问题 2

问题 2：这是关于治理流程还是架构约束？
├─ 治理流程 → 治理层（900~999）
└─ 架构约束 → 问题 3

问题 3：这是静态结构还是运行时行为？
├─ 静态结构 → 结构层（100~199）
└─ 运行时行为 → 问题 4

问题 4：这是行为边界还是技术实现？
├─ 行为边界 → 运行层（200~299）
└─ 技术实现 → 技术层（300~399）
```

#### 1.2 使用 RFC 模板创建提案

```bash
# 复制模板
cp docs/templates/rcf-template.md docs/adr/[layer]/RFC-[XXX]-[name].md

# 填写内容（参考模板各章节）
```

**RFC 应包含**（参考 ADR-0900）：
- [ ] 背景：为什么需要这个 ADR？
- [ ] 问题：要解决什么痛点？
- [ ] 提议：具体的裁决规则（3-10 条）
- [ ] 备选方案：考虑过哪些其他方案？
- [ ] 影响分析：对现有 ADR、代码、团队的影响
- [ ] 实施计划：测试、Prompt、迁移计划
- [ ] 开放问题：需要进一步讨论的点

#### 1.3 创建 GitHub Issue 进行讨论

```markdown
标题：[RFC] ADR-XXX: [标题]

内容：
- 链接到 RFC 文档
- @ 相关人员
- 设置标签：rfc, adr-proposal
```

#### 1.4 收集反馈并修订

- 至少等待 2-3 个工作日
- 回应所有评论
- 根据反馈修订 RFC
- 达成共识后申请审批

---

### 阶段 2：ADR 正文创建（2-3 天）

#### 2.1 使用 ADR 模板

```bash
cp docs/templates/adr-template.md docs/adr/[layer]/ADR-[XXX]-[name].md
```

#### 2.2 编写规则本体（Rule）

**结构要求**：

```markdown
## 规则本体（Rule）

### ADR-XXX.1：[规则标题]

**约束**：[精确描述，ADR正文中使用"必须"/"禁止"]

**执行级别**：L1/L2/L3

**架构测试**：[测试类名].[测试方法名]

**示例**：

```csharp
// ✅ 正确
public class GoodExample { }

// ❌ 错误
public class BadExample { }
```

---

### ADR-XXX.2：[规则标题]
...
```

**每条规则清单**：
- [ ] 有清晰的标题
- [ ] 约束描述精确（不含"建议"、"推荐"）
- [ ] 标注执行级别（L1/L2/L3）
- [ ] 指定对应的架构测试
- [ ] 提供 1 个正面 + 1 个反面示例（不超过 10 行）

#### 2.3 编写执法模型（Enforcement）

```markdown
## 执法模型（Enforcement）

| 规则编号 | 执行级 | 测试/手段 | CI 阻断 |
|---------|--------|-----------|---------|
| ADR-XXX.1 | L1 | NetArchTest: HandlersMustBeScoped | ✅ 是 |
| ADR-XXX.2 | L2 | Roslyn Analyzer: NoStatefulSingletons | ✅ 是 |
| ADR-XXX.3 | L3 | Code Review | ❌ 否 |
```

#### 2.4 控制文档长度

**目标长度**：
- 裁决型 ADR：150-300 行
- 结构层 ADR：200-400 行
- 技术层 ADR：250-500 行

**如果超长，考虑**：
1. 删除背景论证（移至 RFC）
2. 精简示例（每条规则 1 正 + 1 反）
3. 移除实施细节（创建工程标准文档）
4. 拆分为多个 ADR

---

### 阶段 3：架构测试编写（2-4 天）

#### 3.1 创建测试文件

```bash
# 路径：src/tests/ArchitectureTests/ADR/
touch src/tests/ArchitectureTests/ADR/ADR_0XXX_Architecture_Tests.cs
```

#### 3.2 测试结构模板

```csharp
using NetArchTest.Rules;
using Xunit;

namespace Zss.BilliardHall.ArchitectureTests.ADR;

/// <summary>
/// ADR-XXX：[ADR 标题]
/// 参考：docs/adr/[layer]/ADR-XXX-[name].md
/// </summary>
public class ADR_0XXX_Architecture_Tests
{
    [Fact]
    public void Rule_XXX_1_Description()
    {
        // Arrange
        var types = Types.InCurrentDomain()
            .That()
            .ResideInNamespace("Zss.BilliardHall.Modules")
            .GetTypes();

        // Act
        var result = types
            .Should()
            .MeetCustomRule(new CustomRule())
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"【ADR-XXX.1 违规】{result.FailingTypes?.Count() ?? 0} 个类型违反规则：\n" +
            string.Join("\n", result.FailingTypes ?? Enumerable.Empty<Type>()));
    }
}
```

#### 3.3 测试覆盖清单

- [ ] 每个 L1/L2 规则有对应测试
- [ ] 测试方法命名清晰（描述被验证的规则）
- [ ] 失败消息引用 ADR 编号和规则编号
- [ ] 测试在本地通过：`dotnet test --filter "FullyQualifiedName~ADR_0XXX"`

#### 3.4 常见测试模式

**模式 1：命名约束**

```csharp
var result = types
    .That().HaveNameEndingWith("Handler")
    .Should().HaveNameMatching(@"^[A-Z][a-zA-Z0-9]+Handler$")
    .GetResult();
```

**模式 2：依赖约束**

```csharp
var result = types
    .That().ResideInNamespace("Zss.BilliardHall.Platform")
    .ShouldNot().HaveDependencyOn("Zss.BilliardHall.Application")
    .GetResult();
```

**模式 3：继承约束**

```csharp
var result = types
    .That().ImplementInterface(typeof(ICommandHandler<>))
    .Should().BeSealed()
    .GetResult();
```

**模式 4：特性约束**

```csharp
var result = types
    .That().HaveNameEndingWith("Event")
    .Should().HaveCustomAttribute(typeof(DataContractAttribute))
    .GetResult();
```

---

### 阶段 4：Copilot 提示词创建（1-2 天）

#### 4.1 创建提示词文件

```bash
touch docs/copilot/adr-0xxx.prompts.md
```

#### 4.2 提示词结构

```markdown
# ADR-XXX Copilot 提示词库

**版本**：1.0  
**ADR 正文**：[链接到 ADR]  
**架构测试**：[链接到测试文件]

---

## 触发场景（When to Trigger）

Copilot 应在以下场景主动提示：

1. **场景 1**：开发者创建新的 Handler 类
   - 检查：生命周期注册是否为 Scoped
   - 提示：ADR-XXX.1 要求 Handler 必须注册为 Scoped

2. **场景 2**：开发者修改 Handler 构造函数
   - 检查：是否依赖了 Singleton 有状态服务
   - 提示：参考 ADR-XXX.2 关于依赖 Singleton 有状态服务的约束

...（4-5 个场景）

---

## 应阻止的反模式（参考本指南）

### 反模式 1：Handler 注册为 Singleton

```csharp
❌ 错误：
services.AddSingleton<CreateOrderHandler>();

✅ 正确：
services.AddScoped<CreateOrderHandler>();
```

**阻止理由**：违反 ADR-XXX.1

---

## CI 失败诊断（Failure Diagnosis）

### 测试：HandlersMustBeScoped

**失败消息**：
```
【ADR-XXX.1 违规】3 个类型违反规则：
- MyModule.MyHandler
- OtherModule.OtherHandler
```

**原因分析**：
- 这些 Handler 未注册为 Scoped 生命周期
- 或者使用了 Transient/Singleton

**修复建议**：
1. 检查 Program.cs 或 ServiceCollectionExtensions
2. 修改为：`services.AddScoped<YourHandler>()`
3. 重新运行测试验证

---

## 快速参考卡片（ADR正文示例）

```
规则 | 简述 | 执行级别 | CI 阻断
-----|------|----------|--------
ADR-XXX.1 | Handler 必须 Scoped | L1 | ✅
ADR-XXX.2 | 禁止依赖 Singleton 状态 | L2 | ✅
```

**说明**：以上为 ADR 正文中的规则示例格式。

---

## 开发者自检清单

创建/修改 Handler 时检查：
- [ ] 是否注册为 Scoped？
- [ ] 是否依赖了 Singleton 有状态服务？
- [ ] 是否实现了 IDisposable（如有需要）？
- [ ] 是否使用了静态字段存储状态？
```

#### 4.3 提示词清单

- [ ] 包含 4-5 个触发场景
- [ ] 包含 3-5 个应阻止的反模式
- [ ] 每个架构测试有对应的失败诊断
- [ ] 包含快速参考卡片
- [ ] 包含开发者自检清单

---

### 阶段 5：集成与验证（1 天）

#### 5.1 本地验证

```bash
# 1. 运行架构测试
dotnet test --filter "FullyQualifiedName~ADR_0XXX"

# 2. 运行全部架构测试（确保没有破坏其他测试）
dotnet test src/tests/ArchitectureTests/

# 3. 验证文档链接
# 手动检查所有链接可访问

# 4. 检查文档长度
wc -l docs/adr/[layer]/ADR-XXX-*.md
```

#### 5.2 更新索引文件

**应更新的文件**：

1. `docs/adr/README.md`
   - 在"已采纳的 ADR"章节添加新 ADR
   - 更新统计数字

2. `docs/adr/[layer]/README.md`
   - 在"当前 ADR 列表"中添加新 ADR
   - 从"待落地的 ADR"中移除

3. `docs/copilot/README.md`
   - 添加新的 Copilot 提示词文件

4. `docs/adr/PENDING-ADR-PROPOSALS.md`
   - 更新提案状态为 ✓ Completed
   - 更新进度统计

#### 5.3 创建 PR

```bash
# 1. 提交所有更改
git add .
git commit -m "feat(ADR-XXX): 实施 [ADR 标题]

三位一体交付：
- ADR 正文：docs/adr/[layer]/ADR-XXX-[name].md
- 架构测试：src/tests/ArchitectureTests/ADR/ADR_0XXX_Architecture_Tests.cs
- Copilot 提示词：docs/copilot/adr-0xxx.prompts.md

影响范围：[描述]

参考：RFC-XXX"

# 2. 推送并创建 PR
git push origin feature/adr-xxx
# 通过 GitHub UI 创建 PR
```

**PR 描述模板**：

```markdown
## ADR-XXX：[标题]

### 变更类型
- [x] 新增 ADR
- [ ] 修订 ADR
- [ ] 废弃 ADR

### 三位一体交付清单
- [x] ADR 正文：docs/adr/[layer]/ADR-XXX-[name].md
- [x] 架构测试：src/tests/ArchitectureTests/ADR/ADR_0XXX_Architecture_Tests.cs
- [x] Copilot 提示词：docs/copilot/adr-0xxx.prompts.md
- [x] 更新索引：docs/adr/README.md, docs/adr/[layer]/README.md, docs/copilot/README.md

### 质量门禁
- [x] 文档长度 < 400 行
- [x] 规则数量：[N] 条（3-10 条）
- [x] 所有规则标注执行级别
- [x] 所有 L1/L2 规则有对应测试
- [x] 所有测试通过
- [x] 无柔性词汇（"建议"、"推荐"）

### 影响分析
- 对现有 ADR 的影响：[无 / 有，列出]
- 对现有代码的影响：[无 / 有，说明迁移计划]
- 对团队的影响：[无 / 有，是否需要培训]

### 参考
- RFC：[链接]
- Issue：[链接]
```

---

## 常见问题与解决方案

### Q1：ADR 写得太长怎么办？

**A：** 应用"拆分三原则"：

1. **裁决 vs 指导**：
   - 裁决规则 → 保留在 ADR
   - 最佳实践、详细解释 → 移至 `docs/guides/` 工程标准

2. **核心 vs 细节**：
   - 核心约束（不可违反）→ ADR
   - 实施细节（可选方案）→ 附录或工程标准

3. **层级归属**：
   - 运行层内容混入技术细节 → 拆分出技术层 ADR
   - 多个独立关注点 → 拆分为多个 ADR

### Q2：规则无法自动判定怎么办？

**A：** 使用"判定性转换"（以下为 ADR 正文写法示例）：

```
不可判定表达 → 判定性转换（ADR正文示例）

"应该记录详细日志" → "Handler 必须调用 ILogger.LogInformation"
"合理组织目录" → "测试目录必须镜像源码目录结构"  
"避免紧耦合" → "模块禁止引用其他模块命名空间"
```

**如果实在无法转换**：
- 这不应该是 ADR 规则
- 移至工程标准文档（非裁决性）
- 标注为 L3（Code Review 检查，不 CI 阻断）

### Q3：如何处理与现有代码冲突？

**A：** 遵循"新老划断"原则：

1. **评估影响面**：
   ```bash
   # 运行新的架构测试
   dotnet test --filter "FullyQualifiedName~ADR_0XXX"
   # 记录失败数量和位置
   ```

2. **制定迁移计划**：
   - 小于 10 个违规：本 PR 一起修复
   - 10-50 个违规：创建后续 Issue，标记 tech-debt
   - 大于 50 个违规：分阶段迁移，先 ADR 标记为"渐进式"

3. **破例管理**：
   - 如果确实无法立即修复，记录在 `ARCH-VIOLATIONS.md`
   - 必须指定归还日期和责任人

### Q4：Copilot 提示词写什么内容？

**A：** 关注"开发者痛点场景"：

**好的提示词**（场景驱动）：
```markdown
## 场景 1：创建新的事件订阅者

当开发者：
1. 创建类实现 IEventHandler<TEvent>
2. 注入其他模块的服务

Copilot 应：
- 警告：参考 ADR-XXX.2 关于跨模块事件订阅者的约束
- 提示：根据 ADR-XXX.2，应异步处理
- 建议：使用 Task.Run 或发布新的命令
```

**坏的提示词**（脱离场景）：
```markdown
## 提示 1：遵守 ADR-XXX

Copilot 应提醒开发者遵守 ADR-XXX 的所有规则。
```

### Q5：测试失败但我认为代码是对的？

**A：** 优先级判定：

1. **ADR 是对的**（99% 情况）：
   - 修改代码使其符合 ADR
   - ADR 是经过审批的架构约束

2. **ADR 需要修订**（1% 情况）：
   - 提交新的 RFC 修订 ADR
   - 在修订批准前，代码仍需遵守现有 ADR

3. **真实破例**（极少数）：
   - 申请破例并记录在 `ARCH-VIOLATIONS.md`
   - 指定归还计划

**绝不允许**：
- ❌ 修改测试以通过代码
- ❌ 注释掉失败的测试
- ❌ 跳过测试合并代码

---

## 成功案例参考

### 案例 1：ADR-121（契约命名规范）

**初稿问题**：
- 文档 955 行（过长）
- 包含大量解释和示例
- 版本管理策略模糊

**重构后**：
- 文档 381 行（精简 60%）
- 严格遵循 ADR 模板
- 补充废弃标记策略
- v1.2 通过审批

**关键改进**：
1. 删除所有"为什么"解释
2. 每条规则仅保留 1 正 + 1 反示例
3. 废弃策略提炼为 3 个阶段（警告→错误→移除）

### 案例 2：ADR-240（Handler 异常约束）

**初稿问题**：
- 文档 950 行（包含大量工程实践）
- 部分规则不可自动判定
- 运行层 ADR 包含技术实现细节

**重构后**：
- ADR-240（裁决型）：~200 行，5 条规则
- 工程标准：~750 行，详细实践指导
- 100% 规则可自动判定

**关键改进**：
1. 拆分为 ADR + 工程标准
2. 删除"重试 3-5 次"等技术细节
3. 移除"记录详细日志"等不可判定规则

---

## 时间估算参考

| 阶段 | 简单 ADR | 中等 ADR | 复杂 ADR |
|------|---------|---------|---------|
| RFC 准备 | 0.5 天 | 1 天 | 2 天 |
| ADR 正文 | 1 天 | 2 天 | 3 天 |
| 架构测试 | 1 天 | 2 天 | 4 天 |
| Copilot 提示词 | 0.5 天 | 1 天 | 2 天 |
| 集成与验证 | 0.5 天 | 1 天 | 1 天 |
| **总计** | **3-4 天** | **6-7 天** | **12-13 天** |

**复杂度分类**：
- **简单**：主要是命名、组织规范（如 ADR-122、ADR-123）
- **中等**：涉及生命周期、流程（如 ADR-201、ADR-930）
- **复杂**：涉及分布式、版本化、事务（如 ADR-210、ADR-220）

---

## 质量自检清单（最终）

在创建 PR 前，确保：

### ADR 正文
- [ ] 使用 ADR 模板结构
- [ ] 规则数量：3-10 条
- [ ] 文档长度：< 400 行
- [ ] 每条规则有清晰标题
- [ ] 每条规则有执行级别标注
- [ ] 每条规则有对应架构测试引用
- [ ] 无"建议"、"推荐"等柔性词汇
- [ ] 示例代码精简（每条规则 1 正 + 1 反，< 10 行）
- [ ] 无大段背景解释或实施指导

### 架构测试
- [ ] 每个 L1/L2 规则有对应测试
- [ ] 测试方法命名描述被验证的规则
- [ ] 失败消息引用 ADR 编号和规则编号
- [ ] 所有测试在本地通过
- [ ] 所有测试在 CI 通过

### Copilot 提示词
- [ ] 包含 4-5 个触发场景
- [ ] 包含 3-5 个应阻止的反模式
- [ ] 每个架构测试有失败诊断
- [ ] 包含快速参考卡片
- [ ] 包含开发者自检清单

### 索引更新
- [ ] 更新 docs/adr/README.md
- [ ] 更新 docs/adr/[layer]/README.md
- [ ] 更新 docs/copilot/README.md
- [ ] 更新 docs/adr/PENDING-ADR-PROPOSALS.md
- [ ] 验证所有链接有效

### PR 质量
- [ ] PR 标题遵循 Conventional Commits
- [ ] PR 描述使用标准模板
- [ ] 包含影响分析
- [ ] 标注相关 Issue/RFC

---

## 附录：工具与资源

### 文档模板
- [ADR 模板](../templates/adr-template.md)
- [RFC 模板](../templates/rcf-template.md)
- [Copilot 提示词模板](../templates/copilot-pormpts-template.md)

### 参考 ADR
- [ADR-0900：ADR 新增与修订流程](./governance/ADR-0900-adr-process.md)
- [ADR-0000：架构测试元规则](./governance/ADR-0000-architecture-tests.md)

### 参考文档
- [PR 常见问题总结](../copilot/pr-common-issues.prompts.md)
- [待落地 ADR 提案跟踪清单](./PENDING-ADR-PROPOSALS.md)

### 自动化脚本

```bash
# 运行特定 ADR 的测试
./scripts/test-adr.sh 0XXX

# 验证 ADR 文档格式
./scripts/lint-adr.sh docs/adr/[layer]/ADR-XXX-*.md

# 检查索引一致性
./scripts/verify-adr-index.sh
```

（注：脚本需要根据实际需求开发）

---

## 版本历史

| 版本 | 日期 | 变更说明 | 修订人 |
|------|------|----------|--------|
| 1.0 | 2026-01-24 | 初始版本，创建 ADR 实施指南 | GitHub Copilot |
