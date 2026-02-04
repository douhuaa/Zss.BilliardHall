using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_400;

/// <summary>
/// ADR-400_1: 测试代码目录与文件结构
/// 验证测试代码符合目录结构和文件命名规范
/// 
/// 测试覆盖映射：
/// - ADR-400_1_1: 测试目录结构规范
/// - ADR-400_1_2: 测试文件命名规则
/// 
/// 关联文档：
/// - ADR: docs/adr/technical/ADR-400-testing-engineering-standards.md
/// </summary>
public sealed class ADR_400_1_Architecture_Tests
{
    private const string ArchitectureTestsPath = "src/tests/ArchitectureTests";
    
    /// <summary>
    /// ADR-400_1_1: 测试目录结构规范
    /// 验证架构测试位于正确的目录结构中
    /// </summary>
    [Fact(DisplayName = "ADR-400_1_1: 测试目录结构规范")]
    public void ADR_400_1_1_Architecture_Tests_Directory_Structure_Must_Be_Correct()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        // 验证架构测试目录存在
        Directory.Exists(testDirectory).Should().BeTrue(
            $"架构测试目录必须存在: {testDirectory}");
        
        // 验证 ADR 子目录组织
        var adrDirectories = Directory.GetDirectories(testDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        adrDirectories.Should().NotBeEmpty("架构测试必须按 ADR 编号组织到子目录");
        
        // 验证每个 ADR 目录都包含测试文件
        var violations = new List<string>();
        
        foreach (var adrDir in adrDirectories)
        {
            var dirName = Path.GetFileName(adrDir);
            var testFiles = Directory.GetFiles(adrDir, "*_Architecture_Tests.cs", SearchOption.TopDirectoryOnly);
            
            if (testFiles.Length == 0)
            {
                violations.Add($"目录 {dirName} 不包含任何架构测试文件");
            }
        }
        
        violations.Should().BeEmpty(
            $"所有 ADR 目录都必须包含架构测试文件。违规：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }
    
    /// <summary>
    /// ADR-400_1_2: 测试文件命名规则
    /// 验证架构测试文件命名符合规范
    /// </summary>
    [Fact(DisplayName = "ADR-400_1_2: 测试文件命名规则")]
    public void ADR_400_1_2_Architecture_Test_Files_Must_Follow_Naming_Convention()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        if (!Directory.Exists(testDirectory))
        {
            Assert.Fail($"未找到架构测试目录：{testDirectory}");
        }
        
        // 查找所有 ADR-* 子目录中的测试文件
        var adrDirectories = Directory.GetDirectories(testDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        var violations = new List<string>();
        
        // 正确的文件命名模式：ADR_XXX_Architecture_Tests.cs 或 ADR_XXX_Y_Architecture_Tests.cs
        var correctPattern = new Regex(@"^ADR_\d+(_\d+)?_Architecture_Tests\.cs$", RegexOptions.Compiled);
        
        foreach (var adrDir in adrDirectories)
        {
            var testFiles = Directory.GetFiles(adrDir, "*.cs", SearchOption.TopDirectoryOnly);
            
            foreach (var file in testFiles)
            {
                var fileName = Path.GetFileName(file);
                
                // 检查文件名是否符合规范
                if (!correctPattern.IsMatch(fileName))
                {
                    var dirName = Path.GetFileName(adrDir);
                    violations.Add($"{dirName}/{fileName} - 文件名不符合规范 'ADR_XXX_Architecture_Tests.cs' 或 'ADR_XXX_Y_Architecture_Tests.cs'");
                }
            }
        }
        
        violations.Should().BeEmpty(
            $"所有架构测试文件必须遵循命名规范。违规：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }
    
    /// <summary>
    /// 验证架构测试类的命名空间符合规范
    /// </summary>
    [Fact(DisplayName = "ADR-400_1_2: 架构测试类命名空间必须符合规范")]
    public void ADR_400_1_2_Architecture_Test_Namespaces_Must_Follow_Convention()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        if (!Directory.Exists(testDirectory))
        {
            Assert.Fail($"未找到架构测试目录：{testDirectory}");
        }
        
        var adrDirectories = Directory.GetDirectories(testDirectory, "ADR-*", SearchOption.TopDirectoryOnly);
        var violations = new List<string>();
        
        // 正确的命名空间模式：Zss.BilliardHall.Tests.ArchitectureTests.ADR_XXX
        var namespacePattern = new Regex(
            @"namespace\s+Zss\.BilliardHall\.Tests\.ArchitectureTests\.ADR_\d+;",
            RegexOptions.Compiled | RegexOptions.Multiline);
        
        foreach (var adrDir in adrDirectories)
        {
            var dirName = Path.GetFileName(adrDir);
            var expectedAdrNumber = dirName.Replace("ADR-", "ADR_");
            var testFiles = Directory.GetFiles(adrDir, "*.cs", SearchOption.TopDirectoryOnly);
            
            foreach (var file in testFiles)
            {
                var content = File.ReadAllText(file);
                var fileName = Path.GetFileName(file);
                
                // 检查命名空间声明
                if (!namespacePattern.IsMatch(content))
                {
                    violations.Add(
                        $"{dirName}/{fileName} - 命名空间必须为 'Zss.BilliardHall.Tests.ArchitectureTests.{expectedAdrNumber}'");
                }
                else
                {
                    // 验证命名空间与目录一致
                    var expectedNamespace = $"Zss.BilliardHall.Tests.ArchitectureTests.{expectedAdrNumber}";
                    if (!content.Contains(expectedNamespace))
                    {
                        violations.Add(
                            $"{dirName}/{fileName} - 命名空间应该与目录名一致：{expectedNamespace}");
                    }
                }
            }
        }
        
        violations.Should().BeEmpty(
            $"所有架构测试类必须使用标准命名空间。违规：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
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
