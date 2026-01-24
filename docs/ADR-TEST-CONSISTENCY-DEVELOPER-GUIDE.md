# ADR-测试一致性开发者指南

**版本**：1.0  
**目标受众**：开发者、架构师  
**最后更新**：2026-01-23

---

## 一、快速开始

### 1.1 我需要做什么？

如果你的 PR 涉及以下任一内容，**必须**确保 ADR-测试映射一致性：

- ✅ 新增或修订 ADR 文档
- ✅ 新增或修改架构测试
- ✅ 修改影响架构约束的代码

### 1.2 三步自检流程

```bash
# 步骤 1：运行映射验证脚本
./scripts/validate-adr-test-mapping.sh

# 步骤 2：运行架构测试
dotnet test src/tests/ArchitectureTests/

# 步骤 3：填写 PR 模板中的"ADR-测试一致性检查"
```

---

## 二、场景化指导

### 场景 1：我新增了一个 ADR 约束

#### 步骤 1：在 ADR 文档中标记约束

在需要测试的约束后添加 **【必须架构测试覆盖】** 标记：

```markdown
### 3. 模块通信约束

仅允许以下三种模块间通信方式：

1. 领域事件（Domain Events） **【必须架构测试覆盖】**
2. 数据契约（Contracts，只读稳定 DTO） **【必须架构测试覆盖】**
3. 原始类型 / 标准库类型

严禁行为：

- ❌ 跨模块引用 Entity / Aggregate / ValueObject **【必须架构测试覆盖】**
```

#### 步骤 2：在快速参考表中记录

```markdown
## 快速参考表（Quick Reference Table）

| 约束编号 | 约束描述 | 必须测试 | 测试覆盖 | ADR 章节 |
|---------|---------|---------|---------|----------|
| ADR-0001.1 | 模块不得相互引用 | ✅ | ADR_0001_Architecture_Tests::Modules_Should_Not_Reference_Other_Modules | 1 |
| ADR-0001.3 | 跨模块禁止引用实体 | ✅ | ❌ 待补充 | 3 |
```

#### 步骤 3：编写对应的架构测试

在 `src/tests/ArchitectureTests/ADR/ADR_XXXX_Architecture_Tests.cs` 中添加测试方法：

```csharp
[Theory(DisplayName = "ADR-0001.3: 模块不得跨模块引用实体")]
[ClassData(typeof(ModuleAssemblyData))]
public void Modules_Should_Not_Reference_Entities_Across_Modules(Assembly moduleAssembly)
{
    // 测试逻辑
    var result = Types.InAssembly(moduleAssembly)
        .That().ResideInNamespace("*.Domain.Entities")
        .ShouldNot().BeUsedByOtherAssemblies()
        .GetResult();

    Assert.True(result.IsSuccessful,
        $"❌ ADR-0001.3 违规: 模块实体被其他模块引用。\n" +
        $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName))}。\n" +
        $"修复建议：使用数据契约（DTO）替代直接引用实体。\n" +
        $"参考：docs/copilot/adr-0001.prompts.md");
}
```

#### 步骤 4：更新 Prompts 文件

在 `docs/copilot/adr-XXXX.prompts.md` 的"测试覆盖自检清单"中添加：

```markdown
| ADR 约束 | 测试方法 | 测试文件 | 状态 |
|---------|---------|---------|------|
| ADR-0001.3: 跨模块禁止引用实体 | `Modules_Should_Not_Reference_Entities_Across_Modules` | ADR_0001_Architecture_Tests.cs | ✅ 已覆盖 |
```

#### 步骤 5：验证

```bash
# 验证映射一致性
./scripts/validate-adr-test-mapping.sh

# 运行测试
dotnet test src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs
```

---

### 场景 2：我修改了现有的 ADR 约束

#### 步骤 1：更新 ADR 文档

修改或删除约束，确保 **【必须架构测试覆盖】** 标记仍然准确。

#### 步骤 2：同步更新测试

- 如果约束变更，修改对应的测试方法
- 如果约束删除，删除或注释对应的测试方法
- 如果约束新增，按"场景 1"添加测试

#### 步骤 3：更新快速参考表

确保快速参考表中的"测试覆盖"列反映最新的测试方法。

