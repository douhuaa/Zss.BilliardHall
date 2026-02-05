# 架构测试编写指南（Architecture Test Guidelines）

> **文档版本**: 1.1 
> **最后更新**: 2026-02-05  
> **文档定位**: 非裁决性指导文档，提供最佳实践建议  
> **权威依据**: 本文档基于对现有 133+ 测试文件的分析，识别共性问题并提供解决方案

## 文档目的

本文档通过分析现有测试文件（`src/tests/ArchitectureTests`）中的共性问题，提供一套统一的架构测试编写规范，帮助开发者：

1. **避免代码重复**：消除重复的辅助方法（如 `FindRepositoryRoot`）
2. **保持一致性**：统一测试结构、命名和断言格式
3. **提高可维护性**：减少维护成本，便于后续扩展
4. **提升测试质量**：确保测试清晰、准确、易于理解

---

## 共性问题分析

### 问题 1：大量重复的 `FindRepositoryRoot` 方法

**现状分析**：
- 84 个测试文件中定义了相同的 `FindRepositoryRoot` 方法
- 每个方法包含 20+ 行重复代码
- 总计超过 1600 行重复代码

**问题影响**：
- ❌ 维护成本高：修改逻辑需要同步更新 84 个文件
- ❌ 代码冗余：违反 DRY（Don't Repeat Yourself）原则
- ❌ 增加出错风险：不同版本可能存在微妙差异

**解决方案**：
✅ 使用 `Shared/TestEnvironment.cs` 提供的统一实现

**反例（❌ 不推荐）**：
```csharp
public sealed class ADR_951_1_Architecture_Tests
{
    private static string? FindRepositoryRoot()
    {
        var envRoot = Environment.GetEnvironmentVariable("REPO_ROOT");
        if (!string.IsNullOrEmpty(envRoot) && Directory.Exists(envRoot))
        {
            return envRoot;
        }

        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) ||
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                File.Exists(Path.Combine(currentDir, "Zss.BilliardHall.slnx")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return null;
    }

    [Fact]
    public void Test_Something()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        // ...
    }
}
```

**正例（✅ 推荐）**：
```csharp
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

public sealed class ADR_951_1_Architecture_Tests
{
    [Fact]
    public void Test_Something()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        // TestEnvironment 会自动抛出异常如果找不到仓库根目录
        // ...
    }
}
```

**迁移建议**：
1. 删除所有本地的 `FindRepositoryRoot` 方法
2. 添加 `using Zss.BilliardHall.Tests.ArchitectureTests.Shared;`
3. 使用 `TestEnvironment.RepositoryRoot` 替代
4. 使用 `TestEnvironment.AdrPath`、`TestEnvironment.AgentFilesPath` 等预定义路径

---

### 问题 2：不一致的测试结构和命名

**现状分析**：
- 部分测试使用 `sealed class`，部分不使用
- DisplayName 格式不统一（有的包含中文冒号，有的包含英文冒号）
- 测试方法命名风格不一致

**问题影响**：
- ❌ 降低代码可读性
- ❌ 难以统一维护和升级
- ❌ 新开发者学习成本高

**解决方案**：
✅ 遵循统一的测试类结构规范

**反例（❌ 不推荐）**：
```csharp
// 缺少 sealed 关键字
public class ADR_960_Tests
{
    // 缺少文档注释
    [Fact(DisplayName = "ADR-960-1-1 Onboarding文档检查")]  // 格式不一致
    public void test_onboarding()  // 命名风格不符合规范
    {
        // ...
    }
}
```

