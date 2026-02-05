using System.Reflection;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using NetArchTest.Rules;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-340: 结构化日志与监控约束
/// 验证 Platform 层正确引用日志包，Application/Modules 层不直接配置日志
/// 
/// 测试-ADR 映射清单：
/// ├─ ADR-340.1: Platform_Must_Reference_Logging_Packages → Platform 必须引用日志基础设施包
/// ├─ ADR-340.2: PlatformBootstrapper_Must_Contain_Logging_Configuration → PlatformBootstrapper 必须包含配置代码
/// ├─ ADR-340.3: （Roslyn Analyzer 待实现）Handler 禁止使用 Console 输出
/// ├─ ADR-340.4: （Roslyn Analyzer 待实现）日志调用禁止使用字符串插值
/// └─ ADR-340.5: Modules_Cannot_Reference_Logging_Implementation → Modules 禁止直接引用日志实现
/// 
/// 执行能力说明：
/// - L1 规则（340.1/340.2/340.5）：当前可通过 NetArchTest 和文本检查验证，CI 阻断
/// - L2 规则（340.3/340.4）：需要 Roslyn Analyzer，当前作为 Review Gate，不参与 CI 阻断
/// 
/// 参考文档：
/// - ADR-340: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md
/// - 工程标准: docs/guides/structured-logging-monitoring-standard.md
/// </summary>
public sealed class ADR_340_Architecture_Tests
{
    #region 1. Platform 层日志和监控依赖约束（L1 - 依赖验证）

