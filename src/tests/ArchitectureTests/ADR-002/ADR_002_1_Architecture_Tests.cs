namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_002;

/// <summary>
/// ADR-002_1: Platform 层约束（Rule）
/// 验证 Platform 层职责边界和依赖隔离
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-002_1_1: Platform 不依赖 Application
/// - ADR-002_1_2: Platform 不依赖 Host
/// - ADR-002_1_3: Platform 不依赖 Modules
/// - ADR-002_1_4: Platform 唯一 Bootstrapper 入口
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md
/// </summary>
public sealed class ADR_002_1_Architecture_Tests
{
    /// <summary>
    /// ADR-002_1_1: Platform 不依赖 Application
    /// 验证 Platform 层不可访问 Application 层（§ADR-002_1_1）
    /// </summary>
    [Fact(DisplayName = "ADR-002_1_1: Platform 不应依赖 Application")]
    public void ADR_002_1_1_Platform_Should_Not_Depend_On_Application()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Application")
            .GetResult();

        var message = AssertionMessageBuilder.BuildFromArchTestResult(
            ruleId: "ADR-002_1_1",
            summary: "Platform 层不应依赖 Application 层",
            failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
            remediationSteps: new[]
            {
                "移除 Platform 对 Application 的引用",
                "将共享的技术抽象提取到 Platform 层",
                "确保依赖方向正确: Host → Application → Platform"
            },
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

        result.IsSuccessful.Should().BeTrue(message);
    }

    /// <summary>
    /// ADR-002_1_2: Platform 不依赖 Host
    /// 验证 Platform 层不可访问 Host 层（§ADR-002_1_2）
    /// </summary>
    [Fact(DisplayName = "ADR-002_1_2: Platform 不应依赖 Host")]
    public void ADR_002_1_2_Platform_Should_Not_Depend_On_Host()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Zss.BilliardHall.Host", "Zss.BilliardHall.Host.Web", "Zss.BilliardHall.Host.Worker")
            .GetResult();

        var message = AssertionMessageBuilder.BuildFromArchTestResult(
            ruleId: "ADR-002_1_2",
            summary: "Platform 层不应依赖 Host 层",
            failingTypeNames: result.FailingTypes?.Select(t => t.FullName),
            remediationSteps: new[]
            {
                "移除 Platform 对 Host 的引用",
                "Platform 只提供技术基座能力（日志、追踪、异常处理）",
                "不感知运行形态（Web/Worker）"
            },
            adrReference: "docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

        result.IsSuccessful.Should().BeTrue(message);
    }

    /// <summary>
    /// ADR-002_1_3: Platform 不依赖 Modules
    /// 验证 Platform 层不可访问任何业务模块（§ADR-002_1_3）
    /// </summary>
    [Fact(DisplayName = "ADR-002_1_3: Platform 不应依赖任何 Modules")]
    public void ADR_002_1_3_Platform_Should_Not_Depend_On_Modules()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var result = Types
            .InAssembly(platformAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ModuleAssemblyData
                .ModuleNames
                .Select(m => $"Zss.BilliardHall.Modules.{m}")
                .ToArray())
            .GetResult();

        result.IsSuccessful.Should().BeTrue($"❌ ADR-002_1_3 违规: Platform 层不应依赖任何 Modules\n\n" +
        $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
        $"修复建议：\n" +
        $"1. 移除 Platform 对 Modules 的引用\n" +
        $"2. Platform 是技术基座，不感知业务模块\n" +
        $"3. 将业务无关的通用逻辑提取到 BuildingBlocks\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
    }

    /// <summary>
    /// ADR-002_1_4: Platform 唯一 Bootstrapper 入口
    /// 验证 Platform 必须有唯一入口 PlatformBootstrapper.Configure（§ADR-002_1_4）
    /// </summary>
    [Fact(DisplayName = "ADR-002_1_4: Platform 应有唯一的 PlatformBootstrapper 入口")]
    public void ADR_002_1_4_Platform_Should_Have_Single_Bootstrapper_Entry_Point()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var bootstrappers = Types
            .InAssembly(platformAssembly)
            .That()
            .HaveNameEndingWith("Bootstrapper")
            .And()
            .AreClasses()
            .GetTypes()
            .ToList();

        bootstrappers.Should().NotBeEmpty($"❌ ADR-002_1_4 违规: Platform 层必须包含 Bootstrapper 入口点\n\n" + $"修复建议：\n" + $"1. 创建 PlatformBootstrapper 类作为 Platform 层的唯一入口\n" + $"2. 在 PlatformBootstrapper 中封装所有技术配置\n" + $"3. 提供 public static void Configure() 方法\n\n" + $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");

        // 验证有 Configure 方法
        var platformBootstrapper = bootstrappers.FirstOrDefault(t => t.Name == "PlatformBootstrapper");
        platformBootstrapper.Should().NotBeNull();

        var configureMethods = platformBootstrapper
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == "Configure")
            .ToList();

        configureMethods.Should().NotBeEmpty($"❌ ADR-002_1_4 违规: PlatformBootstrapper 必须包含 Configure 方法\n\n" +
        $"修复建议：\n" +
        $"1. 在 PlatformBootstrapper 中添加 public static void Configure() 方法\n" +
        $"2. 方法签名应接受 IServiceCollection, IConfiguration, IHostEnvironment\n" +
        $"3. 在此方法中注册所有 Platform 层服务\n\n" +
        $"参考: docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
    }
}