**正例（✅ 推荐）**：
```csharp
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

/// <summary>
/// ADR-960_1: Onboarding 文档的权威定位（Rule）
/// 验证 Onboarding 文档符合非裁决性定位要求
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-960_1_1: 不是裁决性文档
/// - ADR-960_1_2: 不得定义架构约束
/// - ADR-960_1_3: 唯一合法职责
/// - ADR-960_1_4: 权威层级
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-960-onboarding-documentation-governance.md
/// </summary>
public sealed class ADR_960_1_Architecture_Tests
{
    /// <summary>
    /// ADR-960_1_1: 不是裁决性文档
    /// 验证 Onboarding 文档存在且不包含裁决性语言（§ADR-960_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-960_1_1: Onboarding 文档不得包含裁决性语言")]
    public void ADR_960_1_1_Onboarding_Must_Not_Contain_Decision_Language()
    {
        var repoRoot = TestEnvironment.RepositoryRoot;
        // 测试实现...
    }
}
```

---

### 问题 3：不一致的断言消息格式

**现状分析**：
- 279 个断言包含"修复建议"
- 断言消息格式多样（有的包含"问题分析"，有的包含"当前状态"，有的包含"执行级别"）
- 错误信息结构不统一，缺乏标准化的字段规范

**问题影响**：
- ❌ 测试失败时难以快速定位问题
- ❌ 修复建议质量参差不齐
- ❌ 不利于自动化处理失败信息
- ❌ 开发者需要理解多种不同的错误消息格式

**解决方案**：
✅ 遵循统一的断言消息格式

---

#### 标准格式规范

**必需字段**：

```
❌ ADR-XXX_Y_Z 违规：<简短问题描述>

当前状态：<具体违规情况>

修复建议：
1. <具体步骤 1>
2. <具体步骤 2>
3. <具体步骤 3>

参考：<ADR 文档路径> §ADR-XXX_Y_Z
```

**字段说明**：

1. **❌ ADR-XXX_Y_Z 违规**：（必需）
   - 必须使用 ❌ emoji 开头
   - 必须包含完整的 RuleId（格式：`ADR-XXX_Y_Z`）
   - 必须包含简短的问题描述（一句话说明违规内容）
   - 示例：`❌ ADR-002_1_1 违规：Platform 层不应依赖 Application 层`

2. **当前状态**：（必需）
   - 说明当前的具体违规情况
   - 可以包含具体的文件路径、类型名称、缺失的内容等
   - 使用具体的数据和事实，避免模糊描述
   - 示例：`当前状态：PlatformBootstrapper.cs 未 using Serilog`

3. **修复建议**：（必需）
   - 使用编号列表（1. 2. 3. ...）
   - 每个步骤应该是可操作的具体行动
   - 步骤之间应该有逻辑顺序
   - 至少包含 1 个步骤，建议 2-4 个步骤
   - 避免模糊的建议（如"修复问题"、"检查代码"）

4. **参考**：（必需）
   - 必须包含完整的 ADR 文档路径（相对于仓库根目录）
   - 必须包含章节引用（格式：`§ADR-XXX_Y_Z`）
   - 示例：`参考：docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md §ADR-002_1_1`

**可选字段**（根据具体情况决定是否包含）：

- **问题分析**：当需要解释问题背景和影响时使用
- **预期路径**：当验证文件或目录存在性时使用
- **违规类型**：当有多个违规项需要列举时使用
- **执行级别**：当需要说明测试的执行能力限制时使用（如 L1/L2/L3）

---

#### 反例（❌ 不推荐）

**反例 1：信息不完整**
```csharp
result.IsSuccessful.Should().BeTrue(
    "Platform 层不应依赖 Application 层");  // 缺少 RuleId、当前状态、修复建议、参考文档
```

**反例 2：缺少错误上下文**
```csharp
content.Should().Contain("必需内容");  // 没有说明为什么需要这个内容，如何修复
```

**反例 3：完全没有错误信息**
```csharp
File.Exists(path).Should().BeTrue();  // 测试失败时无法知道是什么文件，为什么需要它
```

**反例 4：格式不规范**
```csharp
violations.Should().BeEmpty(
    $"ADR-007_1_1 违规：以下 Agent 文件违反了定位规则\n\n" +  // 缺少 ❌ emoji
    string.Join("\n", violations) + "\n\n" +
    "修复建议：\n" +  // 没有使用编号列表
    "移除所有声称拥有裁决权的表述\n" +
    "参考 ADR-007_1_1 Agent 定位规则");  // 缺少完整的文档路径和 § 引用
```

