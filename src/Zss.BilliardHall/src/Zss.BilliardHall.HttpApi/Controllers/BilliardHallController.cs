using Zss.BilliardHall.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Zss.BilliardHall.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class BilliardHallController : AbpControllerBase
{
    protected BilliardHallController()
    {
        LocalizationResource = typeof(BilliardHallResource);
    }
}
