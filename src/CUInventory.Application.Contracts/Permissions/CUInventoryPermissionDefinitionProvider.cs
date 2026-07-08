using CUInventory.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Permissions;

public class CUInventoryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(CUInventoryPermissions.GroupName);

        //Define your own permissions here. Example:
        //myGroup.AddPermission(CUInventoryPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CUInventoryResource>(name);
    }
}
