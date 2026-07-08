using Volo.Abp.Settings;

namespace CUInventory.Settings;

public class CUInventorySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(CUInventorySettings.MySetting1));
    }
}
