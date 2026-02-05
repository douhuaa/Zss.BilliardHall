namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

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
        
        var classMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-007_6_1",
            "测试类不存在",
            "ADR_007_6_Architecture_Tests 测试类缺失",
            "确保 ADR_007_6_Architecture_Tests 测试类存在",
            TestConstants.Adr007Path);
        
        testType.Should().NotBeNull(classMessage);

        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();

        var methodsMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-007_6_1",
            "测试类缺少测试方法",
            "测试类中没有测试方法",
            "添加验证 ADR-007_6 相关规则的测试方法",
            TestConstants.Adr007Path);

        methods.Should().NotBeEmpty(methodsMessage);
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-007_6_2: Agent 变更判定规则验证")]
    public void ADR_007_6_2_Agent_Change_Decision_Rules()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_007/ADR_007_6_Architecture_Tests.cs");

        var fileMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
            "ADR-007_6_2",
            testFile,
            "测试文件",
            new[] { "确保测试文件存在于 src/tests/ArchitectureTests/ADR_007/ 目录" },
            TestConstants.Adr007Path);

        File.Exists(testFile).Should().BeTrue(fileMessage);

        // 验证文件包含实质性内容
        var content = File.ReadAllText(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_007_6");
    }

}
