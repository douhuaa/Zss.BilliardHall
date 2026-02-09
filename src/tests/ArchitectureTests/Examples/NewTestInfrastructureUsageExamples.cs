using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Builders;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Extensions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Fixtures;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Examples;

/// <summary>
/// 示例：展示如何使用新的测试工具类
/// 演示 Builder、Extensions 和 Fixtures 的用法
/// </summary>
[Collection("ADR Tests")] // 使用自定义集合
public class NewTestInfrastructureUsageExamples : IClassFixture<AdrTestFixture>
{
    private readonly AdrTestFixture _adrFixture;

    public NewTestInfrastructureUsageExamples(AdrTestFixture fixture)
    {
        _adrFixture = fixture;
    }

    #region 使用 AdrTestFixture 示例

    [Fact(DisplayName = "示例：使用 AdrTestFixture 访问已加载的 ADR")]
    public void Example_UsingAdrTestFixture()
    {
        // Arrange & Act: 从 Fixture 获取数据（已缓存，无需重复加载）
        var allAdrs = _adrFixture.AllAdrs;
        var adrList = _adrFixture.AdrList;

        // Assert: 验证数据已加载
        allAdrs.Should().NotBeEmpty("应该至少有一些 ADR 文档");
        adrList.Should().NotBeEmpty();
        
        Console.WriteLine($"✅ 已加载 {allAdrs.Count} 个 ADR 文档");
    }

    [Fact(DisplayName = "示例：使用 Fixture 的辅助方法")]
    public void Example_UsingFixtureHelperMethods()
    {
        // 验证数据已加载（至少 10 个）
        _adrFixture.AssertAdrsLoaded(minimumCount: 10);

        // 获取已接受的 ADR
        var acceptedAdrs = _adrFixture.GetAcceptedAdrs();
        acceptedAdrs.Should().NotBeEmpty("应该有已接受的 ADR");

        // 按 ID 模式查找
        var governanceAdrs = _adrFixture.FindByIdPattern("900");
        
        Console.WriteLine($"✅ 找到 {acceptedAdrs.Count()} 个已接受的 ADR");
        Console.WriteLine($"✅ 找到 {governanceAdrs.Count()} 个治理相关的 ADR");
    }

    #endregion

    #region 使用 AdrDocumentBuilder 示例

    [Fact(DisplayName = "示例：使用 AdrDocumentBuilder 创建测试数据")]
    public void Example_UsingAdrDocumentBuilder()
    {
        // Arrange: 使用 Builder 创建测试数据
        var testAdr = new AdrDocumentBuilder()
            .WithId("ADR-999")
            .WithStatus("已接受")
            .WithType("adr")
            .WithLevel("governance")
            .AddDependsOn("ADR-001", "ADR-002")
            .AddSupersedes("ADR-888")
            .Build();

        // Assert: 验证创建的数据
        testAdr.Id.Should().Be("ADR-999");
        testAdr.Status.Should().Be("已接受");
        testAdr.DependsOn.Should().Contain("ADR-001");
        testAdr.Supersedes.Should().Contain("ADR-888");
        
        Console.WriteLine($"✅ 创建测试 ADR: {testAdr.Id}");
    }

    [Fact(DisplayName = "示例：快速创建多个测试 ADR")]
    public void Example_BuildingMultipleTestAdrs()
    {
        // 创建一组相关的测试 ADR
        var baseAdr = new AdrDocumentBuilder()
            .WithId("ADR-100")
            .WithStatus("已接受")
            .Build();

        var supersedingAdr = new AdrDocumentBuilder()
            .WithId("ADR-101")
            .WithStatus("已接受")
            .AddSupersedes("ADR-100")
            .Build();

        var relatedAdr = new AdrDocumentBuilder()
            .WithId("ADR-102")
            .WithStatus("待定")
            .AddDependsOn("ADR-100", "ADR-101")
            .Build();

        // 验证关系
        supersedingAdr.Supersedes.Should().Contain("ADR-100");
        relatedAdr.DependsOn.Should().Contain("ADR-100");
        
        Console.WriteLine($"✅ 创建了 3 个相关的测试 ADR");
    }

    #endregion

    #region 使用 AdrTestExtensions 示例

