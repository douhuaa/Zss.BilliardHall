# 架构测试重构快速参考

> **配套文档**：[架构测试编写指南](./ARCHITECTURE-TEST-GUIDELINES.md)  
> **用途**：提供快速查阅的重构模式和代码片段

## 快速链接

- [删除重复的 FindRepositoryRoot](#删除重复的-findrepositoryroot)
- [标准化测试类结构](#标准化测试类结构)
- [统一断言消息格式](#统一断言消息格式)
- [常用代码片段](#常用代码片段)

---

## 删除重复的 FindRepositoryRoot

### 需要重构的文件识别

运行以下命令找出需要重构的文件：

```bash
# 查找所有包含 FindRepositoryRoot 的测试文件
grep -r "private static string? FindRepositoryRoot" src/tests/ArchitectureTests --include="*.cs" -l

# 统计数量
grep -r "private static string? FindRepositoryRoot" src/tests/ArchitectureTests --include="*.cs" -l | wc -l
```

### 重构步骤

对于每个包含 `FindRepositoryRoot` 的文件：

1. **添加 using 语句**（如果还没有）：
```csharp
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
```

2. **删除整个 FindRepositoryRoot 方法**：
```csharp
// 删除这个方法块（通常 20+ 行）
private static string? FindRepositoryRoot()
{
    // ... 删除全部内容
}
```

3. **替换所有调用**：
```csharp
// 旧代码
var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");

// 新代码
var repoRoot = TestEnvironment.RepositoryRoot;
```

### 自动化重构脚本

可以使用以下 PowerShell 脚本辅助重构：

```powershell
# 查找并列出需要重构的文件
$files = Get-ChildItem -Path "src/tests/ArchitectureTests" -Recurse -Include "*.cs" | 
    Where-Object { (Get-Content $_.FullName -Raw) -match "private static string\? FindRepositoryRoot" }

foreach ($file in $files) {
    Write-Host "需要重构: $($file.FullName)"
}

Write-Host "`n总计: $($files.Count) 个文件需要重构"
```

---

## 标准化测试类结构

### 检查清单

使用此清单逐项检查每个测试类：

```
测试类: ADR_XXX_Y_Architecture_Tests.cs
├─ [ ] 类声明
│  ├─ [ ] 使用 sealed 关键字
│  ├─ [ ] 类名格式: ADR_<编号>_<Rule序号>_Architecture_Tests
│  └─ [ ] 命名空间格式: Zss.BilliardHall.Tests.ArchitectureTests.ADR_XXX
│
├─ [ ] 类文档注释
│  ├─ [ ] 包含 Rule 标题
│  ├─ [ ] 列出所有 Clause 映射
│  └─ [ ] 包含 ADR 文档路径
│
├─ [ ] Using 语句
│  ├─ [ ] using FluentAssertions;
│  └─ [ ] using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
│
└─ [ ] 测试方法
   ├─ [ ] 方法名格式: ADR_XXX_Y_Z_<描述>
   ├─ [ ] DisplayName 格式: "ADR-XXX_Y_Z: <中文描述>"
   ├─ [ ] 包含方法文档注释
   └─ [ ] 使用标准断言格式
```

### 重构模板

#### Before（需要重构）

```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_960;

public class ADR_960_Tests  // ❌ 缺少 sealed
{
    [Fact(DisplayName = "ADR-960-1-1 测试")]  // ❌ 格式不规范
    public void test_something()  // ❌ 命名不规范，缺少文档注释
    {
        var root = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        // ...
    }
    
    private static string? FindRepositoryRoot() { /* ... */ }  // ❌ 重复代码
}
```

#### After（已重构）

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
        // ...
    }
}
```

---

## 统一断言消息格式

### 标准格式模板

```csharp
<对象>.Should().<断言方法>(
    $"❌ ADR-XXX_Y_Z 违规：<简短问题描述>\n\n" +
    $"当前状态：<具体违规情况>\n\n" +
    $"修复建议：\n" +
    $"1. <步骤1>\n" +
    $"2. <步骤2>\n" +
    $"3. <步骤3>\n\n" +
    $"参考：<ADR文档路径> §ADR-XXX_Y_Z");
```

### 常见断言场景

#### 1. 文件存在性检查

```csharp
File.Exists(filePath).Should().BeTrue(
    $"❌ ADR-004_1_1 违规：仓库根目录必须存在 Directory.Packages.props 文件\n\n" +
    $"预期路径：{filePath}\n\n" +
    $"修复建议：\n" +
    $"1. 在仓库根目录创建 Directory.Packages.props 文件\n" +
    $"2. 添加必需的配置节点\n" +
    $"3. 运行测试验证配置\n\n" +
    $"参考：docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_1_1");
```

#### 2. 文件内容检查

```csharp
content.Contains("ExpectedKeyword").Should().BeTrue(
    $"❌ ADR-004_1_2 违规：配置文件必须包含关键配置项\n\n" +
    $"当前状态：未找到 'ExpectedKeyword' 配置\n" +
    $"文件路径：{filePath}\n\n" +
    $"修复建议：\n" +
    $"1. 打开配置文件 {Path.GetFileName(filePath)}\n" +
    $"2. 添加 <ExpectedKeyword>true</ExpectedKeyword>\n" +
    $"3. 保存并重新运行测试\n\n" +
    $"参考：docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_1_2");
```

#### 3. 架构依赖检查（NetArchTest）

```csharp
result.IsSuccessful.Should().BeTrue(
    $"❌ ADR-002_1_1 违规：Platform 层不应依赖 Application 层\n\n" +
    $"违规类型：\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
    $"修复建议：\n" +
    $"1. 检查违规类型的依赖关系\n" +
    $"2. 移除 Platform 对 Application 的引用\n" +
    $"3. 将共享抽象提取到正确的层\n\n" +
    $"参考：docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md §ADR-002_1_1");
```

#### 4. 集合为空检查

```csharp
violations.Should().BeEmpty(
    $"❌ ADR-007_1_1 违规：以下文件违反了规则\n\n" +
    $"{string.Join("\n", violations)}\n\n" +
    $"修复建议：\n" +
    $"1. 查看上述违规列表\n" +
    $"2. 逐个修复违规项\n" +
    $"3. 重新运行测试验证\n\n" +
    $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md §ADR-007_1_1");
```

---

## 常用代码片段

### 获取仓库路径

```csharp
// ✅ 推荐
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

var repoRoot = TestEnvironment.RepositoryRoot;
var adrPath = TestEnvironment.AdrPath;
var agentPath = TestEnvironment.AgentFilesPath;
```

### 读取文件内容

```csharp
var filePath = Path.Combine(TestEnvironment.RepositoryRoot, "path", "to", "file.md");

// 检查文件是否存在
if (!File.Exists(filePath))
{
    Console.WriteLine($"⚠️ 文件不存在：{filePath}");
    return;
}

// 读取文件内容
var content = File.ReadAllText(filePath);
```

### 遍历目录文件

```csharp
var directory = Path.Combine(TestEnvironment.RepositoryRoot, "target", "directory");

if (!Directory.Exists(directory))
{
    Console.WriteLine($"⚠️ 目录不存在：{directory}");
    return;
}

var files = Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories);

foreach (var file in files)
{
    var content = File.ReadAllText(file);
    var fileName = Path.GetFileName(file);
    
    // 执行验证逻辑
}
```

### 批量验证模式

```csharp
var violations = new List<string>();

foreach (var file in files)
{
    var content = File.ReadAllText(file);
    var fileName = Path.GetFileName(file);

    if (/* 违规条件 */)
    {
        violations.Add($"  • {fileName}: {违规描述}");
    }
}

violations.Should().BeEmpty(
    $"❌ ADR-XXX_Y_Z 违规：发现以下违规项\n\n" +
    $"{string.Join("\n", violations)}\n\n" +
    $"修复建议：\n" +
    $"1. 查看违规列表\n" +
    $"2. 修复各项违规\n" +
    $"3. 重新运行测试\n\n" +
    $"参考：<ADR文档路径> §ADR-XXX_Y_Z");
```

### 条件性测试（功能未实现）

```csharp
[Fact(DisplayName = "ADR-951_1_1: 案例库目录结构必须符合规范")]
public void ADR_951_1_1_Case_Repository_Must_Have_Valid_Directory_Structure()
{
    var casesDirectory = Path.Combine(TestEnvironment.RepositoryRoot, "docs/cases");

    // 功能尚未实现，提示并跳过
    if (!Directory.Exists(casesDirectory))
    {
        Console.WriteLine("⚠️ ADR-951_1_1 提示：docs/cases/ 目录尚未创建，这是一个待实现的功能。");
        return;
    }

    // 功能已实现，执行实际验证
    var readmePath = Path.Combine(casesDirectory, "README.md");
    File.Exists(readmePath).Should().BeTrue(
        $"❌ ADR-951_1_1 违规：案例库目录必须包含 README.md\n\n" +
        $"当前状态：{casesDirectory} 存在，但缺少 README.md\n\n" +
        $"修复建议：\n" +
        $"1. 在案例库目录创建 README.md 文件\n" +
        $"2. 添加案例库说明和索引\n" +
        $"3. 参考 ADR-951 了解案例库规范\n\n" +
        $"参考：docs/adr/governance/ADR-951-case-repository-management.md §ADR-951_1_1");
}
```

---

## 重构优先级

按以下优先级进行重构：

### P0 - 立即重构（阻塞性问题）
- [ ] 删除所有重复的 `FindRepositoryRoot` 方法
- [ ] 修复测试类缺少 `sealed` 关键字

### P1 - 高优先级（影响可维护性）
- [ ] 统一所有测试类和方法的命名格式
- [ ] 标准化所有 DisplayName 格式
- [ ] 添加缺失的 XML 文档注释

### P2 - 中优先级（改进质量）
- [ ] 统一断言消息格式
- [ ] 添加详细的修复建议
- [ ] 补充 ADR 文档引用

### P3 - 低优先级（锦上添花）
- [ ] 优化测试性能（避免重复文件读取）
- [ ] 添加更多辅助方法到 TestEnvironment
- [ ] 补充代码注释

---

## 验证重构结果

### 自动化检查

重构完成后，运行以下检查：

```bash
# 1. 确认没有重复的 FindRepositoryRoot
grep -r "private static string? FindRepositoryRoot" src/tests/ArchitectureTests --include="*.cs" -l | wc -l
# 预期输出：0

# 2. 确认所有测试类都使用 sealed
find src/tests/ArchitectureTests -name "*_Architecture_Tests.cs" -exec grep -L "sealed class" {} \;
# 预期输出：空（没有遗漏）

# 3. 运行所有架构测试
dotnet test src/tests/ArchitectureTests
# 预期：所有测试通过
```

### 手动审查

使用"迁移清单"（见主指南文档）逐项检查重构的测试类。

---

## 获取帮助

- **主指南文档**：[ARCHITECTURE-TEST-GUIDELINES.md](./ARCHITECTURE-TEST-GUIDELINES.md)
- **问题反馈**：通过 Issue 提交
- **改进建议**：通过 PR 提交

---

最后更新：2026-02-05
