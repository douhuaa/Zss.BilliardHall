namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// Members 模块启动器
/// 负责注册模块内的服务和配置
/// </summary>
public class MemberModule : IModule,IMartenModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Members 模块特定的服务注册
        // 例如：validators, custom services 等
        // Wolverine 会自动发现 Handlers 和 Endpoints
    }
    public void ConfigureMarten(StoreOptions options)
    {
        options.Schema.For<Member>()
            .UniqueIndex(x => x.Email);
    }
}
