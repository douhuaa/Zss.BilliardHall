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
    /// 验证 ADR 中的所有 Rule 都有对应的架构测试
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
        
        // 收集所有 ADR 文档中的 RuleId
        var adrFiles = Directory
            .GetFiles(adrDirectory, "ADR-*.md", SearchOption.AllDirectories)
            .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Contains("archive", StringComparison.OrdinalIgnoreCase));
        
        var rulesWithoutTests = new List<string>();
        
        // RuleId 模式：ADR-XXX_Y 或 ADR-XXX_Y_Z
        var ruleIdPattern = new Regex(@"ADR-(\d+)_(\d+)(?:_(\d+))?", RegexOptions.Compiled);
        
        foreach (var adrFile in adrFiles)
        {
            var content = File.ReadAllText(adrFile);
            var adrFileName = Path.GetFileName(adrFile);
            
            // 提取 ADR 编号（例如从 ADR-400-xxx.md 提取 400）
            var adrNumberMatch = Regex.Match(adrFileName, @"ADR-(\d+)");
            if (!adrNumberMatch.Success)
            {
                continue;
            }
            
            var adrNumber = adrNumberMatch.Groups[1].Value;
            
            // 查找该 ADR 中的所有 RuleId
            var ruleIds = ruleIdPattern.Matches(content)
                .Select(m => m.Value)
                .Distinct()
                .Where(id => id.StartsWith($"ADR-{adrNumber}_"))
                .ToList();
            
            if (ruleIds.Count == 0)
            {
                // 没有找到 Rule，可能是新 ADR 或特殊 ADR
                continue;
            }
            
            // 检查对应的测试目录
            var testDir = Path.Combine(testDirectory, $"ADR-{adrNumber}");
            
            if (!Directory.Exists(testDir))
            {
                // ADR 有 Rule 但没有测试目录
                rulesWithoutTests.Add($"ADR-{adrNumber}: 没有对应的测试目录 (包含 {ruleIds.Count} 个 Rule)");
                continue;
            }
            
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
        }
        
        rulesWithoutTests.Should().BeEmpty(
            $"所有 ADR Rule 都必须有对应的测试。未覆盖的 Rule：{Environment.NewLine}{string.Join(Environment.NewLine, rulesWithoutTests)}");
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
