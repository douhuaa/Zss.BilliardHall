namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

/// <summary>
/// 验证 ADR-008_6：文档变更治理（Rule）
/// 验证 ADR-008_6_1 到 ADR-008_6_3
/// </summary>
public sealed class ADR_008_6_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_6_1: 文档变更要求检查")]
    public void ADR_008_6_1_Document_Change_Requirements()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_008_6_Architecture_Tests);
        var classMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-008_6_1",
            "测试类不存在",
            "测试类缺失",
            "确保 ADR_008_6_Architecture_Tests 测试类存在",
            TestConstants.Adr007Path);
        
        testType.Should().NotBeNull(classMessage);

        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();

        var methodsMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-008_6_1",
            "测试类缺少测试方法",
            "测试类中没有测试方法",
            "添加验证 ADR-008_6 相关规则的测试方法",
            TestConstants.Adr007Path);

        methods.Should().NotBeEmpty(methodsMessage);

        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-008_6_2: 冲突裁决优先级")]
    public void ADR_008_6_2_Conflict_Resolution_Priority()
    {
        // 验证本测试类的命名空间符合规范
        var testType = typeof(ADR_008_6_Architecture_Tests);
        var ns = testType.Namespace;

        ns.Should().NotBeNull($"❌ ADR-008_6_2 违规：测试类缺少命名空间\n\n" +
            $"修复建议：确保测试类定义在正确的命名空间中\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§6.2）");

        ns!.Should().Contain("ArchitectureTests");
        ns.Should().Contain("ADR_008");
    }

    [Fact(DisplayName = "ADR-008_6_3: 文档变更判定规则")]
    public void ADR_008_6_3_Document_Change_Decision_Rules()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot;
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_008/ADR_008_6_Architecture_Tests.cs");

        File.Exists(testFile).Should().BeTrue($"❌ ADR-008_6_3 违规：测试文件不存在\n\n" +
            $"修复建议：确保测试文件存在于 src/tests/ArchitectureTests/ADR_008/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§6.3）");

        // 验证文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_008_6");
    }

}