---

#### 正例（✅ 推荐）

**正例 1：完整的标准格式**
```csharp
result.IsSuccessful.Should().BeTrue(
    $"❌ ADR-002_1_1 违规：Platform 层不应依赖 Application 层\n\n" +
    $"当前状态：\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
    $"修复建议：\n" +
    $"1. 移除 Platform 对 Application 的引用\n" +
    $"2. 将共享的技术抽象提取到 Platform 层\n" +
    $"3. 确保依赖方向正确：Host → Application → Platform\n\n" +
    $"参考：docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md §ADR-002_1_1");
```

**正例 2：包含预期路径的格式**
```csharp
File.Exists(cpmFile).Should().BeTrue(
    $"❌ ADR-004_1_1 违规：仓库根目录必须存在 Directory.Packages.props 文件以启用 Central Package Management (CPM)\n\n" +
    $"当前状态：文件不存在\n" +
    $"预期路径：{cpmFile}\n\n" +
    $"修复建议：\n" +
    $"1. 在仓库根目录创建 Directory.Packages.props 文件\n" +
    $"2. 添加 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n" +
    $"3. 添加 <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>\n\n" +
    $"参考：docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_1_1");
```

**正例 3：包含可选字段的格式**
```csharp
violations.Should().BeEmpty(
    $"❌ ADR-007_1_1 违规：以下 Agent 文件违反了定位规则\n\n" +
    $"当前状态：\n{string.Join("\n", violations)}\n\n" +
    $"修复建议：\n" +
    $"1. Agent 应定位为工具，而非决策者\n" +
    $"2. 移除所有声称拥有裁决权的表述\n" +
    $"3. 确保 Agent 配置明确引用 ADR 作为权威来源\n\n" +
    $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md §ADR-007_1_1");
```

---

### 问题 4：缺少统一的测试辅助方法

**现状分析**：
- 许多测试需要相同的功能（如文件遍历、内容检查）
- 这些功能在不同测试中重复实现
- 增加了维护难度

**解决方案**：
✅ 在 `Shared/` 目录下提供统一的辅助方法

**建议扩展 `TestEnvironment` 类**：
```csharp
public static class TestEnvironment
{
    // 现有属性
    public static string RepositoryRoot { get; }
    public static string AdrPath { get; }
    public static string AgentFilesPath { get; }
    
    // 建议新增的辅助方法
    
    /// <summary>
    /// 获取指定目录下所有 ADR 文档文件
    /// </summary>
    public static IEnumerable<string> GetAllAdrFiles(string? subfolder = null)
    {
        var path = subfolder != null 
            ? Path.Combine(AdrPath, subfolder) 
            : AdrPath;
        
        if (!Directory.Exists(path))
            return Enumerable.Empty<string>();
            
        return Directory.GetFiles(path, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取指定目录下所有 Agent 文件
    /// </summary>
    public static IEnumerable<string> GetAllAgentFiles(bool includeSystemAgents = false)
    {
        if (!Directory.Exists(AgentFilesPath))
            return Enumerable.Empty<string>();
            
        var files = Directory.GetFiles(AgentFilesPath, "*.agent.md", SearchOption.AllDirectories);
        
        if (!includeSystemAgents)
        {
            var systemAgents = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "expert-dotnet-software-engineer.agent.md",
                "README.md"
            };
            files = files.Where(f => !systemAgents.Contains(Path.GetFileName(f))).ToArray();
        }
        
        return files;
    }
}
```

---

## 标准测试类结构

### 完整模板

