using Zss.BilliardHall.Localization;

using Volo.Abp.AspNetCore.Components;

namespace Zss.BilliardHall.Blazor.Client;

public abstract class BilliardHallComponentBase : AbpComponentBase
{
    protected BilliardHallComponentBase()
    {
        LocalizationResource = typeof(BilliardHallResource);
    }
}
