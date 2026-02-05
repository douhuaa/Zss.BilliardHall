using NetArchTest.Rules;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_002;

/// <summary>
/// ADR-002_4: 三层依赖方向验证（Rule）
/// 验证完整的单向依赖链：Host → Application → Platform
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-002_4_1: 完整的单向依赖链验证
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md
/// </summary>
public sealed class ADR_002_4_Architecture_Tests
{
    /// <summary>
    /// ADR-002_4_1: 完整的单向依赖链
    /// 验证完整的单向依赖链：Host → Application → Platform（§ADR-002_4_1）
    /// </summary>
    [Fact(DisplayName = "ADR-002_4_1: 验证完整的三层依赖方向 (Host -> Application -> Platform)")]
    public void ADR_002_4_1_Verify_Complete_Three_Layer_Dependency_Direction()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;

        // Platform 不应依赖 Application 或 Host
        var platformResult = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zss.BilliardHall.Application", "Zss.BilliardHall.Host")
            .GetResult();

        platformResult.IsSuccessful.Should().BeTrue($"❌ ADR-002_4_1 违规: Platform 不应依赖 Application 或 Host\n\n" +
        $"违规类型:\n{string.Join("\n", platformResult.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议：\n" +
        $"1. 确保三层依赖方向: Host → Application → Platform\n" +
        $"2. Platform 是最底层，不依赖任何上层\n" +
        $"3. 移除违规的依赖引用\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

        // Application 不应依赖 Host
        var applicationResult = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Host")
            .GetResult();

        applicationResult.IsSuccessful.Should().BeTrue($"❌ ADR-002_4_1 违规: Application 不应依赖 Host\n\n" +
        $"违规类型:\n{string.Join("\n", applicationResult.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议：\n" +
        $"1. 确保三层依赖方向: Host → Application → Platform\n" +
        $"2. Application 不感知运行形态\n" +
        $"3. 移除对 Host 的依赖\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
    }
}
