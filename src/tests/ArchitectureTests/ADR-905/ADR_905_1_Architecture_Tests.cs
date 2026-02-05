using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_905;

/// <summary>
/// ADR-905_1: 执行级别分类体系
/// 验证架构规则按照 L1/L2/L3 执行级别正确分类和实施
/// 
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-905_1_1: 架构规则必须分级执行
/// - ADR-905_1_2: Level 1（L1）规则的执行标准
/// - ADR-905_1_3: Level 2（L2）规则的执行标准
/// - ADR-905_1_4: Level 3（L3）规则的执行标准
/// - ADR-905_1_5: 执行级别的分级意义
/// 
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-905-enforcement-level-classification.md
/// - Prompts: docs/copilot/adr-905.prompts.md
/// </summary>
public sealed class ADR_905_1_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const int MaxAdrFilesToCheck = 50;

    // 执行级别枚举值
    private static readonly string[] ValidEnforcementLevels = new[] { "L1", "L2", "L3" };

    /// <summary>
    /// ADR-905_1_1: 架构规则必须分级执行
    /// 验证 ADR 文档在 Enforcement 章节明确声明执行级别（§1.1）
    /// </summary>
    [Fact(DisplayName = "ADR-905_1_1: 架构规则必须分级执行")]
    public void ADR_905_1_1_Architecture_Rules_Must_Have_Enforcement_Level()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var warnings = new List<string>();
        
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        
        Directory.Exists(adrDirectory).Should().BeTrue($"未找到 ADR 文档目录：{adrDirectory}");

        // 扫描宪法层和治理层 ADR（这些 ADR 必须有 Enforcement 章节）
        var constitutionalDir = Path.Combine(adrDirectory, "constitutional");
        var governanceDir = Path.Combine(adrDirectory, "governance");
        var adrFiles = new List<string>();
        
        if (Directory.Exists(constitutionalDir))
        {
            adrFiles.AddRange(Directory
                .GetFiles(constitutionalDir, "ADR-*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase)));
        }
        
        if (Directory.Exists(governanceDir))
        {
            adrFiles.AddRange(Directory
                .GetFiles(governanceDir, "ADR-*.md", SearchOption.AllDirectories)
                .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
                .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase)));
        }

        adrFiles = adrFiles.Take(MaxAdrFilesToCheck).ToList();

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 提取 Enforcement 章节
            var enforcementSection = ExtractEnforcementSection(content);
            
            if (string.IsNullOrWhiteSpace(enforcementSection))
            {
                warnings.Add($"  ⚠️ {relativePath} - 缺少 Enforcement 章节");
                continue;
            }
            
            // 验证 Enforcement 章节包含执行级别映射表
            var hasEnforcementTable = Regex.IsMatch(enforcementSection, 
                @"\|\s*规则编号\s*\|.*执行级.*\|", RegexOptions.IgnoreCase);
            
            if (!hasEnforcementTable)
            {
                warnings.Add($"  ⚠️ {relativePath} - Enforcement 章节缺少执行级别映射表");
                continue;
            }
            
            // 验证表格中的执行级别值是否合法（L1, L2, L3）
            var enforcementLevelPattern = @"\|\s*\*\*ADR-\d+_\d+_\d+\*\*\s*\|\s*(L[123])\s*\|";
            var matches = Regex.Matches(enforcementSection, enforcementLevelPattern);
            
            foreach (Match match in matches)
            {
                var level = match.Groups[1].Value;
                if (!ValidEnforcementLevels.Contains(level, StringComparer.Ordinal))
                {
                    warnings.Add($"  ⚠️ {relativePath} - 执行级别 '{level}' 不合法，必须是 L1, L2 或 L3");
                }
            }
        }

        // L2 级别：警告但不失败构建
        if (warnings.Any())
        {
            var warningMessage = string.Join("\n", new[]
            {
                "⚠️ ADR-905_1_1 警告（L2）：以下 ADR 文档的执行级别声明不符合规范",
                "",
                "根据 ADR-905_1_1：所有架构规则必须在 Enforcement 章节明确声明执行级别（L1/L2/L3）。",
                ""
            }
            .Concat(warnings.Take(20))
            .Concat(warnings.Count > 20 ? new[] { $"  ... 还有 {warnings.Count - 20} 个警告" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "建议：",
                "  1. 确保 ADR 包含 Enforcement 章节",
                "  2. 在 Enforcement 章节中添加执行级别映射表",
                "  3. 表格格式：| 规则编号 | 执行级 | 执法方式 | Decision 映射 |",
                "  4. 执行级别必须是：L1（阻断级）、L2（警告级）或 L3（人工级）之一",
                "",
                "注意：这是 L2 警告级别，不会阻断构建。",
                "",
                "参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.1"
            }));
            
            Console.WriteLine(warningMessage);
            Console.WriteLine();
        }
        
        // L2 警告：测试总是通过，但已输出警告信息
    }

    /// <summary>
    /// ADR-905_1_2: Level 1（L1）规则的执行标准
    /// 验证标注为 L1 的规则有对应的 NetArchTest 架构测试（§1.2）
    /// </summary>
    [Fact(DisplayName = "ADR-905_1_2: Level 1（L1）规则的执行标准")]
    public void ADR_905_1_2_L1_Rules_Must_Have_Architecture_Tests()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var warnings = new List<string>();
        
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        
        Directory.Exists(adrDirectory).Should().BeTrue($"未找到 ADR 文档目录：{adrDirectory}");

        // 扫描所有 ADR，提取 L1 规则
        var adrFiles = Directory
            .GetFiles(adrDirectory, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
            .Take(MaxAdrFilesToCheck);

        var l1RulesWithoutTests = new List<string>();

        foreach (var file in adrFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file);
            
            // 提取 ADR 编号
            var adrNumber = ExtractAdrNumber(Path.GetFileNameWithoutExtension(file));
            if (string.IsNullOrEmpty(adrNumber))
                continue;
            
            // 提取 Enforcement 章节
            var enforcementSection = ExtractEnforcementSection(content);
            if (string.IsNullOrWhiteSpace(enforcementSection))
                continue;
            
            // 查找所有标记为 L1 的规则
            var l1Pattern = @"\|\s*\*\*(" + Regex.Escape(adrNumber) + @"_\d+_\d+)\*\*\s*\|\s*L1\s*\|";
            var l1Matches = Regex.Matches(enforcementSection, l1Pattern);
            
            foreach (Match match in l1Matches)
            {
                var ruleId = match.Groups[1].Value;
                
                // 检查是否存在对应的架构测试
                // 测试文件格式：src/tests/ArchitectureTests/ADR-XXX/ADR_XXX_Y_Architecture_Tests.cs
                // 或 src/tests/ArchitectureTests/ADR/ADR_XXX_Architecture_Tests.cs
                var hasTest = CheckIfTestExists(repoRoot, adrNumber, ruleId);
                
                if (!hasTest)
                {
                    l1RulesWithoutTests.Add($"  ⚠️ {ruleId} (来自 {relativePath})");
                }
            }
        }

        // L2 级别：警告但不失败构建
        if (l1RulesWithoutTests.Any())
        {
            var warningMessage = string.Join("\n", new[]
            {
                "⚠️ ADR-905_1_2 警告（L2）：以下 L1 规则缺少对应的架构测试",
                "",
                "根据 ADR-905_1_2：标注为 L1 的规则必须通过 NetArchTest 等静态分析工具实现自动化检查。",
                ""
            }
            .Concat(l1RulesWithoutTests.Take(20))
            .Concat(l1RulesWithoutTests.Count > 20 ? new[] { $"  ... 还有 {l1RulesWithoutTests.Count - 20} 个缺失" } : Array.Empty<string>())
            .Concat(new[]
            {
                "",
                "建议：",
                "  1. 为每个 L1 规则创建对应的架构测试方法",
                "  2. 测试方法命名：ADR_XXX_Y_Z_<Description>",
                "  3. 测试必须使用 NetArchTest 或类似静态分析工具",
                "  4. 测试失败应明确指出违规代码位置",
                "",
                "注意：这是 L2 警告级别，不会阻断构建。",
                "",
                "参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.2"
            }));
            
            Console.WriteLine(warningMessage);
            Console.WriteLine();
        }
        
        // L2 警告：测试总是通过，但已输出警告信息
    }

    /// <summary>
    /// ADR-905_1_3: Level 2（L2）规则的执行标准
    /// 验证 L2 规则的告警性质（§1.3）
    /// </summary>
    [Fact(DisplayName = "ADR-905_1_3: Level 2（L2）规则的执行标准")]
    public void ADR_905_1_3_L2_Rules_Should_Be_Warning_Level()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-905-enforcement-level-classification.md");
        
        File.Exists(adrFile).Should().BeTrue($"ADR-905 文档不存在：{adrFile}");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证 ADR-905 定义了 L2 规则的特征
        content.Should().Contain("Level 2（L2）",
            $"❌ ADR-905_1_3 违规：ADR-905 必须定义 Level 2（L2）规则的执行标准\n\n" +
            $"修复建议：在 Decision 章节中明确定义 L2 规则的特征和执行标准\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.3");
        
        // 验证 L2 规则的语义半自动特征
        content.Should().Contain("语义半自动",
            $"❌ ADR-905_1_3 违规：ADR-905 必须说明 L2 规则的语义半自动特征\n\n" +
            $"修复建议：明确说明 L2 规则需要语义分析，当前为启发式检查\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.3");
        
        // 验证 L2 规则可申请破例
        content.Should().Contain("可申请破例",
            $"❌ ADR-905_1_3 违规：ADR-905 必须说明 L2 规则可申请破例\n\n" +
            $"修复建议：明确说明 L2 规则测试失败可申请破例，需充分理由\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.3");
    }

    /// <summary>
    /// ADR-905_1_4: Level 3（L3）规则的执行标准
    /// 验证 L3 规则的人工审查流程存在（§1.4）
    /// </summary>
    [Fact(DisplayName = "ADR-905_1_4: Level 3（L3）规则的执行标准")]
    public void ADR_905_1_4_L3_Rules_Must_Have_Manual_Review_Process()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-905-enforcement-level-classification.md");
        
        File.Exists(adrFile).Should().BeTrue($"ADR-905 文档不存在：{adrFile}");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证 ADR-905 定义了 L3 规则的特征
        content.Should().Contain("Level 3（L3）",
            $"❌ ADR-905_1_4 违规：ADR-905 必须定义 Level 3（L3）规则的执行标准\n\n" +
            $"修复建议：在 Decision 章节中明确定义 L3 规则的特征和执行标准\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.4");
        
        // 验证 L3 规则的人工 Gate 特征
        content.Should().Contain("人工 Gate",
            $"❌ ADR-905_1_4 违规：ADR-905 必须说明 L3 规则需要人工审查\n\n" +
            $"修复建议：明确说明 L3 规则无法完全自动化，需要人工审查和决策\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.4");
        
        // 验证 L3 规则的人工审查流程
        content.Should().Contain("人工 Gate 流程",
            $"❌ ADR-905_1_4 违规：ADR-905 必须定义 L3 规则的人工审查流程\n\n" +
            $"修复建议：定义完整的人工审查流程，包括 PR 提交、CI 检查、架构审查和记录归档\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.4");
        
        // 验证破例必须记录
        content.Should().Contain("破例必须记录",
            $"❌ ADR-905_1_4 违规：ADR-905 必须要求 L3 破例记录\n\n" +
            $"修复建议：明确说明所有破例必须记录并定期审计\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.4");
    }

    /// <summary>
    /// ADR-905_1_5: 执行级别的分级意义
    /// 验证执行级别体系被正确理解和执行（§1.5）
    /// </summary>
    [Fact(DisplayName = "ADR-905_1_5: 执行级别的分级意义")]
    public void ADR_905_1_5_Enforcement_Levels_Must_Be_Understood_By_All_Roles()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrFile = Path.Combine(repoRoot, "docs/adr/governance/ADR-905-enforcement-level-classification.md");
        
        File.Exists(adrFile).Should().BeTrue($"ADR-905 文档不存在：{adrFile}");
        
        var content = File.ReadAllText(adrFile);
        
        // 验证 ADR-905 说明了对开发者的意义
        content.Should().Contain("对开发者",
            $"❌ ADR-905_1_5 违规：ADR-905 必须说明执行级别对开发者的意义\n\n" +
            $"修复建议：明确说明开发者如何理解和应对不同执行级别\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.5");
        
        // 验证 ADR-905 说明了对架构师的意义
        content.Should().Contain("对架构师",
            $"❌ ADR-905_1_5 违规：ADR-905 必须说明执行级别对架构师的意义\n\n" +
            $"修复建议：明确说明架构师如何根据执行级别进行审查和决策\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.5");
        
        // 验证 ADR-905 说明了对工具链的意义
        content.Should().Contain("对工具链",
            $"❌ ADR-905_1_5 违规：ADR-905 必须说明执行级别对工具链的意义\n\n" +
            $"修复建议：明确说明不同执行级别对应的工具链实现\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.5");
        
        // 验证提及了 NetArchTest
        content.Should().Contain("NetArchTest",
            $"❌ ADR-905_1_5 违规：ADR-905 必须提及 NetArchTest 作为 L1 工具\n\n" +
            $"修复建议：明确说明 Level 1 规则使用 NetArchTest 实现\n\n" +
            $"参考：docs/adr/governance/ADR-905-enforcement-level-classification.md §1.5");
    }

    // ========== 辅助方法 ==========


    private static string ExtractEnforcementSection(string content)
    {
        var match = Regex.Match(content, @"^##\s+Enforcement.*?\n(.*?)(?=^##\s+|\z)", 
            RegexOptions.Multiline | RegexOptions.Singleline);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ExtractAdrNumber(string fileName)
    {
        var match = Regex.Match(fileName, @"(ADR-\d{3,4})");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static bool CheckIfTestExists(string repoRoot, string adrNumber, string ruleId)
    {
        // 测试可能在两个位置：
        // 1. src/tests/ArchitectureTests/ADR-XXX/ADR_XXX_Y_Architecture_Tests.cs
        // 2. src/tests/ArchitectureTests/ADR/ADR_XXX_Architecture_Tests.cs
        
        var testDir1 = Path.Combine(repoRoot, "src/tests/ArchitectureTests", adrNumber);
        var testDir2 = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR");
        
        // 提取 Rule 编号（如 ADR-905_1）
        var ruleMatch = Regex.Match(ruleId, @"(ADR-\d+_\d+)_\d+");
        if (!ruleMatch.Success)
            return false;
        
        var rulePrefix = ruleMatch.Groups[1].Value.Replace("-", "_");
        
        // 检查新格式测试（按 Rule 组织）
        if (Directory.Exists(testDir1))
        {
            var testFiles = Directory.GetFiles(testDir1, $"{rulePrefix}_Architecture_Tests.cs", SearchOption.AllDirectories);
            if (testFiles.Any())
            {
                // 检查文件内容是否包含对应的测试方法
                foreach (var testFile in testFiles)
                {
                    var testContent = File.ReadAllText(testFile);
                    var testMethodPattern = ruleId.Replace("-", "_");
                    if (testContent.Contains(testMethodPattern))
                        return true;
                }
            }
        }
        
        // 检查旧格式测试（单文件）
        if (Directory.Exists(testDir2))
        {
            var adrTestFileName = adrNumber.Replace("-", "_") + "_Architecture_Tests.cs";
            var testFile = Path.Combine(testDir2, adrTestFileName);
            if (File.Exists(testFile))
            {
                var testContent = File.ReadAllText(testFile);
                var testMethodPattern = ruleId.Replace("-", "_");
                return testContent.Contains(testMethodPattern);
            }
        }
        
        return false;
    }
}
