using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

namespace Zss.BilliardHall.Host.Worker;

/// <summary>
/// Worker Host 层统一 Bootstrapper
/// 封装所有初始化逻辑，简化 Program.cs
/// </summary>
public static class HostBootstrapper
{
    /// <summary>
    /// 配置服务容器
    /// </summary>
    public static void ConfigureServices(HostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        PlatformBootstrapper.Configure(builder.Services, builder.Configuration, builder.Environment);

        // Host 层决定加载哪些模块（类型安全）
        var moduleAssemblies = ModuleRegistry.GetEnabledAssemblies(builder.Configuration);

        ApplicationBootstrapper.Configure(
            builder.Services,
            builder.Configuration,
            builder.Environment,
            moduleAssemblies);
    }
}

