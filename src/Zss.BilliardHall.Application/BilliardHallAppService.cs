using Zss.BilliardHall.Localization;

using Volo.Abp.Application.Services;

namespace Zss.BilliardHall;

/* Inherit your application services from this class.
 */
public abstract class BilliardHallAppService : ApplicationService
{
    protected BilliardHallAppService()
    {
        LocalizationResource = typeof(BilliardHallResource);
    }
}
