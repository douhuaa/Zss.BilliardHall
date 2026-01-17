using Zss.BilliardHall.Wolverine.ServiceDefaults;

namespace Zss.BilliardHall.Wolverine.Bootstrapper;

/// <summary>
/// 平台层默认配置扩展
/// </summary>
public static class PlatformDefaults
{
    /// <summary>
    /// 添加平台层默认配置（基础设施、日志、健康检查等）
    /// </summary>
    public static WebApplicationBuilder AddPlatformDefaults(this WebApplicationBuilder builder)
    {
        // 1. Aspire Service Defaults（健康检查、OpenTelemetry、Serilog 等）
        builder.AddServiceDefaults();

        // 2. 其他平台级配置
        // builder.Services.AddAuthorization();
        // builder.Services.AddAuthentication();

        return builder;
    }
}