    [Fact(DisplayName = "示例：使用扩展方法过滤和查询 ADR")]
    public void Example_UsingAdrExtensions()
    {
        // 使用扩展方法进行链式查询
        var acceptedGovernanceAdrs = _adrFixture.AdrList
            .Accepted()                  // 只要已接受的
            .GovernanceLevel()           // 只要治理层级的
            .ToList();

        // 按编号范围过滤
        var earlyAdrs = _adrFixture.AdrList
            .InRange(1, 100)            // ADR-001 到 ADR-100
            .OnlyAdrs()                 // 只要正式 ADR
            .ToList();

        // 使用断言扩展
        _adrFixture.AdrList.AssertNotEmpty("应该有 ADR 文档");
        
        Console.WriteLine($"✅ 找到 {acceptedGovernanceAdrs.Count} 个已接受的治理 ADR");
        Console.WriteLine($"✅ 找到 {earlyAdrs.Count} 个早期 ADR（001-100）");
    }

    [Fact(DisplayName = "示例：验证 ADR 关系")]
    public void Example_VerifyingAdrRelationships()
    {
        // 找一个有关系的 ADR 进行测试
        var adrWithRelationships = _adrFixture.AdrList
            .FirstOrDefault(a => a.DependsOn.Any() || a.Supersedes.Any());

        if (adrWithRelationships != null)
        {
            // 使用扩展方法验证
            adrWithRelationships.AssertIsAdr("应该是正式 ADR");
            adrWithRelationships.AssertHasFrontMatter("应该有 Front Matter");

            // 获取所有依赖
            var allDeps = adrWithRelationships.GetAllDependencies();
            Console.WriteLine($"✅ ADR {adrWithRelationships.Id} 有 {allDeps.Count()} 个依赖");
        }
    }

    [Fact(DisplayName = "示例：组合使用多个扩展方法")]
    public void Example_CombiningExtensions()
    {
        // 复杂查询：找到所有已接受的、有依赖关系的治理 ADR
        var complexQuery = _adrFixture.AdrList
            .Accepted()
            .GovernanceLevel()
            .Where(a => a.DependsOn.Any() || a.Supersedes.Any())
            .WithIdPattern("90") // ID 包含 "90"
            .ToList();

        Console.WriteLine($"✅ 复杂查询结果: {complexQuery.Count} 个 ADR");
        
        foreach (var adr in complexQuery.Take(3))
        {
            Console.WriteLine($"   - {adr.Id} (依赖: {adr.DependsOn.Count}, 替代: {adr.Supersedes.Count})");
        }
    }

    #endregion

    #region 使用 ArchitectureRuleSetBuilder 示例

    [Fact(DisplayName = "示例：使用 ArchitectureRuleSetBuilder 创建规则集")]
    public void Example_UsingRuleSetBuilder()
    {
        // 创建一个简单的规则集
        var ruleSet = new ArchitectureRuleSetBuilder(907)
            .WithRule(1, "规则 1", DecisionLevel.Must)
            .WithClause(1, 1, "条件 1.1", "执行 1.1")
            .WithCompleteRule(2, "规则 2")
            .Build();

        // 验证
        ruleSet.AdrNumber.Should().Be(907);
        ruleSet.Rules.Should().HaveCount(2);
        
        Console.WriteLine($"✅ 创建了 ADR-907 的规则集，包含 {ruleSet.Rules.Count} 个规则");
    }

    [Fact(DisplayName = "示例：快速创建多规则集")]
    public void Example_BuildingMultipleRules()
    {
        // 快速创建多个规则
        var ruleSet = new ArchitectureRuleSetBuilder(900)
            .WithRules(1, 2, 3, 4, 5) // 快速添加 5 个规则
            .Build();

        ruleSet.Rules.Should().HaveCount(5);
        
        Console.WriteLine($"✅ 快速创建了 5 个规则");
    }

    #endregion

    #region 迁移前后对比示例

    [Fact(DisplayName = "对比：迁移前的写法")]
    public void Comparison_OldStyle()
    {
        // 旧写法：需要手动加载和过滤
        var adrs = _adrFixture.AdrList;
        var acceptedAdrs = adrs.Where(a => 
            a.Status?.Equals("已接受", StringComparison.OrdinalIgnoreCase) == true ||
            a.Status?.Equals("accepted", StringComparison.OrdinalIgnoreCase) == true);
        var governanceAdrs = acceptedAdrs.Where(a => 
            a.Level?.Equals("governance", StringComparison.OrdinalIgnoreCase) == true);

        governanceAdrs.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "对比：迁移后的写法")]
    public void Comparison_NewStyle()
    {
        // 新写法：使用扩展方法，更清晰简洁
        var governanceAdrs = _adrFixture.AdrList
            .Accepted()
            .GovernanceLevel();

        governanceAdrs.AssertNotEmpty("应该有治理层级的 ADR");
        
        Console.WriteLine("✅ 新写法更简洁、更易读");
    }

    #endregion
}
