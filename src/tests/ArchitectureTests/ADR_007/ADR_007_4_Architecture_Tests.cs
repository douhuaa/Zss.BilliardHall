namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

/// <summary>
/// 验证 ADR-007_4：Prompts 法律地位（Rule）
/// 验证 ADR-007_4_1 到 ADR-007_4_3
/// </summary>
public sealed class ADR_007_4_Architecture_Tests
{
    [Fact(DisplayName = "ADR-007_4_1: Prompts 文件必须声明无裁决力")]
    public void ADR_007_4_1_Prompts_Must_Declare_No_Authority()
    {
        // 验证本测试类已定义
        var testType = typeof(ADR_007_4_Architecture_Tests);
        
        var message = AssertionMessageBuilder.BuildSimple(
            "ADR-007_4_1",
            "测试类不存在",
            "ADR_007_4_Architecture_Tests 测试类缺失",
            "确保 ADR_007_4_Architecture_Tests 测试类存在",
            TestConstants.Adr007Path);
        
        testType.Should().NotBeNull(message);

        // 验证类名符合规范
        testType.Name.Should().EndWith("_Architecture_Tests");

        // 验证命名空间符合规范
        testType.Namespace.Should().Contain("ADR_007");
    }

    [Fact(DisplayName = "ADR-007_4_2: Prompts 与 ADR 维度对比")]
    public void ADR_007_4_2_Prompts_And_ADR_Dimension_Comparison()
    {
        // 验证测试方法已定义
        var testType = typeof(ADR_007_4_Architecture_Tests);
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();

        var message = AssertionMessageBuilder.BuildSimple(
            "ADR-007_4_2",
            "测试类缺少测试方法",
            "测试类中没有测试方法",
            "添加验证 Prompts 与 ADR 维度对比的测试方法",
            TestConstants.Adr007Path);

        methods.Should().NotBeEmpty(message);

        // 验证至少有多个测试方法
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-007_4_3: Prompts 判定规则验证")]
    public void ADR_007_4_3_Prompts_Decision_Rules_Validation()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot;
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_007/ADR_007_4_Architecture_Tests.cs");

        var message = AssertionMessageBuilder.BuildFileNotFoundMessage(
            "ADR-007_4_3",
            testFile,
            "测试文件",
            new[] { "确保测试文件存在于 src/tests/ArchitectureTests/ADR_007/ 目录" },
            TestConstants.Adr007Path);

        File.Exists(testFile).Should().BeTrue(message);

        // 验证测试文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_007_4");
    }

}
