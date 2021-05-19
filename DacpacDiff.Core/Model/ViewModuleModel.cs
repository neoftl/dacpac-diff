using System;

namespace DacpacDiff.Core.Model
{
    public class ViewModuleModel : ModuleModel
    {
        public ViewModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.VIEW)
        {
        }
    }
}
