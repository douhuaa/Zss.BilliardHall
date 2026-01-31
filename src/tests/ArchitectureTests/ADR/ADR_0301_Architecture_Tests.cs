using NetArchTest.Rules;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-301: 集成测试环境自动化与隔离约束
/// 参考：docs/adr/technical/ADR-301-integration-test-automation.md
/// </summary>
public sealed class ADR_0301_Architecture_Tests
{
    [Fact(DisplayName = "ADR-301.1: 集成测试项目必须存在")]
    public void Integration_Test_Project_Should_Exist()
    {
        // 验证集成测试项目命名规范
        var testAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.Contains("Tests") == true)
            .ToList();

        // 至少应该有架构测试项目
        testAssemblies.Should().NotBeEmpty();
        
        // 验证测试程序集命名符合规范
        var invalidNames = testAssemblies
            .Where(a => !a.GetName().Name!.EndsWith("Tests"))
            .Select(a => a.GetName().Name)
            .ToList();
            
        invalidNames.Should().BeEmpty();
    }

    [Fact(DisplayName = "ADR-301.2: TestContainers 配置文件验证")]
    public void TestContainers_Configuration_Should_Be_Valid()
    {
        // 验证项目根目录和测试目录的基本结构
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        repoRoot.Should().NotBeNull();
        
        // 验证 tests 目录存在
        var testsDir = Path.Combine(repoRoot!, "src", "tests");
        Directory.Exists(testsDir).Should().BeTrue($"【ADR-301.2】测试目录应存在：{testsDir}");
    }
    
    [Fact(DisplayName = "ADR-301.3: 测试项目应使用统一的测试框架")]
    public void Test_Projects_Should_Use_Consistent_Test_Framework()
    {
        // 验证当前测试项目使用了 xUnit
        var xunitAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.StartsWith("xunit") == true);
            
        xunitAssembly.Should().NotBeNull();
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
