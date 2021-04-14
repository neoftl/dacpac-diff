using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
{
    public class DiffFieldCreate : IDifference
    {
        public FieldModel Field { get; }

        public IModel Model => Field;
        public string Title => "Add table field";
        public string Name => $"[{Field.Table.Schema.Name}].[{Field.Table.Name}].[{Field.Name}]";

        public DiffFieldCreate(FieldModel field)
        {
            Field = field;
        }

        public override string ToString()
        {
            var sql = $"ALTER TABLE [{Field.Table.Schema.Name}].[{Field.Table.Name}] ADD {Field.GetAlterSql()}";

            if (!Field.Nullable && !Field.HasDefault)
            {
                return sql + " -- NOTE: Cannot create NOT NULL column";
            }

            // TODO: ref
            // TODO: default
            // TODO: unique

            return sql;
        }
    }
}