```csharp
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_XXX;

/// <summary>
/// ADR-XXX_Y: <Rule 标题>（Rule）
/// <简短说明：这个测试类验证什么>
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-XXX_Y_1: <Clause 1 简述>
/// - ADR-XXX_Y_2: <Clause 2 简述>
/// - ADR-XXX_Y_3: <Clause 3 简述>
///
/// 关联文档：
/// - ADR: docs/adr/<category>/ADR-XXX-<title>.md
/// </summary>
public sealed class ADR_XXX_Y_Architecture_Tests
{
    /// <summary>
    /// ADR-XXX_Y_1: <Clause 标题>
    /// <详细说明：这个测试验证什么>（§ADR-XXX_Y_1）
    /// </summary>
    [Fact(DisplayName = "ADR-XXX_Y_1: <测试显示名称>")]
    public void ADR_XXX_Y_1_<TestMethodName>()
    {
        // Arrange（准备）
        var repoRoot = TestEnvironment.RepositoryRoot;
        var targetPath = Path.Combine(repoRoot, "path/to/target");
        
        // Act（执行）
        var result = /* 执行测试操作 */;
        
        // Assert（断言）
        result.Should().BeTrue(
            $"❌ ADR-XXX_Y_1 违规：<问题简述>\n\n" +
            $"当前状态：<具体违规情况>\n\n" +
            $"修复建议：\n" +
            $"1. <步骤 1>\n" +
            $"2. <步骤 2>\n" +
            $"3. <步骤 3>\n\n" +
            $"参考：docs/adr/<category>/ADR-XXX-<title>.md §ADR-XXX_Y_1");
    }
}
```

### 命名规范

#### 测试类命名
格式：`ADR_<编号>_<Rule序号>_Architecture_Tests`

示例：
- ✅ `ADR_002_1_Architecture_Tests`
- ✅ `ADR_960_1_Architecture_Tests`
- ❌ `ADR002Tests`
- ❌ `Adr002ArchitectureTests`

#### 测试方法命名
格式：`ADR_<编号>_<Rule序号>_<Clause序号>_<描述性名称>`

示例：
- ✅ `ADR_002_1_1_Platform_Should_Not_Depend_On_Application`
- ✅ `ADR_960_1_1_Onboarding_Must_Not_Contain_Decision_Language`
- ❌ `TestPlatformDependency`
- ❌ `test_platform_deps`

#### DisplayName 格式
格式：`"ADR-<编号>_<Rule序号>_<Clause序号>: <中文描述>"`

示例：
- ✅ `"ADR-002_1_1: Platform 不应依赖 Application"`
- ✅ `"ADR-960_1_1: Onboarding 文档不得包含裁决性语言"`
- ❌ `"ADR-002.1.1 Platform Dependency Test"`
- ❌ `"测试 Platform 依赖"`

---

## 断言最佳实践

### 使用 FluentAssertions

推荐使用 FluentAssertions 而非传统的 `Assert`：

**反例（❌ 不推荐）**：
```csharp
Assert.True(result.IsSuccessful, "Platform 层不应依赖 Application 层");
Assert.NotNull(bootstrapper);
Assert.Empty(violations);
```

**正例（✅ 推荐）**：
```csharp
result.IsSuccessful.Should().BeTrue(...);    
bootstrapper.Should().NotBeNull(...);    
violations.Should().BeEmpty(...);
```

### 使用断言消息模板

为了进一步简化断言消息的编写，推荐使用 `AssertionMessageBuilder` 模板系统：

**反例（❌ 不推荐）**：手动拼接字符串
```csharp
result.IsSuccessful.Should().BeTrue(
    $"❌ ADR-002_1_1 违规：Platform 层不应依赖 Application 层\n\n" +
    $"当前状态：\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
    $"修复建议：\n" +
    $"1. 移除 Platform 对 Application 的引用\n" +
    $"2. 将共享的技术抽象提取到 Platform 层\n" +
    $"3. 确保依赖方向正确: Host → Application → Platform\n\n" +
    $"参考：docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
```

**正例（✅ 推荐）**：使用模板
```csharp
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

var message = BuildFromArchTestResult(
    ruleId: "ADR-002_1_1",
    summary: "Platform 层不应依赖 Application 层",
    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
    remediationSteps: new[]
    {
        "移除 Platform 对 Application 的引用",
        "将共享的技术抽象提取到 Platform 层",
        "确保依赖方向正确: Host → Application → Platform"
    },
    adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

result.IsSuccessful.Should().BeTrue(message);
```

