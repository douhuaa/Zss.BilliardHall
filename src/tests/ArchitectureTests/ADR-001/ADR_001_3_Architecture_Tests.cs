using FluentAssertions;
using NetArchTest.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_001;

/// <summary>
/// ADR-001_3: 模块间通信约束（Rule）
/// 验证模块间通信仅允许事件/契约/原始类型，契约不含业务决策字段
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-001_3_1: 模块间通信仅允许事件/契约/原始类型
/// - ADR-001_3_2: 契约不含业务决策字段
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md
/// </summary>
public sealed class ADR_001_3_Architecture_Tests
{
    /// <summary>
    /// ADR-001_3_1: 模块间通信仅允许事件/契约/原始类型
    /// 验证模块间仅允许：领域事件、契约 DTO、原始类型，禁止直接依赖 Entity/Aggregate/VO（§ADR-001_3_1）
    /// </summary>
    [Theory(DisplayName = "ADR-001_3_1: 模块间只允许事件/契约/原始类型通信（语义检查）")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_001_3_1_Contract_Rules_Semantic_Check(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName()
            .Name!
            .Split('.')
            .Last();

        // 检查是否有跨模块的领域对象引用（Entity/Aggregate/ValueObject）
        var domainTypes = new[] { "Entity", "Aggregate", "ValueObject", "DomainEvent" };

        foreach (var other in ModuleAssemblyData.ModuleNames.Where(m => m != moduleName))
        {
            foreach (var domainType in domainTypes)
            {
                var result = Types
                    .InAssembly(moduleAssembly)
                    .That()
                    .ResideInNamespaceStartingWith($"Zss.BilliardHall.Modules.{moduleName}")
                    .ShouldNot()
                    .HaveDependencyOn($"Zss.BilliardHall.Modules.{other}.Domain")
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    BuildFromArchTestResult(
                        ruleId: "ADR-001_3_1",
                        summary: $"模块 {moduleName} 不应依赖其他模块 {other} 的领域对象",
                        failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
                        remediationSteps: new[]
                        {
                            "使用领域事件进行异步通信",
                            "使用只读契约（Contracts）传递数据",
                            "传递原始类型（Guid、string、int）而非领域对象"
                        },
                        adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md"));
            }
        }
    }

    /// <summary>
    /// ADR-001_3_2: 契约不含业务决策字段
    /// 验证契约 DTO 不含业务判断字段（如 CanRefund），契约不含行为方法（§ADR-001_3_2）
    /// </summary>
    [Fact(DisplayName = "ADR-001_3_2: Contract 不应包含业务判断字段（语义分析）")]
    public void ADR_001_3_2_Contract_Business_Field_Analyzer()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var contractTypes = Types
            .InAssembly(platformAssembly)
            .That()
            .ResideInNamespace("Zss.BilliardHall.Platform.Contracts")
            .And()
            .AreClasses()
            .GetTypes();

        // 业务判断相关字段名称模式
        var businessDecisionPatterns = new[] {
            "CanRefund", "CanCancel", "IsValid", "IsEnabled", "IsActive",
            "ShouldApprove", "MustVerify", "AllowEdit"
        };

        foreach (var contractType in contractTypes)
        {
            if (contractType.IsAbstract || contractType.IsInterface)
                continue;

            var properties = contractType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var hasBusinessDecisionPattern = businessDecisionPatterns.Any(pattern => prop.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase));

                hasBusinessDecisionPattern.Should().BeFalse(
                    BuildSimple(
                        ruleId: "ADR-001_3_2",
                        summary: $"Contract {contractType.Name} 包含疑似业务判断字段: {prop.Name}",
                        currentState: "Contract 应仅用于数据传递，不包含业务决策字段。注意：此为 L2/L3 级别检查，可能需要人工确认是否真的违规",
                        remediation: "将业务判断逻辑移至 Handler 或领域模型中，使用简单的数据字段（如状态枚举）而非判断字段",
                        adrReference: "docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md"));
            }
        }
    }
}
