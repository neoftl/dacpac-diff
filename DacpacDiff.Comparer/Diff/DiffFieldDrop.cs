using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Diff
{
    public class DiffFieldDrop : IDifference, IDataLossChange
    {
        public FieldModel Field { get; }

        public IModel Model => Field;
        public string Title => "Drop table field";
        public string Name => $"[{Field.Table.Schema.Name}].[{Field.Table.Name}].[{Field.Name}]";

        public DiffFieldDrop(FieldModel field)
        {
            Field = field;
        }

        public override string ToString()
        {
            var sql = $"ALTER TABLE [{Field.Table.Schema.Name}].[{Field.Table.Name}] DROP COLUMN [{Field.Name}]";

            // TODO: ref

            return sql;
        }

        public bool GetDataLossTable(out string tableName)
        {
            tableName = Field.Table.FullName;
            return true;
        }
    }
}
