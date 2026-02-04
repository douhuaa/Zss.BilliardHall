using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit.Abstractions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_400;

/// <summary>
/// ADR-400_4: CI/CD 集成与 Guardian 执法
/// 验证架构测试与 CI/CD 的集成和 Guardian 执法机制
/// 
/// 测试覆盖映射：
/// - ADR-400_4_1: 架构测试必须集成到 CI/CD
/// - ADR-400_4_2: Guardian 必须验证 Rule-Test 映射
/// 
/// 关联文档：
/// - ADR: docs/adr/technical/ADR-400-testing-engineering-standards.md
/// </summary>
public sealed class ADR_400_4_Architecture_Tests
{
    private const string CiWorkflowPath = ".github/workflows";
    private const string ArchitectureTestsPath = "src/tests/ArchitectureTests";
    
    private readonly ITestOutputHelper _output;

    public ADR_400_4_Architecture_Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// ADR-400_4_1: 架构测试必须集成到 CI/CD
    /// 验证 CI 配置中包含架构测试执行步骤
    /// </summary>
    [Fact(DisplayName = "ADR-400_4_1: 架构测试必须集成到 CI/CD")]
    public void ADR_400_4_1_Architecture_Tests_Must_Be_Integrated_In_CI()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var workflowDirectory = Path.Combine(repoRoot, CiWorkflowPath);
        
        Directory.Exists(workflowDirectory).Should().BeTrue(
            $"CI 工作流目录必须存在: {workflowDirectory}");
        
        var workflowFiles = Directory.GetFiles(workflowDirectory, "*.yml", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(workflowDirectory, "*.yaml", SearchOption.AllDirectories))
            .ToList();
        
        workflowFiles.Should().NotBeEmpty("必须存在至少一个 CI 工作流配置文件");
        
        // 检查是否有工作流包含架构测试
        var hasArchitectureTestWorkflow = false;
        var workflowsWithTests = new List<string>();
        
        foreach (var file in workflowFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);
            
            // 检查工作流是否执行架构测试
            var hasArchitectureTestStep = 
                content.Contains("ArchitectureTests", StringComparison.OrdinalIgnoreCase) ||
                (content.Contains("dotnet test", StringComparison.OrdinalIgnoreCase) &&
                 content.Contains("Architecture", StringComparison.OrdinalIgnoreCase));
            
