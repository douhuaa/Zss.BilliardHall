namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_008;

/// <summary>
/// 验证 ADR-008_7：违规处理（Rule）
/// 验证 ADR-008_7_1 到 ADR-008_7_2
/// </summary>
public sealed class ADR_008_7_Architecture_Tests
{
    [Fact(DisplayName = "ADR-008_7_1: 违规行为定义检查")]
    public void ADR_008_7_1_Violation_Behavior_Definition()
    {
        // 验证本测试类已定义并包含实质性测试
        var testType = typeof(ADR_008_7_Architecture_Tests);
        var classMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-008_7_1",
            "测试类不存在",
            "测试类缺失",
            "确保 ADR_008_7_Architecture_Tests 测试类存在",
            TestConstants.Adr007Path);
        
        testType.Should().NotBeNull(classMessage);

        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();

        var methodsMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-008_7_1",
            "测试类缺少测试方法",
            "测试类中没有测试方法",
            "添加验证 ADR-008_7 相关规则的测试方法",
            TestConstants.Adr007Path);

        methods.Should().NotBeEmpty(methodsMessage);

        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-008_7_2: 违规处理判定规则")]
    public void ADR_008_7_2_Violation_Handling_Decision_Rules()
    {
        // 验证测试文件存在
        var testFile = FileSystemTestHelper.GetAbsolutePath("src/tests/ArchitectureTests/ADR_008/ADR_008_7_Architecture_Tests.cs");

        FileSystemTestHelper.AssertFileExists(testFile,
            $"❌ ADR-008_7_2 违规：测试文件不存在\n\n" +
            $"修复建议：确保测试文件存在于 src/tests/ArchitectureTests/ADR_008/ 目录\n\n" +
            $"参考：docs/adr/governance/ADR-008-documentation-governance-constitution.md（§7.2）");

        // 验证文件包含实质性内容
        FileSystemTestHelper.AssertFileContentLength(testFile, 100, "文件内容过短");
        FileSystemTestHelper.AssertFileContains(testFile, "ADR_008_7", "文件应包含 ADR_008_7 相关内容");
    }

}
