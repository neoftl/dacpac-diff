namespace DacpacDiff.Core.Model
{
    public class TriggerModuleModel : ModuleModel
    {
        public string Parent { get; set; } = string.Empty;

        /// <summary>
        /// True if a BEFORE (INSTEAD OF) trugger, else AFTER
        /// </summary>
        public bool Before { get; set; }

        public bool ForDelete { get; set; }
        public bool ForInsert { get; set; }
        public bool ForUpdate { get; set; }

        public TriggerModuleModel(SchemaModel schema, string name)
            : base(schema, name, ModuleType.TRIGGER)
        {
        }
    }
}