**优势**：
- ✅ 代码简洁清晰，减少字符串拼接错误
- ✅ 格式自动统一，确保一致性
- ✅ 集中维护，修改一处生效全局
- ✅ 类型安全，参数明确

**详细使用指南**：请参考 [ASSERTION-MESSAGE-TEMPLATE-USAGE.md](./ASSERTION-MESSAGE-TEMPLATE-USAGE.md)

---

## 辅助工具使用

为了简化测试编写和提高代码质量，推荐使用 `Shared/` 目录下的辅助工具，避免重复实现常见功能：

### FileSystemTestHelper

提供统一的文件系统操作方法，避免直接使用原生 `File` 和 `Directory` 类：

- **文件存在性断言**：`AssertFileExists(filePath, message)` - 断言文件存在并提供详细错误信息
- **目录存在性断言**：`AssertDirectoryExists(dirPath, message)` - 断言目录存在
- **安全读取文件内容**：`ReadFileContent(filePath)` - 安全读取文件内容，自动处理异常
- **文件内容断言**：`AssertFileContains(filePath, text, message)` - 断言文件包含指定内容
- **目录文件遍历**：`GetFilesInDirectory(path, pattern, option)` - 获取目录中的文件列表
- **路径转换**：`GetAbsolutePath(relativePath)`, `GetRelativePath(absolutePath)` - 路径转换工具

**使用示例**：
```csharp
// 验证 ADR 文档存在且包含必需内容
var file = FileSystemTestHelper.GetAbsolutePath("docs/adr/ADR-XXX.md");
FileSystemTestHelper.AssertFileExists(file, "ADR 文档不存在");
FileSystemTestHelper.AssertFileContains(file, "## 决策", "应包含决策章节");
```

**详细说明**：请参考 [Shared/README.md](../src/tests/ArchitectureTests/Shared/README.md)

### TestEnvironment

提供仓库路径常量，避免重复查找：

```csharp
var repoRoot = TestEnvironment.RepositoryRoot;
var adrPath = TestEnvironment.AdrPath;
var modulesPath = TestEnvironment.ModulesPath;
```

### AssertionMessageBuilder

构建标准化断言消息（已在“使用断言消息模板”中说明）。

---

## 测试组织原则

### 按 ADR 编号组织

- 每个 ADR 的每个 Rule 对应一个测试类
- 测试类放在对应的子目录中（如 `ADR-002/`, `ADR-960/`）
- 一个 Rule 下的所有 Clause 测试都在同一个类中

**目录结构示例**：
```
src/tests/ArchitectureTests/
├─ ADR-002/
│  ├─ ADR_002_1_Architecture_Tests.cs
│  ├─ ADR_002_2_Architecture_Tests.cs
│  └─ ADR_002_3_Architecture_Tests.cs
├─ ADR-960/
│  ├─ ADR_960_1_Architecture_Tests.cs
│  ├─ ADR_960_2_Architecture_Tests.cs
│  ├─ ADR_960_3_Architecture_Tests.cs
│  └─ ADR_960_4_Architecture_Tests.cs
└─ Shared/
   ├─ TestEnvironment.cs
   ├─ TestConstants.cs
   └─ AdrTestFixture.cs
```

### 使用 `sealed` 关键字

所有测试类都应使用 `sealed` 关键字，表明这些类不应被继承：

```csharp
public sealed class ADR_002_1_Architecture_Tests
{
    // ...
}
```

---

## 迁移清单

如果你正在重构现有测试，请按以下清单逐项检查：

### 代码结构
- [ ] 类使用 `sealed` 关键字
- [ ] 类名遵循 `ADR_XXX_Y_Architecture_Tests` 格式
- [ ] 方法名遵循 `ADR_XXX_Y_Z_<Description>` 格式
- [ ] DisplayName 遵循 `"ADR-XXX_Y_Z: <中文描述>"` 格式

