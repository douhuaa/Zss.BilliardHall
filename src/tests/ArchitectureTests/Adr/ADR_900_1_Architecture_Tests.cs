using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-900_1: ADR-架构测试映射规范（Rule）
/// 验证每个 ADR 都有对应的架构测试类
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-900_1_1: 核心 ADR 必须有对应的架构测试类
/// - ADR-900_1_2: 架构测试类必须包含实质性的测试方法
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-900-architecture-tests.md
/// </summary>
public sealed class ADR_900_1_Architecture_Tests
{
    private const string TestNamespace = "Zss.BilliardHall.Tests.ArchitectureTests.ADR";
    private const string AdrTestPattern = @"ADR_(\d{4})_Architecture_Tests";

    /// <summary>
    /// ADR-900_1_1: 核心 ADR 必须有对应的架构测试类
    /// 验证每条核心 ADR（0000-0099）都有对应的架构测试类（§ADR-900_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_1: 每条核心 ADR 必须有对应的架构测试类")]
    public void ADR_900_1_1_Core_ADRs_Must_Have_Architecture_Test_Classes()
    {
        var repoRoot = TestEnvironment.RepositoryRoot 
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
    /// ADR-900_1_2: 架构测试类必须包含实质性的测试方法
    /// 验证所有架构测试类都包含至少一个测试方法（§ADR-900_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-900_1_2: 架构测试类必须包含实质性的测试方法")]
    public void ADR_900_1_2_Architecture_Test_Classes_Must_Have_Substantial_Test_Methods()
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

}