#### 步骤 4：更新 Prompts 文件

同步更新 `docs/copilot/adr-XXXX.prompts.md` 中的测试覆盖映射表。

---

### 场景 3：我新增了一个架构测试

#### 步骤 1：确保测试方法包含 ADR 引用

测试方法名或 `DisplayName` 必须包含 `ADR-{编号}`：

```csharp
// 方式 1：在 DisplayName 中
[Theory(DisplayName = "ADR-0002.1: Platform 不应依赖 Application")]
public void Platform_Should_Not_Depend_On_Application()

// 方式 2：在方法名中
[Fact]
public void ADR_0002_1_Platform_Should_Not_Depend_On_Application()
```

#### 步骤 2：测试失败消息必须包含 ADR 引用

```csharp
Assert.True(result.IsSuccessful,
    $"❌ ADR-0002.1 违规: Platform 不应依赖 Application。\n" +
    $"违规类型: {violatingTypes}。\n" +
    $"修复建议：{fixSuggestion}。\n" +
    $"参考：docs/copilot/adr-0002.prompts.md");
```

#### 步骤 3：在测试类头部更新映射清单

```csharp
/// <summary>
/// ADR-0002: Platform / Application / Host 三层启动体系
/// 
/// 【测试覆盖映射】
/// ├─ ADR-0002.1: Platform 不依赖 Application → Platform_Should_Not_Depend_On_Application
/// ├─ ADR-0002.2: Application 不依赖 Host → Application_Should_Not_Depend_On_Host
/// └─ ADR-0002.3: Host 无业务逻辑 → Host_Should_Not_Contain_Business_Logic
/// </summary>
```

#### 步骤 4：更新 ADR 文档和 Prompts

确保 ADR 文档的快速参考表和 Prompts 文件的映射表包含新测试。

---

### 场景 4：架构测试失败了

#### 步骤 1：查看失败消息

失败消息应该包含：

- ❌ ADR 编号
- 违规的具体内容
- 修复建议
- 参考文档

#### 步骤 2：询问 Copilot

将失败日志粘贴给 Copilot：

```
请根据以下架构测试失败日志，解释违规原因并提供修复建议：

[粘贴完整的失败日志]
```

#### 步骤 3：修复代码

根据失败消息和 Copilot 的建议修复代码，而不是修改测试（除非测试本身有问题）。

#### 步骤 4：重新运行测试

```bash
dotnet test src/tests/ArchitectureTests/
```

---

## 三、工具使用

### 3.1 验证脚本

#### PowerShell (Windows)

```powershell
# 基本运行
./scripts/validate-adr-test-mapping.ps1

# 详细输出
./scripts/validate-adr-test-mapping.ps1 -Verbose
```

#### Bash (Linux/macOS)

```bash
# 基本运行
./scripts/validate-adr-test-mapping.sh
```

#### 输出解释

```
📊 验证总结
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

ADR 文档统计：
  总 ADR 数：8
  总约束条款数：14          ← ADR 中标记【必须架构测试覆盖】的条款数
  有测试覆盖：14             ← 有对应测试文件的条款数
  缺少测试：0                ← 需要补充测试的条款数（应为 0）

测试文件统计：
  总测试方法数：71           ← 所有架构测试方法数
  有 ADR 引用：162           ← 包含 ADR 引用的测试方法数（可能重复计数）
  缺少 ADR 引用：-91         ← 计算差异（负数说明引用充足）
```

### 3.2 本地测试运行

```bash
# 运行所有架构测试
dotnet test src/tests/ArchitectureTests/

# 运行特定 ADR 的测试
dotnet test src/tests/ArchitectureTests/ --filter "FullyQualifiedName~ADR_0001"

# 运行特定测试方法
dotnet test src/tests/ArchitectureTests/ --filter "FullyQualifiedName~Modules_Should_Not_Reference_Other_Modules"
```

---

## 四、最佳实践

### 4.1 测试命名

✅ **推荐**：

```csharp
[Theory(DisplayName = "ADR-0001.1: 模块不应相互引用")]
public void Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)

[Fact(DisplayName = "ADR-0002.3: Host 不应包含业务逻辑")]
public void Host_Should_Not_Contain_Business_Logic()
```