### 文档注释
- [ ] 类包含完整的 XML 文档注释
- [ ] 方法包含 XML 文档注释
- [ ] 注释中包含 ADR 条款引用（§ADR-XXX_Y_Z）
- [ ] 注释中包含关联 ADR 文档路径

### 依赖和导入
- [ ] 已添加 `using Zss.BilliardHall.Tests.ArchitectureTests.Shared;`
- [ ] 已删除本地的 `FindRepositoryRoot` 方法
- [ ] 使用 `TestEnvironment.RepositoryRoot` 替代本地实现
- [ ] 使用 `TestEnvironment` 提供的其他辅助属性/方法

### 断言格式（必需字段）
- [ ] 使用 FluentAssertions 风格（`.Should()` 方法）
- [ ] 断言消息以 `❌ ADR-XXX_Y_Z 违规：` 开头
- [ ] 断言消息包含"当前状态："字段
- [ ] 断言消息包含"修复建议："字段（使用编号列表：1. 2. 3.）
- [ ] 断言消息包含"参考："字段（格式：`<ADR 文档路径> §ADR-XXX_Y_Z`）
- [ ] 修复建议至少包含 1 个具体的操作步骤
- [ ] 所有步骤都是可操作的具体行动（避免模糊描述）

### 断言格式（质量检查）
- [ ] RuleId 格式正确（`ADR-XXX_Y_Z`，使用下划线而非点号）
- [ ] 简短问题描述清晰明了（一句话说明违规内容）
- [ ] 当前状态包含具体数据和事实（文件路径、类型名称等）
- [ ] 修复建议步骤之间有逻辑顺序
- [ ] ADR 文档路径完整且正确（相对于仓库根目录）
- [ ] 章节引用格式正确（使用 § 符号：`§ADR-XXX_Y_Z`）

### 提取文档路径为常量以简化代码
- [ ] 将文档路径提取为常量变量
- [ ] 简化测试方法中的文件路径引用
- [ ] 增强代码可读性和维护性

---

## 常见问题（FAQ）

### Q1：什么时候应该拆分测试类？
**A**：当一个 Rule 包含的 Clause 超过 10 个时，考虑按功能子分类拆分到多个测试类。每个测试类应该聚焦一个具体的验证主题。

### Q2：测试方法的粒度应该多细？
**A**：每个测试方法应该验证一个且仅一个 Clause。如果一个 Clause 有多个验证点，可以在同一个测试方法中使用多个断言，但它们应该都服务于同一个 Clause 的验证。

### Q3：如何处理依赖多个 ADR 的测试？
**A**：
- 如果测试主要验证 ADR-A，但需要引用 ADR-B 的定义，将测试放在 ADR-A 的测试类中
- 在文档注释中明确说明依赖的其他 ADR
- 在断言消息中引用所有相关的 ADR 文档

### Q4：测试失败时应该怎么办？
**A**：
1. 阅读完整的错误信息，特别是"修复建议"部分
2. 查看引用的 ADR 文档，理解约束的背景和目的
3. 根据修复建议调整代码
4. 重新运行测试验证
5. 如果有特殊情况需要破例，按照 ADR-900 定义的破例流程处理

---

## 结语

本指南基于对 133+ 测试文件的深入分析，总结了当前测试代码中的主要共性问题，并提供了统一的解决方案和最佳实践。

遵循本指南将有助于：
- 减少代码重复和维护成本
- 提高测试代码的一致性和可读性
- 加快新测试的编写速度
- 确保测试质量和架构约束执行效果

**重要提醒**：
- ✅ 本文档是**指导性文档**，提供最佳实践建议
- ✅ **权威依据**仍然是 ADR 文档（docs/adr/）
- ✅ 如有冲突，以 ADR 正文为准
- ✅ 本文档会随着测试实践的演进持续更新

如有问题或建议，请通过 Issue 或 PR 提出。
