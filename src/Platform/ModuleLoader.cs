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
        // 添加日志服务（如果尚未添加）
        services.AddLogging();
        
        foreach (var assembly in moduleAssemblies)
        {
            // 使用 Console 日志临时记录，避免创建 ServiceProvider
            Console.WriteLine($"[ModuleLoader] 扫描模块程序集: {assembly.FullName}");
            
            var bootstrapperTypes = assembly.GetTypes()
                .Where(t => typeof(IModuleBootstrapper).IsAssignableFrom(t) 
                    && t is { IsClass: true, IsAbstract: false })
                .ToList();

            foreach (var bootstrapperType in bootstrapperTypes)
            {
                try
                {
                    Console.WriteLine($"[ModuleLoader] 发现模块启动器: {bootstrapperType.FullName}");
                    
                    var bootstrapper = Activator.CreateInstance(bootstrapperType) as IModuleBootstrapper;
                    if (bootstrapper == null)
                    {
                        Console.WriteLine($"[ModuleLoader] 警告: 无法创建模块启动器实例: {bootstrapperType.FullName}");
                        continue;
                    }
                    
                    bootstrapper.Configure(services, configuration, environment);
                    Console.WriteLine($"[ModuleLoader] 模块启动器配置成功: {bootstrapperType.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ModuleLoader] 错误: 模块启动器配置失败: {bootstrapperType.FullName} - {ex.Message}");
                    throw;
                }
            }
        }
    }
}
