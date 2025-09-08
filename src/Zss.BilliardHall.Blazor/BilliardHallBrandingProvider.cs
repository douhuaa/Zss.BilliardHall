using Microsoft.Extensions.Localization;

using Zss.BilliardHall.Localization;

using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Zss.BilliardHall.Blazor;

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
