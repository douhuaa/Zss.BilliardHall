using Marten;
using Oakton.Resources;
using Wolverine;

namespace Zss.BilliardHall.Wolverine.ServiceDefaults;

/// <summary>
/// Wolverine 框架配置扩展
/// </summary>
public static class WolverineExtensions
{
    /// <summary>
    /// 添加 Wolverine 默认配置（消息处理、HTTP 端点、事务、Outbox 等）
    /// </summary>
    public static WebApplicationBuilder AddWolverineDefaults(
        this WebApplicationBuilder builder,
        Action<WolverineOptions>? configure = null)
    {
        builder.Host.UseWolverine(options =>
        {
            // 1. HTTP 端点自动发现（扫描所有标记 [WolverinePost] 等特性的端点）
            options.Discovery.IncludeAssembly(typeof(WolverineExtensions).Assembly);

            // 2. FluentValidation 集成（自动验证消息）
            options.UseFluentValidation();

            // 3. 自定义配置回调
            configure?.Invoke(options);
        });

        // 4. Wolverine 资源注册（用于 Oakton 启动检查）
        builder.Services.AddResourceSetupOnStartup();

        return builder;
    }

    /// <summary>
    /// 添加 Marten + Wolverine 集成（文档数据库 + Outbox 模式）
    /// </summary>
    public static WebApplicationBuilder AddMartenWithWolverine(this WebApplicationBuilder builder)
    {
        // Marten 配置已在 MartenExtensions 中定义，这里只需集成 Wolverine
        builder.Services.AddMarten(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("PostgreSQL connection string is required");

            options.Connection(connectionString);
            options.DatabaseSchemaName = "billiard_hall";
        })
        .IntegrateWithWolverine()  // 集成 Wolverine Outbox 模式
        .UseLightweightSessions(); // 使用轻量级会话（性能优化）

        return builder;
    }
}
