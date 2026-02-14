using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zss.BilliardHall.Platform.Contracts;

/// <summary>
/// 模块契约：每个业务模块通过实现此接口参与系统装配。
/// 不依赖反射，完全显式管理模块生命周期。
/// 冻结规范：不允许扩展此接口，简单才稳定。
/// </summary>
public interface IModule
{
    /// <summary>
    /// 模块名称（用于配置和日志）
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 注册模块所需服务（DI 注册、配置绑定、Marten Schema 扩展等）
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment);
}


