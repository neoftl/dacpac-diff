using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Comparer.Diff
{
    public class DiffTableCheckDrop : IDifference
    {
        public TableCheckModel TableCheck { get; }

        public IModel Model => TableCheck;
        public string Title => "Drop check constraint";
        public string Name => $"[{TableCheck.Table.Schema.Name}].[{TableCheck.Table.Name}].[{TableCheck.Name}]";

        public DiffTableCheckDrop(TableCheckModel tableCheck)
        {
            TableCheck = tableCheck ?? throw new ArgumentNullException(nameof(tableCheck));
        }

        public override string ToString()
        {
            if (TableCheck.IsSystemNamed)
            {
                return $"DECLARE @DropConstraintSql VARCHAR(MAX) = (SELECT TOP 1 CONCAT('ALTER TABLE {TableCheck.Table.FullName} DROP CONSTRAINT [', [name], ']') FROM sys.check_constraints WHERE [parent_object_id] = OBJECT_ID('{TableCheck.Table.FullName}') AND [type] = 'C' AND [definition] = '{TableCheck.Definition}' AND [is_system_named] = 1); EXEC (@DropConstraintSql)";
            }
            return $"ALTER TABLE {TableCheck.Table.FullName} DROP CONSTRAINT [{TableCheck.Name}]";
        }
    }
}
