# ADR-测试内容映射规范

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。详见 [ADR 目录](../adr/README.md)。
>
> 📘 **权威依据**：本指南说明如何实施以下 ADR：
> - [ADR-0000：架构测试与 CI 治理元规则](../adr/governance/ADR-0000-architecture-tests.md)
> - [ADR-900：ADR 新增与修订流程](../adr/governance/ADR-900-adr-process.md)
>
> 本文档中的"必须"/"禁止"等语言均指代上述 ADR 的要求，本文档不独立产生新规则。

**版本**：1.0  
**状态**：✅ 生效  
**最后更新**：2026-01-23

---

## 一、目的与背景

### 1.1 问题陈述

架构测试已存在，但面临以下风险：

- ❌ **形式合规，内容偏离**：测试类存在，但测试内容与 ADR 约束不一致
- ❌ **映射缺失**：无法追溯哪个测试对应哪条 ADR 约束
- ❌ **覆盖盲区**：ADR 新增约束，但没有对应的测试
- ❌ **失败不明确**：测试失败时无法快速定位违反了哪条 ADR

### 1.2 解决方案

建立 **ADR 文档 ↔ 测试断言 ↔ Prompts 文件** 三方强一致性机制：

```
┌─────────────┐     双向映射     ┌─────────────┐
│ ADR 文档    │ ←──────────────→ │ 架构测试    │
│ 标记约束    │                  │ 验证约束    │
└─────────────┘                  └─────────────┘
       ↑                                ↑
       │          三方交叉验证           │
       │                                │
       └────────→ ┌─────────────┐ ←────┘
                  │ Prompts文档 │
                  │ 解释约束    │
                  └─────────────┘
```

---

## 二、规范要求

### 2.1 ADR 文档标准

#### A. 约束标记规范

所有需要架构测试覆盖的约束应明确标记，支持以下任一标记：

```markdown
✅ 推荐标记方式：

1. 内联标记（置于约束条款后）
   - 这是需要遵守的约束。【必须架构测试覆盖】

2. 段落标记（置于约束段落后）
   **需要测试**：根据 ADR-0000，此约束应通过架构测试验证。

3. 英文标记（便于脚本解析）
   [MUST_TEST] 此约束需要自动化测试。
```

#### B. 快速参考表要求

每个 ADR 的 **快速参考表（Quick Reference）** 建议包含 **"测试覆盖"** 列：

```markdown
## 快速参考表

| 约束 | 描述 | 测试覆盖 | ADR 章节 |
|------|------|---------|----------|
| 模块隔离 | 模块不得相互引用 | ✅ ADR_0001_Architecture_Tests.cs::Modules_Should_Not_Reference_Other_Modules | 3.1 |
| 垂直切片 | 禁止横向 Service 层 | ✅ ADR_0001_Architecture_Tests.cs::Modules_Should_Not_Contain_Traditional_Layering | 3.2 |
```

**列说明**：

- **约束**：简短名称
- **描述**：约束内容摘要
- **测试覆盖**：格式为 `✅ 测试文件名::测试方法名`，如无测试则标记 `❌ 待补充`
- **ADR 章节**：对应 ADR 文档的章节号

#### C. 约束编号规范

为便于追溯，ADR 中的关键约束应采用 **层级编号**：

```markdown
### 3. 模块通信约束（ADR-0001.3）

#### 3.1 允许的通信方式（ADR-0001.3.1）
- 领域事件（Domain Events）【必须架构测试覆盖】
- 数据契约（Contracts）【必须架构测试覆盖】
- 原始类型【必须架构测试覆盖】

#### 3.2 禁止的通信方式（ADR-0001.3.2）
- 直接引用其他模块的实体 【必须架构测试覆盖】
```

---

### 2.2 架构测试代码标准

#### A. 测试类命名

**格式**：`ADR_{ADR编号}_Architecture_Tests`

```csharp
// ✅ 正确
public class ADR_0001_Architecture_Tests { }

// ❌ 错误
public class ModularMonolithTests { }
public class ArchitectureTests { }
```

#### B. 测试方法命名

**格式**：`{约束简称}_Should_{预期行为}` 或 `{ADR条款编号}_{约束描述}`

```csharp
// ✅ 推荐方式 1：语义化命名
[Theory(DisplayName = "ADR-0001.3.1: 模块不应相互引用")]
public void Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)

// ✅ 推荐方式 2：编号前缀命名
[Fact(DisplayName = "ADR-0001.3.2: 禁止横向 Service 层")]
public void ADR_0001_3_2_No_Horizontal_Service_Layer()
```

**要求**：

- 根据 ADR-0000，方法名或 DisplayName 应包含 `ADR-{编号}`
- 清晰表达验证的约束内容

#### C. 测试类头部注释

根据 ADR-0000，每个测试类**应**包含 ADR 映射清单：

