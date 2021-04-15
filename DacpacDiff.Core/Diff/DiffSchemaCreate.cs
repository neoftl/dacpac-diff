using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
{
    public class DiffSchemaCreate : IDifference
    {
        public SchemaModel Schema { get; }

        public IModel Model => Schema;
        public string Title => "Create schema";
        public string Name => $"[{Schema.Name}]";

        public DiffSchemaCreate(SchemaModel schema)
        {
            Schema = schema;
        }
    }
}
