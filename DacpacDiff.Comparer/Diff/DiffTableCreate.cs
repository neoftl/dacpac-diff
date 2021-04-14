using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Linq;
using System.Text;

namespace DacpacDiff.Comparer.Diff
{
    public class DiffTableCreate : IDifference
    {
        public TableModel Table { get; }

        public IModel Model => Table;
        public string Title => "Create table";
        public string Name => Table.FullName;

        public DiffTableCreate(TableModel table)
        {
            Table = table;

            foreach (var fld in table.Fields ?? new FieldModel[0])
            {
                fld.SetState(table, null);
            }
        }

        public override string ToString()
        {
            var sql = new StringBuilder();

            foreach (var fld in (Table.Fields ?? new FieldModel[0]).OrderBy(f => f.Order))
            {
                var ln = fld.GetTableFieldSql();
                sql.Append($",\r\n    {ln}");
            }

            if (sql.Length > 0) sql.Remove(0, 3);
            sql.Insert(0, $"CREATE TABLE {Table.FullName}\r\n(\r\n");

            if ((Table.PrimaryKey?.Length ?? 0) > 0)
            {
                sql.Append($",\r\n    PRIMARY KEY {(Table.IsPrimaryKeyUnclustered ? "NONCLUSTERED " : "")}([{String.Join("], [", Table.PrimaryKey)}])");
            }

            if (Table.Temporality != null)
            {
                sql.Append($",\r\n    PERIOD FOR SYSTEM_TIME ([{Table.Temporality.PeriodFieldFrom}], [{Table.Temporality.PeriodFieldTo}])")
                    .Append("\r\n) WITH (SYSTEM_VERSIONING = ON");
                if ((Table.Temporality.HistoryTable?.Length ?? 0) > 0)
                {
                    sql.Append($" (HISTORY_TABLE = {Table.Temporality.HistoryTable})");
                }
                sql.Append(")");
            }
            else
            {
                sql.Append("\r\n)");
            }

            // TODO: refs?

            return sql.ToString();
        }
    }
}
