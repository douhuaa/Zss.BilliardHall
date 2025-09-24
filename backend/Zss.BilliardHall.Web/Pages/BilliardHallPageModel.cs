using Zss.BilliardHall.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Zss.BilliardHall.Web.Pages;

public abstract class BilliardHallPageModel : AbpPageModel
{
    protected BilliardHallPageModel()
    {
        LocalizationResourceType = typeof(BilliardHallResource);
    }
}
