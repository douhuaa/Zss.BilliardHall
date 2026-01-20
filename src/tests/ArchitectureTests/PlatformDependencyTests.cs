using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// Platform 和 Application 层依赖约束测试
/// 确保 Platform 不依赖 Application；Application 不依赖 Host
/// </summary>
public class PlatformDependencyTests
{
    private static readonly Assembly PlatformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;

    [Fact]
    public void Platform_Should_Not_Depend_On_Application()
    {
        // Platform 不应依赖 Application 层
        var result = Types.InAssembly(PlatformAssembly)
            .ShouldNot().HaveDependencyOn("Zss.BilliardHall.Application")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Platform 层不应依赖 Application 层。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。" +
            $"修复建议：依赖关系应该是 Application -> Platform，而不是反向。");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Host()
    {
        // Application 不应依赖 Host 层
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Host",
                "Zss.BilliardHall.Host.Web",
                "Zss.BilliardHall.Host.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application 层不应依赖 Host 层。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。" +
            $"修复建议：依赖关系应该是 Host -> Application，而不是反向。");
    }

    [Fact]
    public void Platform_Should_Not_Depend_On_Host()
    {
        // Platform 不应依赖 Host 层
        var result = Types.InAssembly(PlatformAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Host",
                "Zss.BilliardHall.Host.Web",
                "Zss.BilliardHall.Host.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Platform 层不应依赖 Host 层。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。" +
            $"修复建议：依赖关系应该是 Host -> Platform，而不是反向。");
    }

    [Fact]
    public void Platform_Should_Not_Depend_On_Modules()
    {
        // Platform 不应依赖任何 Modules
        var result = Types.InAssembly(PlatformAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Modules.Members",
                "Zss.BilliardHall.Modules.Orders")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Platform 层不应依赖任何 Modules。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。" +
            $"修复建议：Platform 是基础设施层，应该被 Modules 依赖，而不是依赖 Modules。");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Modules()
    {
        // Application 不应依赖任何 Modules
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Modules.Members",
                "Zss.BilliardHall.Modules.Orders")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application 层不应依赖任何 Modules。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。" +
            $"修复建议：Application 层用于横切关注点，不应耦合具体业务模块。");
    }
}
