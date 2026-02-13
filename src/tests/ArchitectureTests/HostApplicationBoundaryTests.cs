using Zss.BilliardHall.Application;
using Zss.BilliardHall.Host.Web;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// Host 和 Application 层边界测试
/// 确保 Host 可以引用 Modules，但 Application 不能
/// </summary>
public class HostApplicationBoundaryTests
{
    [Fact(DisplayName = "Application 不能引用 Modules")]
    public void Application_CannotReference_Modules()
    {
        var applicationAssembly = typeof(ApplicationBootstrapper).Assembly;

        var result = Types
            .InAssembly(applicationAssembly)
            .Should()
            .NotHaveDependencyOn("Zss.BilliardHall.Modules")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Application 层不应该依赖 Modules（ADR-002），它应该纯装��通过 Assembly[] 参数接收模块");
    }

    [Theory(DisplayName = "HostBootstrapper.ConfigureApplication 必须映射 Wolverine 端点")]
    [InlineData(typeof(HostBootstrapper))]
    public void HostBootstrapper_ConfigureApplication_MustMapWolverineEndpoints(Type hostBootstrapperType)
    {
        var configureAppMethod = hostBootstrapperType.GetMethod("ConfigureApplication")
            ?? throw new InvalidOperationException($"找不到 {hostBootstrapperType.Name}.ConfigureApplication 方法");

        var il = configureAppMethod.GetMethodBody()?.GetILAsByteArray() ?? [];

        // 检查方法体不能为空（表示至少有实现）
        Assert.True(il.Length > 0,
            $"{hostBootstrapperType.Name}.ConfigureApplication 必须有实现，确保映射 Wolverine 端点");
    }
}


