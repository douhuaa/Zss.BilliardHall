using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;

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

        result.IsSuccessful.Should().BeTrue($"❌ ADR-002_2_1 违规: Application 层不应依赖 Host 层\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Application 对 Host 的引用\n" +
        $"2. Application 定义\"系统是什么\"，不应感知运行形态\n" +
        $"3. 使用抽象替代具体的 Host 类型（如 ICurrentUserProvider 替代 HttpContext）\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
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

        result.IsSuccessful.Should().BeTrue($"❌ ADR-002_2_2 违规: Application 层不应依赖任何 Modules\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除 Application 对 Modules 的引用\n" +
        $"2. Application 通过扫描和反射加载模块，而非直接引用\n" +
        $"3. 使用 ApplicationBootstrapper 自动发现并注册模块\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
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

        bootstrappers.Should().NotBeEmpty($"❌ ADR-002_2_3 违规: Application 层必须包含 Bootstrapper 入口点\n\n" + $"修复建议:\n" + $"1. 创建 ApplicationBootstrapper 类作为 Application 层的唯一入口\n" + $"2. 在 ApplicationBootstrapper 中封装模块扫描和业务装配\n" + $"3. 提供 public static void Configure() 方法\n\n" + $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

        var applicationBootstrapper = bootstrappers.FirstOrDefault(t => t.Name == "ApplicationBootstrapper");
        applicationBootstrapper.Should().NotBeNull();

        var configureMethods = applicationBootstrapper
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Configure")
            .ToList();

        configureMethods.Should().NotBeEmpty($"❌ ADR-002_2_3 违规: ApplicationBootstrapper 必须包含 Configure 方法\n\n" +
        $"修复建议:\n" +
        $"1. 在 ApplicationBootstrapper 中添加 public static void Configure() 方法\n" +
        $"2. 方法签名应接受 IServiceCollection, IConfiguration\n" +
        $"3. 在此方法中注册所有 Application 层服务和模块\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
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

        result.IsSuccessful.Should().BeTrue($"❌ ADR-002_2_4 违规: Application 层不应使用 HttpContext 等 Host 专属类型\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议:\n" +
        $"1. 移除对 HttpContext 的依赖\n" +
        $"2. 创建业务抽象（如 ICurrentUserProvider）\n" +
        $"3. 在 Host 层实现抽象，Application 层只依赖接口\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
    }
}
