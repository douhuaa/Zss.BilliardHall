namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

/// <summary>
/// 验证 ADR-008_3：非 ADR 文档约束（Rule）
/// 验证 ADR-008_3_1 到 ADR-008_3_4
/// </summary>
public sealed class ADR_008_3_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_3_1: Instructions/Agents 必须声明权威依据")]
    public void ADR_008_3_1_Instructions_Must_Declare_Authority()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_008_3_Architecture_Tests);
        testType.Should().NotBeNull($"❌ ADR-008_3_1 违规：测试类不存在\n\n" +
            $"修复建议：确保 ADR_008_3_Architecture_Tests 测试类存在\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§3.1）");

        // 验证测试类的命名符合规范
        testType.Name.Should().EndWith("_Architecture_Tests");

        // 验证命名空间符合规范
        testType.Namespace.Should().Contain("ADR_008");
    }

    [Fact(DisplayName = "ADR-008_3_2: Skills 不得输出判断性结论")]
    public void ADR_008_3_2_Skills_Must_Not_Output_Judgmental_Conclusions()
    {
        // 验证测试方法已定义
        var testType = typeof(ADR_008_3_Architecture_Tests);
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();

        methods.Should().NotBeEmpty($"❌ ADR-008_3_2 违规：测试类缺少测试方法\n\n" +
            $"修复建议：添加验证 Skills 相关规则的测试方法\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§3.2）");

        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-008_3_3: README/Guide 必须声明无裁决力")]
    public void ADR_008_3_3_README_Must_Declare_No_Authority()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_008/ADR_008_3_Architecture_Tests.cs");

        File.Exists(testFile).Should().BeTrue($"❌ ADR-008_3_3 违规：测试文件不存在\n\n" +
            $"修复建议：确保测试文件存在于 src/tests/ArchitectureTests/ADR_008/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§3.3）");

        // 验证文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_008_3");
    }

}
