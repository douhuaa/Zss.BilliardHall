using Volo.Abp.Modularity;
using Volo.Abp.Users;
using Volo.Abp.Localization;

namespace Zss.BilliardHall;

[DependsOn(
    typeof(AbpUsersDomainModule),
    typeof(AbpLocalizationModule)
)]
public class BilliardHallDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });
    }
}