using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_400;

/// <summary>
/// ADR-400_2: Rule → Test 映射方式
/// 验证每个 ADR Rule 都有对应的测试用例
/// 
/// 测试覆盖映射：
/// - ADR-400_2_1: 每个 Rule 必须对应至少一个测试
/// - ADR-400_2_2: 测试必须包含 RuleId 引用
/// 
/// 关联文档：
/// - ADR: docs/adr/technical/ADR-400-testing-engineering-standards.md
/// </summary>
public sealed class ADR_400_2_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string ArchitectureTestsPath = "src/tests/ArchitectureTests";
    
    /// <summary>
    /// ADR-400_2_1: 每个 Rule 必须对应至少一个测试
    /// 验证 ADR-400 本身的 Rule 都有对应的架构测试
    /// （注：本测试仅验证 ADR-400 作为示范，完整的 Rule-Test 映射检查需要逐步完善）
    /// </summary>
    [Fact(DisplayName = "ADR-400_2_1: 每个 Rule 必须对应至少一个测试")]
    public void ADR_400_2_1_Every_Rule_Must_Have_At_Least_One_Test()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        if (!Directory.Exists(adrDirectory))
        {
            true.Should().BeFalse($"未找到 ADR 文档目录：{adrDirectory}");
        }
        
        if (!Directory.Exists(testDirectory))
        {
            true.Should().BeFalse($"未找到架构测试目录：{testDirectory}");
        }
        
        // 仅验证 ADR-400 本身（本 ADR 定义的规范）
        var adr400File = Path.Combine(adrDirectory, "technical", "ADR-400-testing-engineering-standards.md");
        
        if (!File.Exists(adr400File))
        {
            true.Should().BeFalse($"未找到 ADR-400 文档：{adr400File}");
        }
        
        var content = File.ReadAllText(adr400File);
        var rulesWithoutTests = new List<string>();
        
        // RuleId 模式：ADR-XXX_Y 或 ADR-XXX_Y_Z
        var ruleIdPattern = new Regex(@"ADR-400_(\d+)(?:_(\d+))?", RegexOptions.Compiled);
        
        // 查找 ADR-400 中的所有 RuleId
        var ruleIds = ruleIdPattern.Matches(content)
            .Select(m => m.Value)
            .Distinct()
            .ToList();
        
        ruleIds.Should().NotBeEmpty("ADR-400 应该包含 Rule 定义");
        
        // 检查对应的测试目录
        var testDir = Path.Combine(testDirectory, "ADR-400");
        Directory.Exists(testDir).Should().BeTrue("ADR-400 必须有对应的测试目录");
        
        // 读取所有测试文件内容
        var testFiles = Directory.GetFiles(testDir, "*.cs", SearchOption.AllDirectories);
        var allTestContent = string.Join("\n", testFiles.Select(File.ReadAllText));
        
        // 检查每个 RuleId 是否在测试中被引用
        foreach (var ruleId in ruleIds)
        {
            // 将 ADR-XXX_Y 转换为 ADR_XXX_Y（测试中的格式）
            var testRuleId = ruleId.Replace("ADR-", "ADR_");
            
            if (!allTestContent.Contains(ruleId) && !allTestContent.Contains(testRuleId))
            {
                rulesWithoutTests.Add($"{ruleId}: 在测试文件中未找到引用");
            }
        }
        
        rulesWithoutTests.Should().BeEmpty(
            $"ADR-400 的所有 Rule 都必须有对应的测试。未覆盖的 Rule：{Environment.NewLine}{string.Join(Environment.NewLine, rulesWithoutTests)}");
    }
    
    /// <summary>
    /// ADR-400_2_2: 测试必须包含 RuleId 引用
    /// 验证架构测试包含明确的 RuleId 引用
    /// </summary>
    [Fact(DisplayName = "ADR-400_2_2: 测试必须包含 RuleId 引用")]
    public void ADR_400_2_2_Tests_Must_Include_RuleId_References()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        if (!Directory.Exists(testDirectory))
        {
            true.Should().BeFalse($"未找到架构测试目录：{testDirectory}");
        }
        
        var adrDirectories = Directory.GetDirectories(testDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        var violations = new List<string>();
        
        // RuleId 引用模式：ADR-XXX_Y 或 ADR_XXX_Y
        var ruleIdPattern = new Regex(@"ADR[-_]\d+_\d+(?:_\d+)?", RegexOptions.Compiled);
        
        foreach (var adrDir in adrDirectories)
        {
            var dirName = Path.GetFileName(adrDir);
            var testFiles = Directory.GetFiles(adrDir, "*.cs", SearchOption.AllDirectories);
            
            foreach (var file in testFiles)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileName(file);
                
                // 检查文件是否包含任何 RuleId 引用
                var hasRuleIdReference = ruleIdPattern.IsMatch(content);
                
                if (!hasRuleIdReference)
                {
                    violations.Add($"{dirName}/{fileName} - 测试文件必须包含至少一个 RuleId 引用 (格式: ADR-XXX_Y_Z 或 ADR_XXX_Y_Z)");
                }
                else
                {
                    // 验证 RuleId 引用是否与测试文件所在目录一致
                    var expectedAdrNumber = dirName.Replace("ADR-", "");
                    var ruleIdMatches = ruleIdPattern.Matches(content);
                    var hasMatchingRuleId = false;
                    
                    foreach (Match match in ruleIdMatches)
                    {
                        var ruleId = match.Value.Replace("ADR-", "").Replace("ADR_", "");
                        if (ruleId.StartsWith(expectedAdrNumber + "_"))
                        {
                            hasMatchingRuleId = true;
                            break;
                        }
                    }
                    
                    if (!hasMatchingRuleId)
                    {
                        violations.Add($"{dirName}/{fileName} - 测试文件必须引用对应 ADR 的 RuleId (期望: ADR-{expectedAdrNumber}_X)");
                    }
                }
            }
        }
        
        violations.Should().BeEmpty(
            $"所有架构测试必须包含明确的 RuleId 引用。违规：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }
    
    /// <summary>
    /// 验证测试类注释包含测试覆盖映射
    /// </summary>
    [Fact(DisplayName = "ADR-400_2_2: 测试类注释必须包含测试覆盖映射")]
    public void ADR_400_2_2_Test_Classes_Must_Have_Coverage_Mapping_In_Comments()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        if (!Directory.Exists(testDirectory))
        {
            true.Should().BeFalse($"未找到架构测试目录：{testDirectory}");
        }
        
        var adrDirectories = Directory.GetDirectories(testDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        var violations = new List<string>();
        
        foreach (var adrDir in adrDirectories)
        {
            var dirName = Path.GetFileName(adrDir);
            var testFiles = Directory.GetFiles(adrDir, "*_Architecture_Tests.cs", SearchOption.AllDirectories);
            
            foreach (var file in testFiles)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileName(file);
                
                // 检查是否有 <summary> 注释
                if (!content.Contains("<summary>"))
                {
                    violations.Add($"{dirName}/{fileName} - 测试类缺少 XML 注释 <summary>");
                    continue;
                }
                
                // 检查是否包含测试覆盖映射或关联文档说明
                var hasCoverageMapping = content.Contains("测试覆盖映射") ||
                                        content.Contains("Test Coverage Mapping") ||
                                        content.Contains("关联文档") ||
                                        content.Contains("Related Documents");
                
                if (!hasCoverageMapping)
                {
                    violations.Add($"{dirName}/{fileName} - 测试类注释应该包含测试覆盖映射或关联文档说明");
                }
            }
        }
        
        violations.Should().BeEmpty(
            $"所有架构测试类注释应该包含测试覆盖映射。违规：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }
    
    private static string? FindRepositoryRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        
        while (!string.IsNullOrEmpty(currentDirectory))
        {
            if (File.Exists(Path.Combine(currentDirectory, "Zss.BilliardHall.slnx")) ||
                Directory.Exists(Path.Combine(currentDirectory, ".git")))
            {
                return currentDirectory;
            }
            
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }
        
        return null;
    }
}
