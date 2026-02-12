using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Platform;

/// <summary>
/// 模块加载器
/// 职责：通过反射加载并注册实现了 IModuleBootstrapper 的模块
/// </summary>
public static class ModuleLoader
{
    /// <summary>
    /// 从指定程序集清单中加载并配置所有模块
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="environment">主机环境</param>
    /// <param name="moduleAssemblies">模块程序集清单</param>
    public static void LoadModules(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        params Assembly[] moduleAssemblies)
    {
        var logger = CreateLogger(services);
        
        foreach (var assembly in moduleAssemblies)
        {
            logger.LogInformation("扫描模块程序集: {AssemblyName}", assembly.FullName);
            
            var bootstrapperTypes = assembly.GetTypes()
                .Where(t => typeof(IModuleBootstrapper).IsAssignableFrom(t) 
                    && t is { IsClass: true, IsAbstract: false })
                .ToList();

            foreach (var bootstrapperType in bootstrapperTypes)
            {
                try
                {
                    logger.LogInformation("发现模块启动器: {TypeName}", bootstrapperType.FullName);
                    
                    var bootstrapper = Activator.CreateInstance(bootstrapperType) as IModuleBootstrapper;
                    if (bootstrapper == null)
                    {
                        logger.LogWarning("无法创建模块启动器实例: {TypeName}", bootstrapperType.FullName);
                        continue;
                    }
                    
                    bootstrapper.Configure(services, configuration, environment);
                    logger.LogInformation("模块启动器配置成功: {TypeName}", bootstrapperType.FullName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "模块启动器配置失败: {TypeName}", bootstrapperType.FullName);
                    throw;
                }
            }
        }
    }

    private static ILogger CreateLogger(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        return loggerFactory?.CreateLogger(typeof(ModuleLoader)) 
            ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }
}
