using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using Zss.BilliardHall.Localization;

namespace Zss.BilliardHall.Web;

[Dependency(ReplaceServices = true)]
public class BilliardHallBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<BilliardHallResource> _localizer;

    public BilliardHallBrandingProvider(IStringLocalizer<BilliardHallResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