            if (hasArchitectureTestStep)
            {
                hasArchitectureTestWorkflow = true;
                workflowsWithTests.Add(fileName);
                _output.WriteLine($"✓ 找到架构测试工作流: {fileName}");
            }
        }
        
        hasArchitectureTestWorkflow.Should().BeTrue(
            $"CI 配置中必须包含架构测试执行步骤。已检查的工作流文件：{string.Join(", ", workflowFiles.Select(Path.GetFileName))}");
        
        _output.WriteLine($"包含架构测试的工作流: {string.Join(", ", workflowsWithTests)}");
    }
    
    /// <summary>
    /// 验证 CI 工作流在架构测试失败时会阻断构建
    /// </summary>
    [Fact(DisplayName = "ADR-400_4_1: CI 必须在架构测试失败时阻断构建")]
    public void ADR_400_4_1_CI_Must_Fail_On_Architecture_Test_Failure()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var workflowDirectory = Path.Combine(repoRoot, CiWorkflowPath);
        
        var workflowFiles = Directory.GetFiles(workflowDirectory, "*.yml", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(workflowDirectory, "*.yaml", SearchOption.AllDirectories))
            .ToList();
        
        var violations = new List<string>();
        
        foreach (var file in workflowFiles)
        {
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);
            
            // 检查是否执行架构测试
            if (!content.Contains("ArchitectureTests", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            // 检查是否使用了 continue-on-error: true（这会导致测试失败不阻断构建）
            var lines = content.Split('\n');
            var inArchitectureTestStep = false;
            
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // 检测架构测试步骤
                if (line.Contains("ArchitectureTests", StringComparison.OrdinalIgnoreCase) &&
                    (line.Contains("name:", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("run:", StringComparison.OrdinalIgnoreCase)))
                {
                    inArchitectureTestStep = true;
                }
                
                // 如果在架构测试步骤中发现 continue-on-error: true
                if (inArchitectureTestStep && 
                    line.Contains("continue-on-error:", StringComparison.OrdinalIgnoreCase) &&
                    line.Contains("true", StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"{fileName} - 架构测试步骤不应该使用 'continue-on-error: true'，测试失败必须阻断构建");
                }
                
                // 检测下一个步骤开始（新的 name: 或 run:）
                if (inArchitectureTestStep && 
                    line.TrimStart().StartsWith("- name:", StringComparison.OrdinalIgnoreCase))
                {
                    inArchitectureTestStep = false;
                }
            }
        }
        
        violations.Should().BeEmpty(
            $"架构测试失败必须阻断 CI 构建。违规：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
    }
    
    /// <summary>
    /// ADR-400_4_2: Guardian 必须验证 Rule-Test 映射
    /// 验证存在验证 Rule-Test 映射的测试
    /// （本测试本身就是 Guardian 的一部分）
    /// </summary>
    [Fact(DisplayName = "ADR-400_4_2: Guardian 必须验证 Rule-Test 映射")]
    public void ADR_400_4_2_Guardian_Must_Verify_Rule_Test_Mapping()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        Directory.Exists(testDirectory).Should().BeTrue(
            $"架构测试目录必须存在: {testDirectory}");
        
        // 检查是否存在 ADR-400_2 测试（Rule-Test 映射验证）
        var adr400Directory = Path.Combine(testDirectory, "ADR-400");
        Directory.Exists(adr400Directory).Should().BeTrue(
            "必须存在 ADR-400 测试目录来验证测试工程规范");
        
        var testFiles = Directory.GetFiles(adr400Directory, "ADR_400_2_*.cs", SearchOption.TopDirectoryOnly);
        testFiles.Should().NotBeEmpty(
            "必须存在 ADR_400_2 测试类来验证 Rule-Test 映射");
        
        // 验证测试文件内容包含 Rule-Test 映射检查
        var foundMappingTest = false;
        
        foreach (var file in testFiles)
        {
            var content = File.ReadAllText(file);
            
            // 检查是否包含 Rule-Test 映射相关的测试逻辑
            if (content.Contains("Every_Rule_Must_Have_At_Least_One_Test") ||
                content.Contains("RuleId"))
            {
                foundMappingTest = true;
                _output.WriteLine($"✓ 找到 Rule-Test 映射验证测试: {Path.GetFileName(file)}");
            }
        }
        
        foundMappingTest.Should().BeTrue(
            "必须存在验证 Rule-Test 映射的测试方法（Guardian 执法机制）");
    }
    
    /// <summary>
    /// 验证架构测试禁止使用 Skip 属性
    /// </summary>
    [Fact(DisplayName = "ADR-400_P_3: 禁止跳过架构测试")]
    public void ADR_400_P_3_Architecture_Tests_Must_Not_Be_Skipped()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根目录");
        var testDirectory = Path.Combine(repoRoot, ArchitectureTestsPath);
        
        if (!Directory.Exists(testDirectory))
        {
            true.Should().BeFalse($"未找到架构测试目录：{testDirectory}");
        }
        
        var testFiles = Directory.GetFiles(testDirectory, "*.cs", SearchOption.AllDirectories);
        var violations = new List<string>();
        
        // 检测 [Fact(Skip = ...)] 或 [Theory(Skip = ...)]
        var skipPattern = new Regex(@"\[(Fact|Theory)\s*\(\s*Skip\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        foreach (var file in testFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(testDirectory, file);
            
            if (skipPattern.IsMatch(content))
            {
                // 统计跳过的测试数量
                var matches = skipPattern.Matches(content);
                violations.Add($"{relativePath} - 包含 {matches.Count} 个被跳过的测试 (使用 Skip 属性)");
            }
        }
        
        violations.Should().BeEmpty(
            $"架构测试禁止使用 Skip 属性跳过测试。违规：{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
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
