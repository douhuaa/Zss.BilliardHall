namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.8: 禁止的命名空间模式
/// 验证模块不应包含不规范的命名空间模式
/// </summary>
public sealed class ADR_003_8_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_8_1: 模块不应包含不规范的命名空间模式
    /// </summary>
    [Theory(DisplayName = "ADR-003_8_1: 模块不应包含不规范的命名空间模式")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void ADR_003_8_1_Modules_Should_Not_Contain_Irregular_Namespace_Patterns(Assembly moduleAssembly)
    {
        // 禁止的命名空间模式（已在 ADR-001 中覆盖，这里作为 ADR-003 的补充验证）
        var forbiddenPatterns = new[] { ".Util", ".Utils", ".Helper", ".Helpers", ".Common", ".Shared" };

        foreach (var pattern in forbiddenPatterns)
        {
            var result = Types
                .InAssembly(moduleAssembly)
                .That()
                .ResideInNamespaceStartingWith($"{BaseNamespace}.Modules")
                .ShouldNot()
                .ResideInNamespaceContaining(pattern)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                BuildFromArchTestResult(
                    ruleId: "ADR-003_8_1",
                    summary: $"模块不应包含不规范的命名空间模式：{pattern}",
                    failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
                    remediationSteps: new[]
                    {
                        "避免使用 Util/Helper/Common/Shared 等不规范的命名空间",
                        "采用垂直切片组织代码（按用例组织，而非按技术层次）",
                        "技术性抽象移到 Platform 或 BuildingBlocks"
                    },
                    adrReference: "docs/copilot/adr-003.prompts.md (反模式 3, FAQ Q4)"));
        }
    }
}
