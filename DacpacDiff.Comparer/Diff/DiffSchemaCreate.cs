using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Diff
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

        public override string ToString()
        {
            return $"CREATE SCHEMA {Name} AUTHORIZATION [dbo]";
        }
    }
}
