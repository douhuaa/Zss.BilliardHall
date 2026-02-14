namespace Zss.BilliardHall.Modules.Members;

/// <summary>
/// Members 模块启动器
/// 职责：会员管理模块的服务装配与 Marten Schema 配置
/// 冻结规范：实现 IModule（必须）和 IMartenModule（可选，如需扩展 Schema）
/// </summary>
public class MemberModule : IModule, IMartenModule
{
    public string Name => "Members";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Members 模块特定的服务注册
        // 例如：Validators, Policies, Custom Services 等
        // Wolverine 会自动发现此程序集中的 Handlers 和 Endpoints
    }

    public void ConfigureMarten(StoreOptions options)
    {
        // 注册 Members 模块的实体 Schema
        options.Schema.For<Member>()
            .UniqueIndex(x => x.Email);
    }
}
