using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-架构测试映射验证
/// 确保每个 ADR 都有对应的架构测试类
/// </summary>
public sealed class AdrTestMappingTests
{
    private const string TestNamespace = "Zss.BilliardHall.Tests.ArchitectureTests.ADR";
    private const string AdrTestPattern = @"ADR_(\d{4})_Architecture_Tests";

    /// <summary>
    /// ADR-0000: 每条核心 ADR（0000-0099）必须有对应的架构测试类
    /// </summary>
    [Fact(DisplayName = "ADR-0000: 每条核心 ADR 必须有对应的架构测试类")]
    public void Core_ADRs_Must_Have_Architecture_Test_Classes()
    {
        var repoRoot = FindRepositoryRoot() 
            ?? throw new InvalidOperationException("未找到仓库根目录");
        
        var adrPath = Path.Combine(repoRoot, "docs", "adr");
        
        // 获取所有核心 ADR（0000-0099）
        var coreAdrs = Directory.GetFiles(adrPath, "ADR-*.md", SearchOption.AllDirectories)
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Select(name => Regex.Match(name, @"ADR-(\d{4})"))
            .Where(m => m.Success)
            .Select(m => int.Parse(m.Groups[1].Value))
            .Where(num => num <= 99) // 核心 ADR: 0000-0099
            .OrderBy(num => num)
            .ToList();

        // 获取所有架构测试类
        var testAssembly = Assembly.GetExecutingAssembly();
        var testClasses = testAssembly.GetTypes()
            .Where(t => t.Namespace == TestNamespace)
            .Select(t => t.Name)
            .Where(name => Regex.IsMatch(name, AdrTestPattern))
            .Select(name => Regex.Match(name, AdrTestPattern))
            .Where(m => m.Success)
            .Select(m => int.Parse(m.Groups[1].Value))
            .ToHashSet();

        var violations = new List<string>();

        foreach (var adrNum in coreAdrs)
        {
            if (!testClasses.Contains(adrNum))
            {
                violations.Add(
                    $"❌ ADR-{adrNum:D4} 缺少架构测试类\n" +
                    $"   期望类名：ADR_{adrNum:D4}_Architecture_Tests\n" +
                    $"   位置：src/tests/ArchitectureTests/ADR/"
                );
            }
        }

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// 架构测试类必须包含实质性的测试方法
    /// </summary>
    [Fact(DisplayName = "架构测试类必须包含实质性的测试方法")]
    public void Architecture_Test_Classes_Must_Have_Substantial_Test_Methods()
    {
        var testAssembly = Assembly.GetExecutingAssembly();
        var violations = new List<string>();

        var testClasses = testAssembly.GetTypes()
            .Where(t => t.Namespace == TestNamespace)
            .Where(t => Regex.IsMatch(t.Name, AdrTestPattern))
            .ToList();

        foreach (var testClass in testClasses)
        {
            var testMethods = testClass.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<FactAttribute>() != null || 
                           m.GetCustomAttribute<TheoryAttribute>() != null)
                .ToList();

            if (testMethods.Count == 0)
            {
                violations.Add(
                    $"❌ {testClass.Name} 没有测试方法\n" +
                    $"   请添加带 [Fact] 或 [Theory] 特性的测试方法"
                );
            }
        }

        violations.Should().BeEmpty();
    }

    /// <summary>
    /// 查找仓库根目录
    /// </summary>
    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }
            
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        return null;
    }
}
