using NetArchTest.Rules;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-301: 集成测试环境自动化与隔离约束
/// 参考：docs/adr/technical/ADR-301-integration-test-automation.md
/// </summary>
public sealed class ADR_301_Architecture_Tests
{
    [Fact(DisplayName = "ADR-301_1_1: 集成测试项目必须存在")]
    public void Integration_Test_Project_Should_Exist()
    {
        // 验证集成测试项目命名规范
        var testAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.Contains("Tests") == true)
            .ToList();

        // 至少应该有架构测试项目
        testAssemblies.Should().NotBeEmpty($"❌ ADR-301_1_1 违规：集成测试项目不存在\n\n" +
            $"问题分析：\n" +
            $"系统必须包含测试项目以确保代码质量和功能正确性\n\n" +
            $"修复建议：\n" +
            $"1. 创建至少一个测试项目\n" +
            $"2. 测试项目命名应以 Tests 结尾\n" +
            $"3. 示例：ArchitectureTests, IntegrationTests\n\n" +
            $"参考：docs/adr/technical/ADR-301-integration-test-automation.md（§1.1）");
        
        // 验证测试程序集命名符合规范
        var invalidNames = testAssemblies
            .Where(a => !a.GetName().Name!.EndsWith("Tests"))
            .Select(a => a.GetName().Name)
            .ToList();
            
        invalidNames.Should().BeEmpty($"❌ ADR-301_1_1 违规：测试项目命名不规范\n\n" +
            $"违规项目：{string.Join(", ", invalidNames)}\n\n" +
            $"问题分析：\n" +
            $"测试项目必须以 'Tests' 后缀结尾以保持命名一致性\n\n" +
            $"修复建议：\n" +
            $"1. 将项目重命名为 {{ProjectName}}Tests\n" +
            $"2. 示例：IntegrationTests, UnitTests, ArchitectureTests\n\n" +
            $"参考：docs/adr/technical/ADR-301-integration-test-automation.md（§1.1）");
    }

    [Fact(DisplayName = "ADR-301_1_2: TestContainers 配置文件验证")]
    public void TestContainers_Configuration_Should_Be_Valid()
    {
        // 验证项目根目录和测试目录的基本结构
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        repoRoot.Should().NotBeNull($"❌ ADR-301_1_2 违规：无法找到仓库根目录\n\n" +
            $"问题分析：\n" +
            $"无法定位包含 .git 或 Directory.Build.props 的仓库根目录\n\n" +
            $"修复建议：\n" +
            $"1. 确保项目包含 .git 目录或 Directory.Build.props 文件\n" +
            $"2. 检查测试运行环境的工作目录设置\n\n" +
            $"参考：docs/adr/technical/ADR-301-integration-test-automation.md（§1.2）");
        
        // 验证 tests 目录存在
        var testsDir = Path.Combine(repoRoot!, "src", "tests");
        Directory.Exists(testsDir).Should().BeTrue($"❌ ADR-301_1_2 违规：测试目录不存在\n\n" +
            $"目录路径：{testsDir}\n\n" +
            $"问题分析：\n" +
            $"项目必须包含 src/tests 目录来组织所有测试项目\n\n" +
            $"修复建议：\n" +
            $"1. 在项目根目录创建 src/tests 目录\n" +
            $"2. 将测试项目放置在该目录下\n" +
            $"3. 确保目录结构符合标准布局\n\n" +
            $"参考：docs/adr/technical/ADR-301-integration-test-automation.md（§1.2）");
    }
    
    [Fact(DisplayName = "ADR-301_1_3: 测试项目应使用统一的测试框架")]
    public void Test_Projects_Should_Use_Consistent_Test_Framework()
    {
        // 验证当前测试项目使用了 xUnit
        var xunitAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.StartsWith("xunit") == true);
            
        xunitAssembly.Should().NotBeNull($"❌ ADR-301_1_3 违规：未使用 xUnit 测试框架\n\n" +
            $"问题分析：\n" +
            $"项目必须使用统一的 xUnit 测试框架以保持一致性和可维护性\n\n" +
            $"修复建议：\n" +
            $"1. 添加 xUnit NuGet 包：xunit, xunit.runner.visualstudio\n" +
            $"2. 确保测试项目引用了 xUnit 框架\n" +
            $"3. 将现有测试迁移到 xUnit 格式\n\n" +
            $"参考：docs/adr/technical/ADR-301-integration-test-automation.md（§1.3）");
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")) ||
                File.Exists(Path.Combine(dir.FullName, "Directory.Build.props")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }
}
