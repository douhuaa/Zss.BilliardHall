using FluentAssertions;
using NetArchTest.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_001;

/// <summary>
/// ADR-001_2: 垂直切片架构（Rule）
/// 验证垂直切片以用例为最小单元，禁止横向 Service 抽象
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-001_2_1: 垂直切片以用例为最小单元
/// - ADR-001_2_2: 禁止横向 Service 抽象
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md
/// </summary>
public sealed class ADR_001_2_Architecture_Tests
{
    /// <summary>
    /// ADR-001_2_1: 垂直切片以用例为最小单元
    /// 验证用例（Use Case）为最小组织单元，Handler 必须在 UseCases 命名空间（§ADR-001_2_1）
    /// </summary>
    [Theory(DisplayName = "ADR-001_2_1: Handler 应该在 UseCases 命名空间下")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_001_2_1_Handlers_Should_Be_In_UseCases_Namespace(Assembly moduleAssembly)
    {
        var handlers = Types
            .InAssembly(moduleAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .GetTypes();

        foreach (var handler in handlers)
        {
            var ns = handler.Namespace ?? "";
            // 接受 UseCases 或 Features（历史原因，语义相同）
            var hasUseCases = ns.Contains(".UseCases.") || ns.Contains(".Features.");

            hasUseCases.Should().BeTrue(
                BuildSimple(
                    ruleId: "ADR-001_2_1",
                    summary: $"Handler {handler.FullName} 必须在 UseCases 或 Features 命名空间下",
                    currentState: $"当前命名空间: {ns}",
                    remediation: "将 Handler 移动到对应的 UseCases/<用例名>/ 或 Features/<用例名>/ 目录下，采用垂直切片架构，每个用例包含 Endpoint、Command/Query、Handler，避免创建横向的 Service 层",
                    adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md"));
        }
    }

    /// <summary>
    /// ADR-001_2_2: 禁止横向 Service 抽象
    /// 验证禁止使用 Service/Manager/Helper 类承载业务逻辑（§ADR-001_2_2）
    /// </summary>
    [Theory(DisplayName = "ADR-001_2_2: 模块不应包含横向 Service 类")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_001_2_2_Modules_Should_Not_Contain_Service_Classes(Assembly moduleAssembly)
    {
        var forbidden = new[] { "Repository", "Service", "Manager", "Store" };
        foreach (var word in forbidden)
        {
            var result = Types
                .InAssembly(moduleAssembly)
                .That()
                .ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
                .ShouldNot()
                .HaveNameMatching($".*{word}.*")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                BuildFromArchTestResult(
                    ruleId: "ADR-001_2_2",
                    summary: $"模块 {moduleAssembly.GetName().Name} 禁止包含名称含 '{word}' 的类型",
                    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
                    remediationSteps: new[]
                    {
                        "将业务逻辑放入领域模型或 Handler 中",
                        "采用垂直切片架构，避免横向抽象层",
                        "如需共享技术能力，移至 Platform/BuildingBlocks"
                    },
                    adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md"));
        }
    }
}
