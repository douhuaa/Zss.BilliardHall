using Zss.BilliardHall.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Zss.BilliardHall.Permissions;

public class BilliardHallPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(BilliardHallPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(BilliardHallPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BilliardHallResource>(name);
    }
}
