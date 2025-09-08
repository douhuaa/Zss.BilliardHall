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

        var booksPermission = myGroup.AddPermission(BilliardHallPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(BilliardHallPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(BilliardHallPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(BilliardHallPermissions.Books.Delete, L("Permission:Books.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(BilliardHallPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BilliardHallResource>(name);
    }
}
