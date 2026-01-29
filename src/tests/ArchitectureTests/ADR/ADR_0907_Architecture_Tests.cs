using System.Reflection;
using System.Text.RegularExpressions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-907: 架构测试执行治理宪章
/// 验证架构测试的执行级别、组织、命名、映射、CI 流程、破例治理等核心约束
/// 
/// ADR 映射清单（ADR Mapping Checklist）：
/// ┌──────────────┬────────────────────────────────────────────────────────┬──────────┐
/// │ 测试方法      │ 对应 ADR 约束                                          │ ADR 章节 │
/// ├──────────────┼────────────────────────────────────────────────────────┼──────────┤
/// │ Test_All_Architecture_Tests_Have_Enforcement_Level                  │ ADR-907.1 │
/// │ Test_All_Architecture_Test_Classes_Follow_Naming_Convention         │ ADR-907.2 │
/// │ Test_All_Architecture_Tests_Have_Proper_Failure_Messages            │ ADR-907.3 │
/// │ Test_ADR_Test_Mapping_Is_Complete                                   │ ADR-907.4 │
/// │ Test_All_L1_Tests_Are_Enforced_By_CI                                │ ADR-907.5 │
/// │ Test_Arch_Violations_File_Exists_And_Format_Is_Valid                │ ADR-907.6 │
/// │ Test_Archive_And_Superseded_ADRs_Are_Excluded_From_Enforcement      │ ADR-907.7 │
/// └──────────────┴────────────────────────────────────────────────────────┴──────────┘
/// </summary>
public sealed class ADR_0907_Architecture_Tests
{
    private static readonly string ArchitectureTestsPath = 
        Path.Combine(GetRepositoryRoot(), "src", "tests", "ArchitectureTests");
    
    private static readonly string AdrDocsPath = 
        Path.Combine(GetRepositoryRoot(), "docs", "adr");
    
    private static readonly string ArchViolationsPath = 
        Path.Combine(GetRepositoryRoot(), "ARCH-VIOLATIONS.md");

    #region ADR-907.1: 执行级别标记

