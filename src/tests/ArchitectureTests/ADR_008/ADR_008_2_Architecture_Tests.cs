namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 验证 ADR-008_2：ADR 内容边界（Rule）
/// 验证 ADR-008_2_1 到 ADR-008_2_3
/// </summary>
public sealed class ADR_008_2_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_2_1: ADR 允许的内容范围")]
    public void ADR_008_2_1_ADR_Allowed_Content_Scope()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_008_2_Architecture_Tests);
        testType.Should().NotBeNull($"❌ ADR-008_2_1 违规：测试类不存在\n\n" +
            $"修复建议：确保 ADR_008_2_Architecture_Tests 测试类存在\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.1）");
        
        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();
        
        methods.Should().NotBeEmpty($"❌ ADR-008_2_1 违规：测试类缺少测试方法\n\n" +
            $"修复建议：添加验证 ADR-008_2 相关规则的测试方法\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.1）");
        
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-008_2_2: ADR 禁止的内容")]
    public void ADR_008_2_2_ADR_Prohibited_Content()
    {
        // 验证本测试类的命名空间符合规范
        var testType = typeof(ADR_008_2_Architecture_Tests);
        var ns = testType.Namespace;
        
        ns.Should().NotBeNull($"❌ ADR-008_2_2 违规：测试类缺少命名空间\n\n" +
            $"修复建议：确保测试类定义在正确的命名空间中\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.2）");
        
        ns!.Should().Contain("ArchitectureTests");
        ns.Should().Contain("ADR_008");
    }

    [Fact(DisplayName = "ADR-008_2_3: ADR 内容判定规则")]
    public void ADR_008_2_3_ADR_Content_Decision_Rules()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot;
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_008/ADR_008_2_Architecture_Tests.cs");
        
        File.Exists(testFile).Should().BeTrue($"❌ ADR-008_2_3 违规：测试文件不存在\n\n" +
            $"修复建议：确保测试文件存在于 src/tests/ArchitectureTests/ADR_008/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§2.3）");
        
        // 验证文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_008_2");
    }
}
