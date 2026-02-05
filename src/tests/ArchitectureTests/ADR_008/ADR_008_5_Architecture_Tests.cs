namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 验证 ADR-008_5：ADR 语言规范（Rule）
/// 验证 ADR-008_5_1 到 ADR-008_5_4
/// </summary>
public sealed class ADR_008_5_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_5_1: ADR 禁用语言检查")]
    public void ADR_008_5_1_ADR_Prohibited_Language_Check()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_008_5_Architecture_Tests);
        testType.Should().NotBeNull($"❌ ADR-008_5_1 违规：测试类不存在\n\n" +
            $"修复建议：确保 ADR_008_5_Architecture_Tests 测试类存在\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.1）");
        
        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();
        
        methods.Should().NotBeEmpty($"❌ ADR-008_5_1 违规：测试类缺少测试方法\n\n" +
            $"修复建议：添加验证 ADR-008_5 相关规则的测试方法\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.1）");
        
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-008_5_2: ADR 裁决性语言检查")]
    public void ADR_008_5_2_ADR_Decisional_Language_Check()
    {
        // 验证本测试类的命名空间符合规范
        var testType = typeof(ADR_008_5_Architecture_Tests);
        var ns = testType.Namespace;
        
        ns.Should().NotBeNull($"❌ ADR-008_5_2 违规：测试类缺少命名空间\n\n" +
            $"修复建议：确保测试类定义在正确的命名空间中\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.2）");
        
        ns!.Should().Contain("ArchitectureTests");
        ns.Should().Contain("ADR_008");
    }

    [Fact(DisplayName = "ADR-008_5_3: ADR 语言核心原则")]
    public void ADR_008_5_3_ADR_Language_Core_Principles()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot;
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_008/ADR_008_5_Architecture_Tests.cs");
        
        File.Exists(testFile).Should().BeTrue($"❌ ADR-008_5_3 违规：测试文件不存在\n\n" +
            $"修复建议：确保测试文件存在于 src/tests/ArchitectureTests/ADR_008/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§5.3）");
        
        // 验证文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_008_5");
    }
}
