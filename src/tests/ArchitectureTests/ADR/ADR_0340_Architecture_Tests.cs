using System.Reflection;
using NetArchTest.Rules;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-340: 结构化日志与监控约束
/// 验证 Platform 层正确配置 Serilog 和 OpenTelemetry，Application/Modules 层不直接配置日志
/// 
/// 测试-ADR 映射清单：
/// ├─ ADR-340.1: Platform_Should_Reference_Serilog → Platform 必须引用 Serilog.AspNetCore
/// ├─ ADR-340.2: Platform_Should_Reference_OpenTelemetry → Platform 必须引用 OpenTelemetry 包
/// ├─ ADR-340.3: PlatformBootstrapper_Should_Configure_Logging → PlatformBootstrapper 必须配置日志
/// ├─ ADR-340.4: PlatformBootstrapper_Should_Configure_OpenTelemetry → PlatformBootstrapper 必须配置监控
/// └─ ADR-340.5: Modules_Cannot_Reference_Logging_Implementation → Modules 禁止引用具体日志实现
/// 
/// 参考文档：
/// - ADR-340: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md
/// - 工程标准: docs/guides/structured-logging-monitoring-standard.md
/// </summary>
public sealed class ADR_0340_Architecture_Tests
{
    #region 1. Platform 层日志和监控配置约束

    [Fact(DisplayName = "ADR-340.1: Platform 层应引用 Serilog.AspNetCore")]
    public void Platform_Should_Reference_Serilog()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var platformCsproj = Path.Combine(root, "src", "Platform", "Platform.csproj");

        Assert.True(File.Exists(platformCsproj),
            $"❌ ADR-340.1 违规: 找不到 Platform.csproj 文件。\n\n" +
            $"预期路径: {platformCsproj}");

        var content = File.ReadAllText(platformCsproj);

