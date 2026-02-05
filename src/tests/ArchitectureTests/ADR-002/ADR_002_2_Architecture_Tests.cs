using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_002;

/// <summary>
/// ADR-002_2: Application 层约束（Rule）
/// 验证 Application 层职责边界和依赖隔离
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-002_2_1: Application 不依赖 Host
/// - ADR-002_2_2: Application 不依赖 Modules
/// - ADR-002_2_3: Application 唯一 Bootstrapper 入口
/// - ADR-002_2_4: Application 不包含 Host 专属类型
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md
/// </summary>
public sealed class ADR_002_2_Architecture_Tests
{
    /// <summary>
    /// ADR-002_2_1: Application 不依赖 Host
    /// 验证 Application 层禁止依赖 Host 层（§ADR-002_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-002_2_1: Application 不应依赖 Host")]
    public void ADR_002_2_1_Application_Should_Not_Depend_On_Host()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zss.BilliardHall.Host", "Zss.BilliardHall.Host.Web", "Zss.BilliardHall.Host.Worker")
            .GetResult();

        var message = BuildFromArchTestResult(
            ruleId: "ADR-002_2_1",
            summary: "Application 层不应依赖 Host 层",
            failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
            remediationSteps: new[]
            {
                "移除 Application 对 Host 的引用",
                "Application 定义\"系统是什么\"，不应感知运行形态",
                "使用抽象替代具体的 Host 类型（如 ICurrentUserProvider 替代 HttpContext）"
            },
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
        result.IsSuccessful.Should().BeTrue(message);
    }

    /// <summary>
    /// ADR-002_2_2: Application 不依赖 Modules
    /// 验证 Application 层禁止依赖任何业务模块（§ADR-002_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-002_2_2: Application 不应依赖任何 Modules")]
    public void ADR_002_2_2_Application_Should_Not_Depend_On_Modules()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblyData
                .ModuleNames
                .Select(m => $"Zss.BilliardHall.Modules.{m}")
                .ToArray())
            .GetResult();

        var message = BuildFromArchTestResult(
            ruleId: "ADR-002_2_2",
            summary: "Application 层不应依赖任何 Modules",
            failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
            remediationSteps: new[]
            {
                "移除 Application 对 Modules 的引用",
                "Application 通过扫描和反射加载模块，而非直接引用",
                "使用 ApplicationBootstrapper 自动发现并注册模块"
            },
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
        result.IsSuccessful.Should().BeTrue(message);
    }

    /// <summary>
    /// ADR-002_2_3: Application 唯一 Bootstrapper 入口
    /// 验证 Application 必须有唯一入口 ApplicationBootstrapper.Configure（§ADR-002_2_3）
    /// </summary>
    [Fact(DisplayName = "ADR-002_2_3: Application 应有唯一的 ApplicationBootstrapper 入口")]
    public void ADR_002_2_3_Application_Should_Have_Single_Bootstrapper_Entry_Point()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var bootstrappers = Types
            .InAssembly(applicationAssembly)
            .That()
            .HaveNameEndingWith("Bootstrapper")
            .And()
            .AreClasses()
            .GetTypes()
            .ToList();

        var message1 = BuildSimple(
            ruleId: "ADR-002_2_3",
            summary: "Application 层必须包含 Bootstrapper 入口点",
            currentState: "未找到任何 Bootstrapper 类",
            remediation: "创建 ApplicationBootstrapper 类作为 Application 层的唯一入口，提供 public static void Configure() 方法",
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
        bootstrappers.Should().NotBeEmpty(message1);

        var applicationBootstrapper = bootstrappers.FirstOrDefault(t => t.Name == "ApplicationBootstrapper");
        applicationBootstrapper.Should().NotBeNull();

        var configureMethods = applicationBootstrapper
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Configure")
            .ToList();

        var message2 = Build(
            ruleId: "ADR-002_2_3",
            summary: "ApplicationBootstrapper 必须包含 Configure 方法",
            currentState: "ApplicationBootstrapper 类存在，但缺少 Configure 方法",
            remediationSteps: new[]
            {
                "在 ApplicationBootstrapper 中添加 public static void Configure() 方法",
                "方法签名应接受 IServiceCollection, IConfiguration",
                "在此方法中注册所有 Application 层服务和模块"
            },
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
        configureMethods.Should().NotBeEmpty(message2);
    }

    /// <summary>
    /// ADR-002_2_4: Application 不包含 Host 专属类型
    /// 验证 Application 不包含 HttpContext 等 Host 专属类型（§ADR-002_2_4）
    /// </summary>
    [Fact(DisplayName = "ADR-002_2_4: Application 不应包含 HttpContext 等 Host 专属类型")]
    public void ADR_002_2_4_Application_Should_Not_Use_HttpContext()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var result = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore.Http.HttpContext")
            .GetResult();

        var message = BuildFromArchTestResult(
            ruleId: "ADR-002_2_4",
            summary: "Application 层不应使用 HttpContext 等 Host 专属类型",
            failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
            remediationSteps: new[]
            {
                "移除对 HttpContext 的依赖",
                "创建业务抽象（如 ICurrentUserProvider）",
                "在 Host 层实现抽象，Application 层只依赖接口"
            },
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
        result.IsSuccessful.Should().BeTrue(message);
    }
}
