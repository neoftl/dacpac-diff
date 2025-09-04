using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class ViewModuleModel(SchemaModel schema, string name)
    : ModuleWithBody(schema, name, ModuleType.VIEW)
{
    public override bool IsSimilarDefinition(ModuleModel other)
    {
        if (other is not ViewModuleModel vw)
        {
            return false;
        }

        return this.IsEqual(vw,
            m => m.Body.ScrubSQL());
    }
}