```csharp
/// <summary>
/// ADR-0001: 模块化单体与垂直切片架构
/// 
/// 【测试覆盖映射】
/// ├─ ADR-0001.3.1: 模块隔离 → Modules_Should_Not_Reference_Other_Modules
/// ├─ ADR-0001.3.2: 垂直切片 → Modules_Should_Not_Contain_Traditional_Layering
/// ├─ ADR-0001.4.1: 契约通信 → Modules_Should_Only_Use_Contracts_For_Communication
/// └─ ADR-0001.4.2: 事件通信 → Modules_Should_Use_Events_For_Async_Communication
/// 
/// 【关联文档】
/// - ADR: docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md
/// - Prompts: docs/copilot/adr-0001.prompts.md
/// </summary>
public sealed class ADR_0001_Architecture_Tests
{
    // ...
}
```

#### D. 断言失败消息规范

**格式**：`❌ ADR-{编号} 违规：{约束描述}。\n{违规详情}\n修复建议：{如何修复}`

```csharp
Assert.True(result.IsSuccessful,
    $"❌ ADR-0001 违规: 模块 {moduleName} 不应依赖模块 {otherModule}。\n" +
    $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
    $"修复建议：将共享逻辑移至 Platform/BuildingBlocks，或改为消息通信（Publish/Invoke），或由 Bootstrapper/Coordinator 做模块级协调。\n" +
    $"参考：docs/copilot/adr-0001.prompts.md");
```

**要求**：

- 应包含 `ADR-{编号}`
- 明确说明违反了哪条约束
- 提供具体的违规详情
- 提供可操作的修复建议
- 可选：提供参考文档链接

---

### 2.3 Copilot Prompts 标准

#### A. 测试-ADR 一致性自检指令

根据 ADR-0000，每个 `adr-XXXX.prompts.md` 文件应包含 **"测试覆盖自检"** 章节：

```markdown
## 六、测试覆盖自检清单

在审查与 ADR-0001 相关的 PR 时，请检查以下映射关系：

### 映射清单

| ADR 约束 | 测试方法 | 状态 |
|---------|---------|------|
| ADR-0001.3.1: 模块不得相互引用 | `Modules_Should_Not_Reference_Other_Modules` | ✅ 已覆盖 |
| ADR-0001.3.2: 禁止横向 Service 层 | `Modules_Should_Not_Contain_Traditional_Layering` | ✅ 已覆盖 |
| ADR-0001.4.1: 使用契约通信 | `Modules_Should_Only_Use_Contracts` | ❌ 待补充 |

### 自检问题

当审查代码变更时，询问自己：

1. ✅ 根据 ADR-0000，所有需要测试覆盖的约束是否都有对应的测试方法？
2. ✅ 测试方法名是否清晰反映了 ADR 约束内容？
3. ✅ 测试失败消息是否包含 ADR 编号和修复建议？
4. ✅ 如果 ADR 新增了约束，是否同步新增了测试？
```

#### B. 编写测试的指导示例

```markdown
## 七、如何编写符合映射要求的测试

### 示例 1：模块隔离测试

\`\`\`csharp
[Theory(DisplayName = "ADR-0001.3.1: 模块不应相互引用")]
[ClassData(typeof(ModuleAssemblyData))]
public void Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)
{
    var moduleName = moduleAssembly.GetName().Name!.Split('.').Last();
    
    foreach (var other in ModuleAssemblyData.ModuleNames.Where(m => m != moduleName))
    {
        var result = Types.InAssembly(moduleAssembly)
            .ShouldNot()
            .HaveDependencyOn($"Zss.BilliardHall.Modules.{other}")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0001.3.1 违规: 模块 {moduleName} 不应依赖模块 {other}。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName))}。\n" +
            $"修复建议：使用领域事件或数据契约进行模块间通信。\n" +
            $"参考：docs/copilot/adr-0001.prompts.md 场景 3");
    }
}
\`\`\`

**要点说明**：
- ✅ DisplayName 明确标注 ADR-0001.3.1
- ✅ 失败消息包含 ADR 编号
- ✅ 提供具体修复建议和参考文档
```

---

## 三、自动化校验机制

### 3.1 校验脚本

项目提供两个校验脚本：

- **PowerShell**：`scripts/validate-adr-test-mapping.ps1`
- **Bash**：`scripts/validate-adr-test-mapping.sh`

### 3.2 脚本功能

1. **扫描 ADR 文档**：
  - 提取所有标记为测试覆盖的约束（根据 ADR-0000 定义的标记格式）
  - 统计需要测试的约束数量

2. **扫描测试代码**：
  - 提取所有测试方法
  - 检查是否包含 ADR 引用

3. **生成映射报告**：
  - 标记缺少测试的 ADR 约束
  - 标记缺少 ADR 引用的测试方法
  - 统计覆盖率

4. **校验结果**：
  - 通过：所有约束都有测试，所有测试都有 ADR 引用
  - 失败：存在映射不一致，返回非零退出码

### 3.3 本地运行

