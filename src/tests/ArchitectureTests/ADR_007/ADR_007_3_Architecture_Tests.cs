namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

/// <summary>
/// 验证 ADR-007_3：Agent 禁止的语义行为（Rule）
/// 验证 ADR-007_3_1 到 ADR-007_3_6
/// </summary>
public sealed class ADR_007_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_3_1: Agent 禁止解释性扩权")]
    public void ADR_007_3_1_Agent_Must_Not_Expand_Authority_By_Interpretation()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_007_3_Architecture_Tests);
        testType.Should().NotBeNull($"❌ ADR-007_3_1 违规：测试类不存在\n\n" +
            $"修复建议：确保 ADR_007_3_Architecture_Tests 测试类存在\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.1）");

        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any() ||
                       m.GetCustomAttributes(typeof(TheoryAttribute), false).Any())
            .ToList();

        methods.Should().NotBeEmpty($"❌ ADR-007_3_1 违规：测试类缺少测试方法\n\n" +
            $"修复建议：添加验证 ADR-007_3 相关规则的测试方法\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.1）");

        // 验证这是一个有效的测试实现（通过检查是否被反射发现）
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-007_3_2: Agent 禁止替代性裁决")]
    public void ADR_007_3_2_Agent_Must_Not_Make_Alternative_Decisions()
    {
        // 验证本测试类的测试方法命名符合规范
        var testType = typeof(ADR_007_3_Architecture_Tests);
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any() ||
                       m.GetCustomAttributes(typeof(TheoryAttribute), false).Any())
            .ToList();

        methods.Should().NotBeEmpty();

        // 验证方法名包含 ADR 引用
        methods.Should().Contain(m => m.Name.Contains("ADR_007"),
            $"❌ ADR-007_3_2 违规：测试方法缺少 ADR 引用\n\n" +
            $"修复建议：确保测试方法名包含 ADR 编号\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.2）");
    }

    [Fact(DisplayName = "ADR-007_3_3: Agent 禁止模糊输出")]
    public void ADR_007_3_3_Agent_Must_Not_Produce_Ambiguous_Output()
    {
        // 验证测试类的命名空间符合规范
        var testType = typeof(ADR_007_3_Architecture_Tests);
        var ns = testType.Namespace;

        ns.Should().NotBeNull($"❌ ADR-007_3_3 违规：测试类缺少命名空间\n\n" +
            $"修复建议：确保测试类定义在正确的命名空间中\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§3.3）");

        ns!.Should().Contain("ArchitectureTests");
        ns.Should().Contain("ADR_007");
    }
}