❌ **避免**：

```csharp
// 缺少 ADR 引用
[Fact]
public void TestModuleIsolation()

// 命名不清晰
[Fact]
public void Test1()
```

### 4.2 失败消息

✅ **推荐**：

```csharp
Assert.True(result.IsSuccessful,
    $"❌ ADR-0001.1 违规: 模块 {moduleName} 不应依赖模块 {otherModule}。\n" +
    $"违规类型: {string.Join(", ", failingTypes)}。\n" +
    $"修复建议：\n" +
    $"  1. 使用领域事件进行异步通信\n" +
    $"  2. 使用数据契约进行只读查询\n" +
    $"  3. 传递原始类型（Guid、string）\n" +
    $"参考：docs/copilot/adr-0001.prompts.md 场景 3");
```

❌ **避免**：

```csharp
// 缺少 ADR 引用、违规详情和修复建议
Assert.True(result.IsSuccessful, "Test failed");
```

### 4.3 ADR 文档标记

✅ **推荐**：

```markdown
### 3. 模块通信约束

严禁行为：

- ❌ 跨模块引用 Entity / Aggregate / ValueObject **【必须架构测试覆盖】**
- ❌ 在契约中表达业务意图、决策字段 **【必须架构测试覆盖】**
```

❌ **避免**：

```markdown
### 3. 模块通信约束

严禁行为：

- ❌ 跨模块引用 Entity / Aggregate / ValueObject （缺少标记）
```

---

## 五、常见问题

### Q1: 验证脚本报告"缺少测试"，但我确实写了测试，怎么办？

**A**: 检查以下几点：

1. 测试方法是否使用了 `[Fact]` 或 `[Theory]` 属性？
2. 测试方法或 DisplayName 是否包含对应的 ADR 编号（如 `ADR-0001`）？
3. ADR 文档中是否正确标记了 **【必须架构测试覆盖】**？

### Q2: 测试失败消息太长，影响可读性？

**A**: 可以将详细的修复建议移到参考文档中，在失败消息中只保留关键信息和文档链接：

```csharp
Assert.True(result.IsSuccessful,
    $"❌ ADR-0001.1 违规: 模块 {moduleName} 依赖了 {otherModule}。\n" +
    $"详细修复指南：docs/copilot/adr-0001.prompts.md 场景 3");
```

### Q3: 某条约束无法通过静态分析验证，怎么办？

**A**: 在 ADR 文档中明确说明：

```markdown
- 此约束需要通过人工 Code Review 验证 **【待自动化测试】**
```

并在快速参考表中标记：

```markdown
| 约束 | 测试覆盖 | 备注 |
|------|---------|------|
| XXX | ⏳ 人工 Code Review | 待工具支持 |
```

### Q4: 我可以临时跳过映射验证吗？

**A**: 不建议。如果确实需要：

1. 在 PR 中明确说明原因
2. 创建后续任务补充测试
3. 在 PR 模板的"ADR-测试一致性检查"中勾选"本 PR 未修改 ADR 文档或架构测试"

### Q5: CI 中的映射验证失败了，本地却通过，怎么办？

**A**: 可能的原因：

1. 检查是否有未提交的文件
2. 确保使用的是最新的脚本版本
3. 检查换行符差异（Windows vs Linux）

---

## 六、参考资源

- [ADR-测试映射规范](../ADR-TEST-MAPPING-SPECIFICATION.md) - 完整的映射规范
- [ADR-0000: 架构测试与 CI 治理](../adr/governance/ADR-0000-architecture-tests.md) - 架构测试的总体指导
- [Copilot 治理体系](../copilot/README.md) - Copilot 的角色和使用指南
- [PR 模板](../.github/PULL_REQUEST_TEMPLATE.md) - PR 提交清单

---

## 七、获取帮助

遇到问题时：

1. **查阅文档**：先查看本指南和相关 ADR 文档
2. **询问 Copilot**：将问题和上下文粘贴给 Copilot
3. **查看示例**：参考现有的 ADR 和测试代码
4. **寻求帮助**：联系架构师或提交 Issue

---

**版本历史**：

- v1.0 (2026-01-23): 初始版本
