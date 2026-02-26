using Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTransformers;
using Zss.BilliardHall.Platform.Infrastructure;

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
        // 注册会员模块的异常转换器（Wolverine pipeline 层，PostgresException → DomainException）
        services.AddSingleton<IPostgresExceptionTransformer, MemberEmailUniqueViolationTransformer>();
        services.AddSingleton<IPostgresExceptionTransformer, MemberPhoneNumberUniqueViolationTransformer>();
    }

    public void ConfigureMarten(StoreOptions options)
    {
        // 注册 Members 模块的实体 Schema
        options.Schema.For<Member>()
            .UniqueIndex(x => x.Email)
            .Index(x => x.PhoneNumber, idx =>
            {
                idx.IsUnique = true;
                idx.Name = "mt_doc_member_uidx_phonenumber";
                // 手机号为可选字段，仅对非空值建立唯一索引
                idx.Predicate = "(data ->> 'PhoneNumber') is not null";
            });
    }
}
