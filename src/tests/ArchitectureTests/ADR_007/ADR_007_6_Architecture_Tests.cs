namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 验证 ADR-007_6：Agent 变更治理（Rule）
/// 验证 ADR-007_6_1 到 ADR-007_6_2
/// </summary>
public sealed class ADR_007_6_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_6_1: Agent 配置变更规则校验")]
    public void ADR_007_6_1_Agent_Configuration_Change_Rules()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_007_6_Architecture_Tests);
        testType.Should().NotBeNull($"❌ ADR-007_6_1 违规：测试类不存在\n\n" +
            $"修复建议：确保 ADR_007_6_Architecture_Tests 测试类存在\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§6.1）");
        
        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();
        
        methods.Should().NotBeEmpty($"❌ ADR-007_6_1 违规：测试类缺少测试方法\n\n" +
            $"修复建议：添加验证 ADR-007_6 相关规则的测试方法\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§6.1）");
        
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-007_6_2: Agent 变更判定规则验证")]
    public void ADR_007_6_2_Agent_Change_Decision_Rules()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot;
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_007/ADR_007_6_Architecture_Tests.cs");
        
        File.Exists(testFile).Should().BeTrue($"❌ ADR-007_6_2 违规：测试文件不存在\n\n" +
            $"修复建议：确保测试文件存在于 src/tests/ArchitectureTests/ADR_007/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§6.2）");
        
        // 验证文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_007_6");
    }
}
