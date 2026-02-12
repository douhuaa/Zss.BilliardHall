using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zss.BilliardHall.Platform.Contracts;

/// <summary>
/// 模块启动器标记接口
/// 每个业务模块必须实现此接口以声明其存在
/// Platform 通过反射发现并调用模块的 Configure 方法
/// </summary>
public interface IModuleBootstrapper
{
    /// <summary>
    /// 配置模块服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="environment">主机环境</param>
    void Configure(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment);
}