        Assert.True(content.Contains("Serilog.AspNetCore"),
            $"❌ ADR-340.1 违规: Platform 层必须引用 Serilog.AspNetCore 包。\n\n" +
            $"当前状态: Platform.csproj 未引用 Serilog.AspNetCore\n\n" +
            $"修复建议:\n" +
            $"1. 在 Platform.csproj 中添加 <PackageReference Include=\"Serilog.AspNetCore\" />\n" +
            $"2. 确保 Directory.Packages.props 中定义了 Serilog.AspNetCore 版本\n" +
            $"3. 在 PlatformBootstrapper.cs 中配置 Serilog\n\n" +
            $"参考: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.1)\n" +
            $"工程标准: docs/guides/structured-logging-monitoring-standard.md (一、Serilog 结构化日志配置)");
    }

    [Fact(DisplayName = "ADR-340.2: Platform 层应引用 OpenTelemetry 核心包")]
    public void Platform_Should_Reference_OpenTelemetry()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var platformCsproj = Path.Combine(root, "src", "Platform", "Platform.csproj");

        Assert.True(File.Exists(platformCsproj),
            $"❌ ADR-340.2 违规: 找不到 Platform.csproj 文件。");

        var content = File.ReadAllText(platformCsproj);

        var requiredPackages = new[]
        {
            "OpenTelemetry.Exporter.OpenTelemetryProtocol",
            "OpenTelemetry.Extensions.Hosting",
            "OpenTelemetry.Instrumentation.AspNetCore",
            "OpenTelemetry.Instrumentation.Http",
            "OpenTelemetry.Instrumentation.Runtime"
        };

        var missingPackages = requiredPackages.Where(pkg => !content.Contains(pkg)).ToList();

        Assert.True(!missingPackages.Any(),
            $"❌ ADR-340.2 违规: Platform 层必须引用以下 OpenTelemetry 包。\n\n" +
            $"缺失的包:\n" +
            string.Join("\n", missingPackages.Select(pkg => $"  - {pkg}")) + "\n\n" +
            $"修复建议:\n" +
            $"1. 在 Platform.csproj 中添加缺失的 OpenTelemetry 包引用\n" +
            $"2. 确保 Directory.Packages.props 中定义了这些包的版本\n" +
            $"3. 在 PlatformBootstrapper.cs 中配置 OpenTelemetry\n\n" +
            $"参考: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.2)\n" +
            $"工程标准: docs/guides/structured-logging-monitoring-standard.md (二、OpenTelemetry 追踪和指标配置)");
    }

    [Fact(DisplayName = "ADR-340.3: PlatformBootstrapper 应配置 Serilog")]
    public void PlatformBootstrapper_Should_Configure_Serilog()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var bootstrapperFile = Path.Combine(root, "src", "Platform", "PlatformBootstrapper.cs");

        Assert.True(File.Exists(bootstrapperFile),
            $"❌ ADR-340.3 违规: 找不到 PlatformBootstrapper.cs 文件。");

        var content = File.ReadAllText(bootstrapperFile);

        Assert.True(content.Contains("using Serilog") || content.Contains("Serilog."),
            $"❌ ADR-340.3 违规: PlatformBootstrapper 必须引用 Serilog 命名空间。\n\n" +
            $"当前状态: PlatformBootstrapper.cs 未 using Serilog\n\n" +
            $"修复建议:\n" +
            $"1. 在文件顶部添加 'using Serilog;'\n" +
            $"2. 在 Configure() 方法中配置 Log.Logger\n" +
            $"3. 配置至少一个 Sink（Console 或 File）\n\n" +
            $"参考: docs/guides/structured-logging-monitoring-standard.md (一、Serilog 结构化日志配置)");

        Assert.True(content.Contains("Log.Logger") || content.Contains("LoggerConfiguration"),
            $"❌ ADR-340.3 违规: PlatformBootstrapper 必须配置 Serilog。\n\n" +
            $"当前状态: 未发现 Serilog 配置代码\n\n" +
            $"修复建议:\n" +
            $"1. 在 PlatformBootstrapper.Configure() 中创建 LoggerConfiguration\n" +
            $"2. 调用 .WriteTo.Console() 或 .WriteTo.File() 配置 Sink\n" +
            $"3. 调用 .CreateLogger() 并赋值给 Log.Logger\n\n" +
            $"示例代码:\n" +
            $"  Log.Logger = new LoggerConfiguration()\n" +
            $"      .ReadFrom.Configuration(configuration)\n" +
            $"      .WriteTo.Console()\n" +
            $"      .CreateLogger();\n\n" +
            $"参考: docs/guides/structured-logging-monitoring-standard.md (附录 A)");
    }

    [Fact(DisplayName = "ADR-340.4: PlatformBootstrapper 应配置 OpenTelemetry")]
    public void PlatformBootstrapper_Should_Configure_OpenTelemetry()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var bootstrapperFile = Path.Combine(root, "src", "Platform", "PlatformBootstrapper.cs");

        Assert.True(File.Exists(bootstrapperFile),
            $"❌ ADR-340.4 违规: 找不到 PlatformBootstrapper.cs 文件。");

        var content = File.ReadAllText(bootstrapperFile);

        Assert.True(content.Contains("AddOpenTelemetry"),
            $"❌ ADR-340.4 违规: PlatformBootstrapper 必须配置 OpenTelemetry。\n\n" +
            $"当前状态: 未发现 AddOpenTelemetry 调用\n\n" +
            $"修复建议:\n" +
            $"1. 在 PlatformBootstrapper.Configure() 中调用 services.AddOpenTelemetry()\n" +
            $"2. 配置 .WithTracing() 启用追踪\n" +
            $"3. 配置 .WithMetrics() 启用指标\n" +
            $"4. 添加 AspNetCore、Http、Runtime 插桩\n\n" +
            $"示例代码:\n" +
            $"  services.AddOpenTelemetry()\n" +
            $"      .WithTracing(tracing => tracing\n" +
            $"          .AddAspNetCoreInstrumentation()\n" +
            $"          .AddHttpClientInstrumentation())\n" +
            $"      .WithMetrics(metrics => metrics\n" +
            $"          .AddAspNetCoreInstrumentation()\n" +
            $"          .AddRuntimeInstrumentation());\n\n" +
            $"参考: docs/guides/structured-logging-monitoring-standard.md (二、OpenTelemetry 追踪和指标配置)");

        Assert.True(content.Contains("WithTracing") && content.Contains("WithMetrics"),
            $"❌ ADR-340.4 违规: OpenTelemetry 必须同时配置 Tracing 和 Metrics。\n\n" +
            $"当前状态: 缺少 WithTracing 或 WithMetrics 配置\n\n" +
            $"修复建议:\n" +
            $"1. 确保同时配置 .WithTracing() 和 .WithMetrics()\n" +
            $"2. 在 WithTracing 中添加 AspNetCore 和 Http 插桩\n" +
            $"3. 在 WithMetrics 中添加 Runtime 插桩\n\n" +
            $"参考: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.2)");
    }

    #endregion

    #region 2. Application/Modules 层日志配置隔离

    [Fact(DisplayName = "ADR-340.5: Modules 层不应直接引用 Serilog")]
    public void Modules_Cannot_Reference_Serilog_Directly()
    {
        var modulesAssemblies = ModuleAssemblyData.ModuleAssemblies;

        if (!modulesAssemblies.Any())
        {
            Assert.Fail("❌ 未加载任何模块程序集。请先运行 `dotnet build` 构建所有模块。");
        }

        var result = Types.InAssemblies(modulesAssemblies)
            .ShouldNot()
            .HaveDependencyOn("Serilog")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-340.5 违规: Modules 层禁止直接引用 Serilog。\n\n" +
            $"违规类型:\n" +
            string.Join("\n", result.FailingTypeNames?.Select(t => $"  - {t}") ?? Array.Empty<string>()) + "\n\n" +
            $"修复建议:\n" +
            $"1. 移除 Modules 项目中对 Serilog 包的直接引用\n" +
            $"2. 仅使用 Microsoft.Extensions.Logging.ILogger<T> 接口\n" +
            $"3. 通过依赖注入获取 ILogger<T>\n\n" +
            $"正确示例:\n" +
            $"  public class CreateOrderHandler(ILogger<CreateOrderHandler> logger)\n" +
            $"  {{\n" +
            $"      logger.LogInformation(\"订单创建 {{OrderId}}\", orderId);\n" +
            $"  }}\n\n" +
            $"参考: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.5)");
    }

    [Fact(DisplayName = "ADR-340.5: Modules 层不应直接引用 OpenTelemetry")]
    public void Modules_Cannot_Reference_OpenTelemetry_Directly()
    {
        var modulesAssemblies = ModuleAssemblyData.ModuleAssemblies;

        if (!modulesAssemblies.Any())
        {
            Assert.Fail("❌ 未加载任何模块程序集。请先运行 `dotnet build` 构建所有模块。");
        }

        var result = Types.InAssemblies(modulesAssemblies)
            .ShouldNot()
            .HaveDependencyOn("OpenTelemetry")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-340.5 违规: Modules 层禁止直接引用 OpenTelemetry（配置类）。\n\n" +
            $"违规类型:\n" +
            string.Join("\n", result.FailingTypeNames?.Select(t => $"  - {t}") ?? Array.Empty<string>()) + "\n\n" +
            $"修复建议:\n" +
            $"1. 移除 Modules 项目中对 OpenTelemetry 配置包的引用\n" +
            $"2. 如需创建自定义 Activity，可使用 System.Diagnostics.ActivitySource\n" +
            $"3. Platform 层负责配置 OpenTelemetry，Modules 层只使用标准 API\n\n" +
            $"允许的用法:\n" +
            $"  - 使用 System.Diagnostics.Activity（.NET BCL）\n" +
            $"  - 使用 ILogger<T> 记录日志\n\n" +
            $"参考: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.5)");
    }

    [Fact(DisplayName = "ADR-340.5: Application 层不应直接配置 Serilog")]
    public void Application_Cannot_Configure_Serilog()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var applicationDll = Path.Combine(root, "src", "Application", "bin", "Debug", "net10.0", "Application.dll");
        
        if (!File.Exists(applicationDll))
        {
            Assert.Fail($"❌ 未找到 Application.dll。请先运行 `dotnet build` 构建 Application 项目。路径: {applicationDll}");
        }

        var applicationAssembly = Assembly.LoadFrom(applicationDll);

        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("Serilog")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-340.5 违规: Application 层禁止直接引用 Serilog。\n\n" +
            $"违规类型:\n" +
            string.Join("\n", result.FailingTypeNames?.Select(t => $"  - {t}") ?? Array.Empty<string>()) + "\n\n" +
            $"修复建议:\n" +
            $"1. 移除 Application 项目中对 Serilog 包的引用\n" +
            $"2. 仅使用 Microsoft.Extensions.Logging.ILogger<T> 接口\n" +
            $"3. 日志配置由 Platform 层的 PlatformBootstrapper 负责\n\n" +
            $"参考: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.5)\n" +
            $"参考: docs/adr/constitutional/ADR-0002-platform-application-host-bootstrap.md");
    }

    [Fact(DisplayName = "ADR-340.5: Application 层不应直接配置 OpenTelemetry")]
    public void Application_Cannot_Configure_OpenTelemetry()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var applicationDll = Path.Combine(root, "src", "Application", "bin", "Debug", "net10.0", "Application.dll");
        
        if (!File.Exists(applicationDll))
        {
            Assert.Fail($"❌ 未找到 Application.dll。请先运行 `dotnet build` 构建 Application 项目。路径: {applicationDll}");
        }

        var applicationAssembly = Assembly.LoadFrom(applicationDll);

        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOn("OpenTelemetry")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-340.5 违规: Application 层禁止直接引用 OpenTelemetry 配置包。\n\n" +
            $"违规类型:\n" +
            string.Join("\n", result.FailingTypeNames?.Select(t => $"  - {t}") ?? Array.Empty<string>()) + "\n\n" +
            $"修复建议:\n" +
            $"1. 移除 Application 项目中对 OpenTelemetry 配置包的引用\n" +
            $"2. OpenTelemetry 配置由 Platform 层的 PlatformBootstrapper 负责\n\n" +
            $"参考: docs/adr/technical/ADR-340-structured-logging-monitoring-constraints.md (ADR-340.5)\n" +
            $"参考: docs/adr/constitutional/ADR-0002-platform-application-host-bootstrap.md");
    }

    #endregion
}
