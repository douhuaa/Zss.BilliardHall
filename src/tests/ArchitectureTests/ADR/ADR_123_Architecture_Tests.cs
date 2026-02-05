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
                $"违规接口：{repoInterface.FullName}\n" +
                $"当前命名空间：{repoInterface.Namespace}\n\n" +
                $"问题分析：\n" +
                $"Repository 接口应该定义在领域层，作为领域模型的一部分\n\n" +
                $"修复建议：\n" +
                $"1. 将接口移动到 {{Module}}.Domain.Repositories 命名空间\n" +
                $"2. 示例：Zss.BilliardHall.Modules.Orders.Domain.Repositories\n" +
                $"3. 确保接口只定义领域相关的持久化操作\n\n" +
                $"参考：docs/adr/structure/ADR-123-repository-interface-layering-naming.md（§1.1）");
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
                $"违规接口：{repoInterface.FullName}\n" +
                $"当前名称：{repoInterface.Name}\n\n" +
                $"问题分析：\n" +
                $"Repository 接口必须遵循 I{{Aggregate}}Repository 命名模式以保持一致性\n\n" +
                $"修复建议：\n" +
                $"1. 接口名称必须以 I 开头\n" +
                $"2. 格式为 I{{Aggregate}}Repository\n" +
                $"3. 示例：IOrderRepository, IMemberRepository\n" +
                $"4. 避免使用 IGenericRepository 等泛型命名\n\n" +
                $"参考：docs/adr/structure/ADR-123-repository-interface-layering-naming.md（§1.2）");
        }
    }
}
