using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;

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

            result.IsSuccessful.Should().BeTrue($"❌ ADR-003_8_1 违规: 模块不应包含不规范的命名空间模式\n\n" +
            $"模块: {moduleAssembly.GetName().Name}\n" +
            $"禁止的模式: {pattern}\n" +
            $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
            $"修复建议:\n" +
            $"1. 避免使用 Util/Helper/Common/Shared 等不规范的命名空间\n" +
            $"2. 采用垂直切片组织代码（按用例组织，而非按技术层次）\n" +
            $"3. 技术性抽象移到 Platform 或 BuildingBlocks\n\n" +
            $"参考: docs/copilot/adr-003.prompts.md (反模式 3, FAQ Q4)");
        }
    }
}
