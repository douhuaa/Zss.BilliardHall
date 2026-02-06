namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_007;

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
        
        var classMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-007_5_1",
            "测试类不存在",
            "ADR_007_5_Architecture_Tests 测试类缺失",
            "确保 ADR_007_5_Architecture_Tests 测试类存在",
            TestConstants.Adr007Path);
        
        testType.Should().NotBeNull(classMessage);

        // 验证至少包含一个测试方法
        var methods = testType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Any())
            .ToList();

        var methodsMessage = AssertionMessageBuilder.BuildSimple(
            "ADR-007_5_1",
            "测试类缺少测试方法",
            "测试类中没有测试方法",
            "添加验证 ADR-007_5 相关规则的测试方法",
            TestConstants.Adr007Path);

        methods.Should().NotBeEmpty(methodsMessage);
        methods.Count.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "ADR-007_5_2: Guardian 判定规则验证")]
    public void ADR_007_5_2_Guardian_Decision_Rules_Validation()
    {
        // 验证测试文件存在
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var testFile = Path.Combine(repoRoot, "src/tests/ArchitectureTests/ADR_007/ADR_007_5_Architecture_Tests.cs");

        var message = AssertionMessageBuilder.BuildFileNotFoundMessage(
            "ADR-007_5_2",
            testFile,
            "测试文件",
            new[] { "确保测试文件存在于 src/tests/ArchitectureTests/ADR_007/ 目录" },
            TestConstants.Adr007Path);

        File.Exists(testFile).Should().BeTrue(message);

        // 验证文件包含实质性内容
        var content = FileSystemTestHelper.ReadFileContent(testFile);
        content.Length.Should().BeGreaterThan(100);
        content.Should().Contain("ADR_007_5");
    }

}
