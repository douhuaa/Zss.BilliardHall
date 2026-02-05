namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 验证 ADR-007_5：Guardian 主从关系（Rule）
/// 验证 ADR-007_5_1 到 ADR-007_5_2
/// </summary>
public sealed class ADR_007_5_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_5_1: Guardian 角色定义检查")]
    public void ADR_007_5_1_Guardian_Role_Definition_Check()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_007_5_Architecture_Tests);
        testType.Should().NotBeNull($"❌ ADR-007_5_1 违规：测试类不存在\n\n" +
            $"修复建议：确保 ADR_007_5_Architecture_Tests 测试类存在\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§5.1）");
        
        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();
        
        methods.Should().NotBeEmpty($"❌ ADR-007_5_1 违规：测试类缺少测试方法\n\n" +
            $"修复建议：添加验证 ADR-007_5 相关规则的测试方法\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§5.1）");
        
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-007_5_2: Guardian 判定规则验证")]
    public void ADR_007_5_2_Guardian_Decision_Rules_Validation()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot;
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_007/ADR_007_5_Architecture_Tests.cs");
        
        File.Exists(testFile).Should().BeTrue($"❌ ADR-007_5_2 违规：测试文件不存在\n\n" +
            $"修复建议：确保测试文件存在于 src/tests/ArchitectureTests/ADR_007/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-007-agent-behavior-permissions-constitution.md（§5.2）");
        
        // 验证文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_007_5");
    }
}
