using NetArchTest.Rules;
using FluentAssertions;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-123: Repository 接口与分层命名规范
/// </summary>
public sealed class ADR_123_Architecture_Tests
{
    [Theory(DisplayName = "ADR-123_1_1: Repository 接口必须位于 Domain 层")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Repository_Interfaces_Must_Be_In_Domain(Assembly moduleAssembly)
    {
        var repositoryInterfaces = Types
            .InAssembly(moduleAssembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameEndingWith("Repository")
            .GetTypes();

        foreach (var repoInterface in repositoryInterfaces)
        {
            var isInDomain = repoInterface.Namespace?.Contains(".Domain.") == true ||
                           repoInterface.Namespace?.EndsWith(".Domain") == true;

            isInDomain.Should().BeTrue($"❌ ADR-123_1_1 违规: Repository 接口不在 Domain 层\n\n" +
                $"违规接口: {repoInterface.FullName}\n\n" +
                $"修复建议:\n" +
                $"将接口移动到 {{Module}}.Domain.Repositories 命名空间\n\n" +
                $"参考: docs/copilot/adr-123.prompts.md");
        }
    }

    [Theory(DisplayName = "ADR-123_1_2: Repository 接口命名必须遵循 I{Aggregate}Repository 模式")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Repository_Interfaces_Must_Follow_Naming(Assembly moduleAssembly)
    {
        var repositoryInterfaces = Types
            .InAssembly(moduleAssembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameEndingWith("Repository")
            .GetTypes();

        foreach (var repoInterface in repositoryInterfaces)
        {
            var startsWithI = repoInterface.Name.StartsWith("I");
            
            startsWithI.Should().BeTrue($"❌ ADR-123_1_2 违规: Repository 接口命名不符合规范\n\n" +
                $"违规接口: {repoInterface.FullName}\n\n" +
                $"修复建议:\n" +
                $"接口名称必须以 I 开头，格式为 I{{Aggregate}}Repository\n\n" +
                $"参考: docs/copilot/adr-123.prompts.md");
        }
    }
}
