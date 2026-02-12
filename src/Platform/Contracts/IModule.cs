using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zss.BilliardHall.Platform.Contracts;

/// <summary>
/// 模块契约：每个业务模块通过实现此接口参与系统装配。
/// 每个业务模块必须实现此接口以声明其存在
/// Platform 通过反射发现并调用模块的 ConfigureServices 方法
/// </summary>
public interface IModule
{
    /// <summary>
    /// 注册模块所需服务（DI 注册、配置绑定、基础设施扩展点等）。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="environment">主机环境</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment);
}
