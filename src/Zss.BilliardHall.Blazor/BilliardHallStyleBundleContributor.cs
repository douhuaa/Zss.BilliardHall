using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

namespace Zss.BilliardHall.Blazor;

public class BilliardHallStyleBundleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        context.Files.Add(new BundleFile("main.css", true));
    }
}
