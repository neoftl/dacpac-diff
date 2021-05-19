using System;

namespace DacpacDiff.Core.Model
{
    public class TriggerModuleModel : ModuleModel
    {
        public TriggerModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.TRIGGER)
        {
        }
    }
}
