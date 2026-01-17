using System.Reflection;
using Zss.BilliardHall.Wolverine.ServiceDefaults;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

/// <summary>
/// 应用模块注册扩展
/// </summary>
public static class ApplicationModules
{
    /// <summary>
    /// 注册所有应用模块（Wolverine + Marten + 业务模块）
    /// </summary>
    public static WebApplicationBuilder AddApplicationModules(this WebApplicationBuilder builder)
    {
        // 1. Wolverine 框架配置（消息处理、端点发现等）
        builder.AddWolverineDefaults(options =>
        {
            // 扫描当前程序集及所有 Modules 目录下的程序集
            options.Discovery.IncludeAssembly(Assembly.GetExecutingAssembly());
            
            // 自动发现所有模块程序集（Members, Sessions, Tables 等）
            var modulesPath = Path.Combine(AppContext.BaseDirectory, "Modules");
            if (Directory.Exists(modulesPath))
            {
                var moduleAssemblies = Directory.GetFiles(modulesPath, "*.dll")
                    .Select(Assembly.LoadFrom);
                
                foreach (var assembly in moduleAssemblies)
                {
                    options.Discovery.IncludeAssembly(assembly);
                }
            }
        });

        // 2. Marten 文档数据库配置（集成 Wolverine Outbox）
        builder.AddMartenWithWolverine();

        // 3. 其他基础设施服务注册
        // builder.Services.AddScoped<IClock, SystemClock>();
        // builder.Services.AddScoped<IEmailService, EmailService>();

        return builder;
    }
}