```bash
# PowerShell
./scripts/validate-adr-test-mapping.ps1

# Bash
./scripts/validate-adr-test-mapping.sh
```

### 3.4 CI 集成

校验已集成到 GitHub Actions 工作流（见 `.github/workflows/architecture-tests.yml`），在以下时机自动运行：

- PR 提交时
- 合并到 main 分支时

---

## 四、PR 审查流程

### 4.1 开发者自检

提交 PR 前，在 PR 模板中完成以下检查：

```markdown
## ADR-测试一致性检查

- [ ] 根据 ADR-0000，所有需要测试的条款都有对应测试方法
- [ ] 所有测试断言都引用了具体 ADR 及条款
- [ ] 本地运行 `./scripts/validate-adr-test-mapping.sh` 通过
- [ ] Copilot 审查已确认测试与 ADR 内容无偏差
```

### 4.2 Copilot 审查要点

在 Copilot 审查环节，提示 Copilot 检查：

```
请审查本 PR 的 ADR-测试一致性：

1. 根据 ADR-0000，检查是否所有需要测试覆盖的约束都有对应测试
2. 检查测试方法命名是否包含 ADR 引用
3. 检查测试失败消息是否符合规范（包含 ADR 编号、违规详情、修复建议）
4. 如果修改了 ADR 文档，检查测试是否同步更新
5. 如果修改了测试，检查 ADR 文档和 Prompts 是否同步更新
```

### 4.3 人工审查重点

审查者应关注：

- ❗ **逆向验证**：给定测试类，能否逆推出 ADR 所有约束都被覆盖？
- ❗ **语义一致**：测试验证的是否真的是 ADR 要求的约束？
- ❗ **失败可读性**：测试失败时，消息是否足够明确？

---

## 五、维护指南

### 5.1 新增 ADR 时

1. **编写 ADR 文档**：
  - 根据 ADR-0000，为需要测试的约束添加测试覆盖标记
  - 在快速参考表中预留"测试覆盖"列

2. **编写架构测试**：
  - 创建 `ADR_XXXX_Architecture_Tests.cs`
  - 按规范编写测试方法和失败消息
  - 在测试类头部添加映射清单注释

3. **编写 Prompts**：
  - 创建 `adr-XXXX.prompts.md`
  - 添加测试覆盖自检清单
  - 提供编写测试的示例

4. **运行校验**：
   ```bash
   ./scripts/validate-adr-test-mapping.sh
   dotnet test src/tests/ArchitectureTests/
   ```

### 5.2 修订 ADR 时

1. **更新 ADR 约束**：
  - 根据 ADR-0000，新增约束时添加测试覆盖标记
  - 废弃约束时更新快速参考表

2. **同步更新测试**：
  - 新增对应的测试方法
  - 或删除/修改已有测试（如约束变更）

3. **同步更新 Prompts**：
  - 更新测试覆盖清单
  - 更新示例代码

4. **运行校验**：
  - 确保映射关系仍然一致

### 5.3 定期审计

建议每季度执行一次全面审计：

1. 运行校验脚本，检查是否有遗漏
2. 人工抽查测试内容是否与 ADR 语义一致
3. 检查测试失败消息是否仍然准确
4. 更新测试覆盖映射文档

---

## 六、常见问题

### Q1: 是否所有 ADR 约束都需要测试？

**A**: 不是。根据 ADR-0000，只有标记为需要测试覆盖的约束才需要自动化测试。以下情况不需要：

- 纯概念性的指导原则
- 需要人工判断的最佳实践
- 无法通过静态分析验证的约束

### Q2: 如果某条约束暂时无法自动化测试怎么办？

**A**: 在 ADR 文档中明确标注：

```markdown
- 此约束目前通过人工 Code Review 验证。【待自动化测试】
```

并在快速参考表中标记：

```markdown
| 约束 | 描述 | 测试覆盖 | 备注 |
|------|------|---------|------|
| XXX | ... | ⏳ 人工 Code Review | 待工具支持 |
```

### Q3: 测试方法很多，如何避免遗漏？

**A**:

1. 在测试类头部维护映射清单注释
2. 定期运行校验脚本
3. 在 PR 模板中强制要求检查

### Q4: 如果校验脚本误报怎么办？

**A**:

1. 根据 ADR-0000，检查是否正确使用了测试覆盖标记
2. 检查测试方法是否包含 ADR 引用（方法名或 DisplayName）
3. 如确实是脚本问题，请提交 Issue 改进脚本

---

## 七、参考资料

- [ADR-0000: 架构测试与 CI 治理](../adr/governance/ADR-0000-architecture-tests.md)
- [ADR-900: ADR 新增与修订流程](../adr/governance/ADR-900-adr-process.md)
- [Copilot 治理体系](../copilot/README.md)
- [PR 模板](../.github/PULL_REQUEST_TEMPLATE.md)

---

**版本历史**：

- v1.0 (2026-01-23): 初始版本，定义 ADR-测试映射规范