    [Fact(DisplayName = "ADR-340_1_1: Platform 层必须引用所有日志和监控基础设施包")]
    public void Platform_Must_Reference_Logging_Packages()
    {
        var root = TestEnvironment.RepositoryRoot;
        var platformCsproj = Path.Combine(root, "src", "Platform", "Platform.csproj");

        File.Exists(platformCsproj).Should().BeTrue($"❌ ADR-340_1_1 违规: 找不到 Platform.csproj 文件\n\n" +
            $"预期路径: {platformCsproj}\n\n" +
            $"问题分析：\n" +
            $"Platform 层是可观测性基础设施的核心，必须包含项目文件以配置日志和监控依赖\n\n" +
            $"修复建议：\n" +
            $"1. 检查 src/Platform 目录是否存在\n" +
            $"2. 如果目录不存在，创建 Platform 目录\n" +
            $"3. 在目录中创建 Platform.csproj 项目文件\n" +
            $"4. 添加必要的包引用（Serilog、OpenTelemetry 等）\n\n" +
            $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.1）");

        var content = File.ReadAllText(platformCsproj);

        var requiredPackages = new[]
        {
            "Serilog.AspNetCore",
            "OpenTelemetry.Exporter.OpenTelemetryProtocol",
            "OpenTelemetry.Extensions.Hosting",
            "OpenTelemetry.Instrumentation.AspNetCore",
            "OpenTelemetry.Instrumentation.Http",
            "OpenTelemetry.Instrumentation.Runtime"
        };

        var missingPackages = requiredPackages.Where(pkg => !content.Contains(pkg)).ToList();
        
        var missingPackagesText = missingPackages.Any()
            ? string.Join("\n", missingPackages.Select(pkg => $"  - {pkg}"))
            : "";

        missingPackages.Any().Should().BeFalse($"❌ ADR-340_1_1 违规：Platform 层必须引用以下日志和监控基础设施包。\n\n" +
            $"缺失的包：\n{missingPackagesText}\n\n" +
            $"修复建议：\n" +
            $"1. 在 Platform.csproj 中添加缺失的包引用：\n" +
            $"   <PackageReference Include=\"<包名>\" />\n" +
            $"2. 确保 Directory.Packages.props 中定义了这些包的版本\n\n" +
            $"执行级别：L1【依赖验证】- 此规则仅验证包引用，不保证实际配置\n\n" +
            $"参考：docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md【ADR-340.1】");
    }

    [Fact(DisplayName = "ADR-340_1_2: PlatformBootstrapper 必须包含日志配置代码")]
    public void PlatformBootstrapper_Must_Contain_Logging_Configuration()
    {
        var root = TestEnvironment.RepositoryRoot;
        var bootstrapperFile = Path.Combine(root, "src", "Platform", "PlatformBootstrapper.cs");

        File.Exists(bootstrapperFile).Should().BeTrue($"❌ ADR-340_1_2 违规: 找不到 PlatformBootstrapper.cs 文件\n\n" +
            $"预期路径: {bootstrapperFile}\n\n" +
            $"问题分析：\n" +
            $"PlatformBootstrapper 是应用程序启动时配置可观测性基础设施的唯一入口，缺少此文件将导致日志和监控功能无法初始化\n\n" +
            $"修复建议：\n" +
            $"1. 在 src/Platform 目录中创建 PlatformBootstrapper.cs 文件\n" +
            $"2. 实现 Configure() 方法用于配置 Serilog 和 OpenTelemetry\n" +
            $"3. 确保在应用程序启动时调用此配置方法\n" +
            $"4. 参考工程标准文档中的代码示例\n\n" +
            $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.2）");

        var content = File.ReadAllText(bootstrapperFile);

        // 检查 Serilog 配置代码
        var hasSerilogUsing = content.Contains("using Serilog") || content.Contains("Serilog.");
        var hasSerilogConfig = content.Contains("Log.Logger") || content.Contains("LoggerConfiguration");

        hasSerilogUsing.Should().BeTrue($"❌ ADR-340_1_2 违规：PlatformBootstrapper 必须引用 Serilog 命名空间。\n\n" +
            $"当前状态：PlatformBootstrapper.cs 未 using Serilog\n\n" +
            $"修复建议：\n" +
            $"1. 在文件顶部添加 'using Serilog;'\n" +
            $"2. 在 Configure() 方法中配置 Log.Logger\n\n" +
            $"执行级别：L1（文本匹配）- 此规则仅验证代码存在，不验证配置正确性\n\n" +
            $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.2）");

        hasSerilogConfig.Should().BeTrue($"❌ ADR-340_1_2 违规: PlatformBootstrapper 必须包含 Serilog 配置代码。\n\n" +
            $"当前状态: 未发现 Log.Logger 或 LoggerConfiguration\n\n" +
            $"修复建议：\n" +
            $"1. 在 PlatformBootstrapper.Configure() 中创建 LoggerConfiguration\n" +
            $"2. 配置至少一个 Sink（Console 或 File）\n" +
            $"3. 调用 .CreateLogger() 并赋值给 Log.Logger\n\n" +
            $"执行级别: L1（文本匹配）- 此规则仅验证代码存在，不验证配置正确性\n\n" +
            $"参考：docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.2)");

        // 检查 OpenTelemetry 配置代码
        var hasOpenTelemetryConfig = content.Contains("AddOpenTelemetry");
        var hasTracingAndMetrics = content.Contains("WithTracing") && content.Contains("WithMetrics");

        hasOpenTelemetryConfig.Should().BeTrue($"❌ ADR-340_1_2 违规：PlatformBootstrapper 必须包含 OpenTelemetry 配置代码。\n\n" +
            $"当前状态：未发现 AddOpenTelemetry 调用\n\n" +
            $"修复建议：\n" +
            $"1. 在 PlatformBootstrapper.Configure() 中调用 services.AddOpenTelemetry()\n" +
            $"2. 配置 .WithTracing() 和 .WithMetrics()\n\n" +
            $"执行级别：L1（文本匹配）- 此规则仅验证代码存在，不验证配置正确性\n\n" +
            $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.2）");

        hasTracingAndMetrics.Should().BeTrue($"❌ ADR-340_1_2 违规: OpenTelemetry 必须同时配置 Tracing 和 Metrics。\n\n" +
            $"当前状态: 缺少 WithTracing 或 WithMetrics 配置\n\n" +
            $"修复建议：\n" +
            $"1. 确保同时配置 .WithTracing() 和 .WithMetrics()\n" +
            $"2. 在 WithTracing 中添加 AspNetCore 和 Http 插桩\n" +
            $"3. 在 WithMetrics 中添加 Runtime 插桩\n\n" +
            $"执行级别: L1（文本匹配）- 此规则仅验证代码存在，不验证配置正确性\n\n" +
            $"参考：docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.2)");
    }

    #endregion

    #region 2. Application/Modules 层日志配置隔离（L1 - 依赖隔离）

    [Fact(DisplayName = "ADR-340_1_5: Modules 层不应直接引用 Serilog")]
    public void Modules_Cannot_Reference_Serilog_Directly()
    {
        var modulesAssemblies = ModuleAssemblyData.ModuleAssemblies;

        if (!modulesAssemblies.Any())
        {
            true.Should().BeFalse($"❌ ADR-340_1_5 违规: 未加载任何模块程序集，无法验证依赖隔离\n\n" +
                $"问题分析：\n" +
                $"测试需要加载模块程序集以验证其是否违规引用 Serilog，当前未找到任何模块\n\n" +
                $"修复建议：\n" +
                $"1. 运行 `dotnet build` 构建所有模块项目\n" +
                $"2. 确保模块项目配置正确且能成功编译\n" +
                $"3. 检查 ModuleAssemblyData 是否正确扫描模块程序集\n" +
                $"4. 验证模块项目输出路径是否正确\n\n" +
                $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.5）");
        }

        var result = Types.InAssemblies(modulesAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Serilog")
            .GetResult();

        var failingTypesText = result.FailingTypeNames != null && result.FailingTypeNames.Any()
            ? string.Join("\n", result.FailingTypeNames.Select(t => $"  - {t}"))
            : "";

        result.IsSuccessful.Should().BeTrue($"❌ ADR-340_1_5 违规：Modules 层禁止直接引用 Serilog。\n\n" +
            $"违规类型：\n{failingTypesText}\n\n" +
            $"修复建议：\n" +
            $"1. 移除 Modules 项目中对 Serilog 包的直接引用\n" +
            $"2. 仅使用 Microsoft.Extensions.Logging.ILogger<T> 接口\n" +
            $"3. 通过依赖注入获取 ILogger<T>\n\n" +
            $"正确示例：\n" +
            $"  public class CreateOrderHandler【ILogger<CreateOrderHandler> logger】\n" +
            $"  {{\n" +
            $"      logger.LogInformation【\\\"订单创建 {{{{OrderId}}}}\\\"，orderId】;\n" +
            $"  }}\n\n" +
            $"执行级别：L1【依赖隔离】\n\n" +
            $"参考：docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md【ADR-340.5】");
    }

    [Fact(DisplayName = "ADR-340_1_6: Modules 层不应直接引用 OpenTelemetry")]
    public void Modules_Cannot_Reference_OpenTelemetry_Directly()
    {
        var modulesAssemblies = ModuleAssemblyData.ModuleAssemblies;

        if (!modulesAssemblies.Any())
        {
            true.Should().BeFalse($"❌ ADR-340_1_6 违规: 未加载任何模块程序集，无法验证依赖隔离\n\n" +
                $"问题分析：\n" +
                $"测试需要加载模块程序集以验证其是否违规引用 OpenTelemetry，当前未找到任何模块\n\n" +
                $"修复建议：\n" +
                $"1. 运行 `dotnet build` 构建所有模块项目\n" +
                $"2. 确保模块项目配置正确且能成功编译\n" +
                $"3. 检查 ModuleAssemblyData 是否正确扫描模块程序集\n" +
                $"4. 验证模块项目输出路径是否正确\n\n" +
                $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.6）");
        }

        var result = Types.InAssemblies(modulesAssemblies)
            .ShouldNot()
            .HaveDependencyOn("OpenTelemetry")
            .GetResult();

        var failingTypesText = result.FailingTypeNames != null && result.FailingTypeNames.Any()
            ? string.Join("\n", result.FailingTypeNames.Select(t => $"  - {t}"))
            : "";

        result.IsSuccessful.Should().BeTrue($"❌ ADR-340_1_5 违规: Modules 层禁止直接引用 OpenTelemetry【配置类】。\n\n" +
            $"违规类型:\n" +
            $"{failingTypesText}\n\n" +
            $"修复建议：\n" +
            $"1. 移除 Modules 项目中对 OpenTelemetry 配置包的引用\n" +
            $"2. 如需创建自定义 Activity，可使用 System.Diagnostics.ActivitySource\n" +
            $"3. Platform 层负责配置 OpenTelemetry，Modules 层只使用标准 API\n\n" +
            $"允许的用法:\n" +
            $"  - 使用 System.Diagnostics.Activity（.NET BCL）\n" +
            $"  - 使用 ILogger<T> 记录日志\n\n" +
            $"执行级别: L1（依赖隔离）\n\n" +
            $"参考：docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.5)");
    }

    [Fact(DisplayName = "ADR-340_1_5: Application 层不应直接配置 Serilog")]
    public void Application_Cannot_Configure_Serilog()
    {
        var root = TestEnvironment.RepositoryRoot;
        var configuration = GetBuildConfiguration();
        var applicationDll = Path.Combine(root, "src", "Application", "bin", configuration, "net10.0", "Application.dll");
        
        if (!File.Exists(applicationDll))
        {
            true.Should().BeFalse($"❌ ADR-340_1_5 违规: 未找到 Application.dll，无法验证 Serilog 依赖隔离\n\n" +
                $"预期路径: {applicationDll}\n\n" +
                $"问题分析：\n" +
                $"测试需要加载 Application 程序集以验证其是否违规引用 Serilog，当前未找到编译输出\n\n" +
                $"修复建议：\n" +
                $"1. 运行 `dotnet build` 构建 Application 项目\n" +
                $"2. 确保 Application 项目配置正确且能成功编译\n" +
                $"3. 检查构建配置是否为 {configuration}\n" +
                $"4. 验证项目输出路径是否正确\n\n" +
                $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.5）");
        }

        var applicationAssembly = Assembly.LoadFrom(applicationDll);

        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Serilog")
            .GetResult();

        var failingTypesText = result.FailingTypeNames != null && result.FailingTypeNames.Any()
            ? string.Join("\n", result.FailingTypeNames.Select(t => $"  - {t}"))
            : "";

        result.IsSuccessful.Should().BeTrue($"❌ ADR-340_1_5 违规: Application 层禁止直接引用 Serilog。\n\n" +
            $"违规类型:\n" +
            $"{failingTypesText}\n\n" +
            $"修复建议：\n" +
            $"1. 移除 Application 项目中对 Serilog 包的引用\n" +
            $"2. 仅使用 Microsoft.Extensions.Logging.ILogger<T> 接口\n" +
            $"3. 日志配置由 Platform 层的 PlatformBootstrapper 负责\n\n" +
            $"执行级别: L1（依赖隔离）\n\n" +
            $"参考：docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.5)\n" +
            $"参考：docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
    }

    [Fact(DisplayName = "ADR-340_1_5: Application 层不应直接配置 OpenTelemetry")]
    public void Application_Cannot_Configure_OpenTelemetry()
    {
        var root = TestEnvironment.RepositoryRoot;
        var configuration = GetBuildConfiguration();
        var applicationDll = Path.Combine(root, "src", "Application", "bin", configuration, "net10.0", "Application.dll");
        
        if (!File.Exists(applicationDll))
        {
            true.Should().BeFalse($"❌ ADR-340_1_5 违规: 未找到 Application.dll，无法验证 OpenTelemetry 依赖隔离\n\n" +
                $"预期路径: {applicationDll}\n\n" +
                $"问题分析：\n" +
                $"测试需要加载 Application 程序集以验证其是否违规引用 OpenTelemetry，当前未找到编译输出\n\n" +
                $"修复建议：\n" +
                $"1. 运行 `dotnet build` 构建 Application 项目\n" +
                $"2. 确保 Application 项目配置正确且能成功编译\n" +
                $"3. 检查构建配置是否为 {configuration}\n" +
                $"4. 验证项目输出路径是否正确\n\n" +
                $"参考：docs/adr/technical/ADR-340-platform-observability-stack.md（§1.5）");
        }

        var applicationAssembly = Assembly.LoadFrom(applicationDll);

        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("OpenTelemetry")
            .GetResult();

        var failingTypesText = result.FailingTypeNames != null && result.FailingTypeNames.Any()
            ? string.Join("\n", result.FailingTypeNames.Select(t => $"  - {t}"))
            : "";

        result.IsSuccessful.Should().BeTrue($"❌ ADR-340_1_5 违规: Application 层禁止直接引用 OpenTelemetry 配置包。\n\n" +
            $"违规类型:\n" +
            $"{failingTypesText}\n\n" +
            $"修复建议：\n" +
            $"1. 移除 Application 项目中对 OpenTelemetry 配置包的引用\n" +
            $"2. OpenTelemetry 配置由 Platform 层的 PlatformBootstrapper 负责\n\n" +
            $"执行级别: L1（依赖隔离）\n\n" +
            $"参考：docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.5)\n" +
            $"参考：docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md");
    }

    #endregion

    private static string GetBuildConfiguration()
    {
        // 检查环境变量（CI 会设置）
        var configFromEnv = Environment.GetEnvironmentVariable("Configuration");
        if (!string.IsNullOrEmpty(configFromEnv))
        {
            return configFromEnv;
        }

        // 尝试从当前程序集路径推断
        var assemblyPath = typeof(ADR_340_Architecture_Tests).Assembly.Location;
        if (assemblyPath.Contains("/Release/") || assemblyPath.Contains("\\Release\\"))
        {
            return "Release";
        }
        
        // 默认使用 Debug
        return "Debug";
    }
}
