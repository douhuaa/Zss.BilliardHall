﻿namespace Zss.BilliardHall.Modules.Orders;

/// <summary>
/// Orders 模块启动器
/// 职责：订单管理模块的服务装配
/// 冻结规范：实现 IModule（必须）
/// </summary>
public class OrderModule : IModule
{
    public string Name => "Orders";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Orders 模块特定的服务注册
    }
}