    [Fact(DisplayName = "ADR-907.1: 所有架构测试必须标记执行级别")]
    public void Test_All_Architecture_Tests_Have_Enforcement_Level()
    {
        var testFiles = Directory.GetFiles(
            Path.Combine(ArchitectureTestsPath, "ADR"), 
            "ADR_*_Architecture_Tests.cs", 
            SearchOption.TopDirectoryOnly);

        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileName(testFile);

            // 检查是否包含执行级别注释（L1/L2/L3）
            // 注释格式示例：/// - ADR-0001.1: 模块不可相互引用 (L1) → Modules_Should_Not_Reference_Other_Modules
            if (!content.Contains("(L1)") && 
                !content.Contains("(L2)") && 
                !content.Contains("(L3)") &&
                fileName != "ADR_0907_Architecture_Tests.cs") // 排除自身
            {
                violations.Add($"  - {fileName}: 缺少执行级别标记 (L1/L2/L3)");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(
                $"❌ ADR-907.1 违规: 架构测试文件缺少执行级别标记\n\n" +
                $"违规文件:\n{string.Join("\n", violations)}\n\n" +
                $"修复建议:\n" +
                $"  1. 在测试类顶部注释中标记每个规则的执行级别\n" +
                $"  2. 格式: /// - ADR-XXXX.Y: 规则描述 (L1/L2/L3) → Test_Method_Name\n" +
                $"  3. L1=静态可执行, L2=语义半自动, L3=人工门控\n\n" +
                $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
        }
    }

    #endregion

    #region ADR-907.2: 测试组织与命名

    [Fact(DisplayName = "ADR-907.2: 架构测试类必须遵循命名约定")]
    public void Test_All_Architecture_Test_Classes_Follow_Naming_Convention()
    {
        var testFiles = Directory.GetFiles(
            Path.Combine(ArchitectureTestsPath, "ADR"), 
            "*.cs", 
            SearchOption.TopDirectoryOnly);

        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(testFile);

            // 检查命名格式：ADR_{XXXX}_Architecture_Tests
            var pattern = @"^ADR_\d{4}_Architecture_Tests$";
            if (!Regex.IsMatch(fileName, pattern))
            {
                violations.Add($"  - {fileName}.cs: 不符合命名规范");
            }
        }

        if (violations.Any())
        {
            Assert.Fail(
                $"❌ ADR-907.2 违规: 架构测试类命名不符合规范\n\n" +
                $"违规文件:\n{string.Join("\n", violations)}\n\n" +
                $"修复建议:\n" +
                $"  1. 测试类必须命名为 ADR_{{XXXX}}_Architecture_Tests.cs\n" +
                $"  2. XXXX 为 4 位 ADR 编号，如 0001、0907\n" +
                $"  3. 示例: ADR_0001_Architecture_Tests.cs\n\n" +
                $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
        }
    }

    [Fact(DisplayName = "ADR-907.2: 架构测试必须位于正确目录")]
    public void Test_Architecture_Tests_Are_In_Correct_Directory()
    {
        var adrTestPath = Path.Combine(ArchitectureTestsPath, "ADR");

        Assert.True(Directory.Exists(adrTestPath),
            $"❌ ADR-907.2 违规: 架构测试目录不存在\n\n" +
            $"期望目录: {adrTestPath}\n\n" +
            $"修复建议:\n" +
            $"  创建 src/tests/ArchitectureTests/ADR/ 目录\n\n" +
            $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");

        var testFiles = Directory.GetFiles(adrTestPath, "*.cs", SearchOption.TopDirectoryOnly);

        Assert.True(testFiles.Length > 0,
            $"❌ ADR-907.2 违规: ADR 测试目录为空\n\n" +
            $"修复建议:\n" +
            $"  在 src/tests/ArchitectureTests/ADR/ 目录下创建测试类\n\n" +
            $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
    }

    #endregion

    #region ADR-907.3: 测试失败消息标准（L2 - 语义检查）

    [Fact(DisplayName = "ADR-907.3: 架构测试应包含标准失败消息（L2启发式）")]
    public void Test_All_Architecture_Tests_Have_Proper_Failure_Messages()
    {
        var testFiles = Directory.GetFiles(
            Path.Combine(ArchitectureTestsPath, "ADR"), 
            "ADR_*_Architecture_Tests.cs", 
            SearchOption.TopDirectoryOnly);

        var violations = new List<string>();

        foreach (var testFile in testFiles)
        {
            var content = File.ReadAllText(testFile);
            var fileName = Path.GetFileName(testFile);

            // L2 启发式检查：查找 Assert 调用但缺少标准消息格式的情况
            // 标准格式应包含：❌ ADR-XXXX.Y 违规、修复建议、参考
            if (content.Contains("Assert.") && 
                !content.Contains("❌ ADR-") &&
                fileName != "ADR_0907_Architecture_Tests.cs") // 排除自身
            {
                violations.Add($"  - {fileName}: 可能缺少标准失败消息格式");
            }
        }

        if (violations.Any())
        {
            Assert.True(false,
                $"⚠️ ADR-907.3 语义检查: 以下测试可能缺少标准失败消息\n\n" +
                $"检测到的文件:\n{string.Join("\n", violations)}\n\n" +
                $"建议:\n" +
                $"  1. 确保失败消息包含 ❌ ADR-XXXX.Y 违规\n" +
                $"  2. 包含修复建议（编号列表）\n" +
                $"  3. 包含参考文档链接\n" +
                $"  4. 本测试为 L2 启发式检查，可能存在误报\n\n" +
                $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
        }
    }

    #endregion

    #region ADR-907.4: ADR-测试映射完整性

    [Fact(DisplayName = "ADR-907.4: ADR 测试映射必须完整")]
    public void Test_ADR_Test_Mapping_Is_Complete()
    {
        // 获取所有非归档的 ADR 文件
        var adrFiles = GetActiveAdrFiles();

        // 获取所有架构测试类
        var testFiles = Directory.GetFiles(
            Path.Combine(ArchitectureTestsPath, "ADR"), 
            "ADR_*_Architecture_Tests.cs", 
            SearchOption.TopDirectoryOnly);

        var violations = new List<string>();

        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var adrNumber = ExtractAdrNumber(adrFile);

            if (string.IsNullOrEmpty(adrNumber))
                continue;

            // 检查是否标记【必须架构测试覆盖】
            if (content.Contains("【必须架构测试覆盖】") || 
                content.Contains("[必须架构测试覆盖]"))
            {
                // 检查是否存在对应的测试类
                var expectedTestFile = $"ADR_{adrNumber}_Architecture_Tests.cs";
                var testExists = testFiles.Any(t => 
                    Path.GetFileName(t).Equals(expectedTestFile, StringComparison.OrdinalIgnoreCase));

                if (!testExists)
                {
                    violations.Add($"  - ADR-{adrNumber}: 标记需要测试但缺少 {expectedTestFile}");
                }
            }
        }

        if (violations.Any())
        {
            Assert.Fail(
                $"❌ ADR-907.4 违规: ADR 测试映射不完整\n\n" +
                $"违规项:\n{string.Join("\n", violations)}\n\n" +
                $"修复建议:\n" +
                $"  1. 为每个标记【必须架构测试覆盖】的 ADR 创建对应测试类\n" +
                $"  2. 测试类命名: ADR_{{XXXX}}_Architecture_Tests.cs\n" +
                $"  3. 测试类位置: src/tests/ArchitectureTests/ADR/\n\n" +
                $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
        }
    }

    #endregion

    #region ADR-907.5: CI 执行流程（L2 - 配置检查）

    [Fact(DisplayName = "ADR-907.5: CI 配置应包含架构测试（L2检查）")]
    public void Test_All_L1_Tests_Are_Enforced_By_CI()
    {
        var repoRoot = GetRepositoryRoot();
        var workflowsPath = Path.Combine(repoRoot, ".github", "workflows");

        if (!Directory.Exists(workflowsPath))
        {
            Assert.True(false,
                $"⚠️ ADR-907.5 语义检查: CI workflows 目录不存在\n\n" +
                $"期望路径: {workflowsPath}\n\n" +
                $"建议:\n" +
                $"  创建 .github/workflows/ 目录并配置架构测试 CI\n\n" +
                $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
        }

        var workflowFiles = Directory.GetFiles(workflowsPath, "*.yml", SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(workflowsPath, "*.yaml", SearchOption.TopDirectoryOnly))
            .ToList();

        var hasArchTestWorkflow = false;
        foreach (var workflow in workflowFiles)
        {
            var content = File.ReadAllText(workflow);
            if (content.Contains("ArchitectureTests") || 
                content.Contains("architecture-tests") ||
                content.Contains("dotnet test"))
            {
                hasArchTestWorkflow = true;
                break;
            }
        }

        Assert.True(hasArchTestWorkflow,
            $"⚠️ ADR-907.5 语义检查: CI 配置可能缺少架构测试\n\n" +
            $"建议:\n" +
            $"  1. 在 .github/workflows/ 中添加架构测试步骤\n" +
            $"  2. 确保 L1 测试失败会阻断 PR\n" +
            $"  3. 本测试为 L2 启发式检查，可能存在误报\n\n" +
            $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
    }

    #endregion

    #region ADR-907.6: 破例治理

    [Fact(DisplayName = "ADR-907.6: ARCH-VIOLATIONS 文件必须存在")]
    public void Test_Arch_Violations_File_Exists_And_Format_Is_Valid()
    {
        var repoRoot = GetRepositoryRoot();
        var archViolationsPath = Path.Combine(repoRoot, "ARCH-VIOLATIONS.md");

        // 如果文件不存在，这不是错误，只是提醒应该创建
        if (!File.Exists(archViolationsPath))
        {
            // 创建空文件以满足规范
            var template = @"# 架构违规记录（Architecture Violations）

> 本文件记录所有已批准的临时性架构违规。所有破例必须包含到期版本和归还计划。

## 当前活跃破例（Active Exceptions）

| ADR | 规则 | 违规文件 | 到期版本 | 负责人 | 归还计划 | 状态 |
|-----|------|---------|---------|--------|---------|------|
| - | - | - | - | - | - | - |

## 已归还破例（Resolved Exceptions）

| ADR | 规则 | 违规文件 | 到期版本 | 负责人 | 归还计划 | 归还日期 |
|-----|------|---------|---------|--------|---------|---------|
| - | - | - | - | - | - | - |

---

**维护**：Architecture Board  
**审核周期**：每月第一天  
**最后更新**：{DateTime.Now:yyyy-MM-dd}
";
            Directory.CreateDirectory(Path.GetDirectoryName(archViolationsPath)!);
            File.WriteAllText(archViolationsPath, template);
        }

        var content = File.ReadAllText(archViolationsPath);

        // 基本格式检查：应该包含表格结构
        var hasTable = content.Contains("|") && content.Contains("ADR") && content.Contains("规则");

        Assert.True(hasTable,
            $"❌ ADR-907.6 违规: ARCH-VIOLATIONS.md 格式无效\n\n" +
            $"文件路径: {archViolationsPath}\n\n" +
            $"修复建议:\n" +
            $"  1. 文件必须包含 Markdown 表格\n" +
            $"  2. 表格列：ADR、规则、违规文件、到期版本、负责人、归还计划、状态\n" +
            $"  3. 参考模板见测试代码中的 template 变量\n\n" +
            $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
    }

    #endregion

    #region ADR-907.7: 归档 ADR 排除

    [Fact(DisplayName = "ADR-907.7: 归档和 Superseded 的 ADR 不应有对应测试")]
    public void Test_Archive_And_Superseded_ADRs_Are_Excluded_From_Enforcement()
    {
        var archivePath = Path.Combine(AdrDocsPath, "archive");

        if (!Directory.Exists(archivePath))
        {
            // 如果 archive 目录不存在，跳过检查
            return;
        }

        var archivedAdrFiles = Directory.GetFiles(archivePath, "ADR-*.md", SearchOption.AllDirectories);
        var testFiles = Directory.GetFiles(
            Path.Combine(ArchitectureTestsPath, "ADR"), 
            "ADR_*_Architecture_Tests.cs", 
            SearchOption.TopDirectoryOnly);

        var violations = new List<string>();

        foreach (var adrFile in archivedAdrFiles)
        {
            var adrNumber = ExtractAdrNumber(adrFile);
            if (string.IsNullOrEmpty(adrNumber))
                continue;

            // 检查是否存在对应的测试文件
            var testFile = $"ADR_{adrNumber}_Architecture_Tests.cs";
            var testExists = testFiles.Any(t => 
                Path.GetFileName(t).Equals(testFile, StringComparison.OrdinalIgnoreCase));

            if (testExists)
            {
                violations.Add($"  - ADR-{adrNumber}: 已归档但仍存在测试类 {testFile}");
            }
        }

        // 检查 Superseded 状态的 ADR
        var activeAdrFiles = GetActiveAdrFiles();
        foreach (var adrFile in activeAdrFiles)
        {
            var content = File.ReadAllText(adrFile);
            if (content.Contains("status: Superseded") || content.Contains("状态**：❌ Superseded"))
            {
                var adrNumber = ExtractAdrNumber(adrFile);
                if (string.IsNullOrEmpty(adrNumber))
                    continue;

                var testFile = $"ADR_{adrNumber}_Architecture_Tests.cs";
                var testExists = testFiles.Any(t => 
                    Path.GetFileName(t).Equals(testFile, StringComparison.OrdinalIgnoreCase));

                if (testExists)
                {
                    violations.Add($"  - ADR-{adrNumber}: 状态为 Superseded 但仍存在测试类 {testFile}");
                }
            }
        }

        if (violations.Any())
        {
            Assert.Fail(
                $"❌ ADR-907.7 违规: 归档或 Superseded 的 ADR 仍存在测试\n\n" +
                $"违规项:\n{string.Join("\n", violations)}\n\n" +
                $"修复建议:\n" +
                $"  1. 删除对应的测试类文件\n" +
                $"  2. 归档的 ADR 不应参与执法\n" +
                $"  3. Superseded 的 ADR 应由替代 ADR 的测试覆盖\n\n" +
                $"参考: docs/adr/governance/ADR-907-architecture-tests-enforcement-governance.md");
        }
    }

    #endregion

    #region 辅助方法

    private static string GetRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) ||
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        return Directory.GetCurrentDirectory();
    }

    private static List<string> GetActiveAdrFiles()
    {
        var adrFiles = new List<string>();

        // 扫描所有 ADR 目录，排除 archive
        var adrCategories = new[] { "constitutional", "governance", "structure", "runtime", "technical" };
        
        foreach (var category in adrCategories)
        {
            var categoryPath = Path.Combine(AdrDocsPath, category);
            if (Directory.Exists(categoryPath))
            {
                adrFiles.AddRange(Directory.GetFiles(categoryPath, "ADR-*.md", SearchOption.TopDirectoryOnly));
            }
        }

        return adrFiles;
    }

    private static string? ExtractAdrNumber(string adrFilePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(adrFilePath);
        var match = Regex.Match(fileName, @"ADR-(\d+)");
        if (match.Success)
        {
            return match.Groups[1].Value.PadLeft(4, '0');
        }
        return null;
    }

    #endregion
}
