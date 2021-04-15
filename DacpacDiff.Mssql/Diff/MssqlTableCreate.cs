using DacpacDiff.Core.Diff;
using System;
using System.Linq;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlTableCreate : BaseMssqlDiffBlock<DiffTableCreate>
    {
        public MssqlTableCreate(DiffTableCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(StringBuilder sb, bool checkForDataLoss, bool prettyPrint)
        {
            sb.AppendLine($"CREATE TABLE {_diff.Table.FullName}")
                .AppendLine("(");

            foreach (var fld in _diff.Table.Fields.OrderBy(f => f.Order))
            {
                var ln = fld.GetTableFieldSql();
                sb.AppendLine($"    {ln},");
            }
            if (_diff.Table.Fields.Length > 0)
            {
                sb.Remove(sb.Length - 3, 3);
            }

            if (_diff.Table.PrimaryKey.Length > 0)
            {
                sb.AppendLine(",")
                    .Append($"    PRIMARY KEY {(_diff.Table.IsPrimaryKeyUnclustered ? "NONCLUSTERED " : "")}([{String.Join("], [", _diff.Table.PrimaryKey)}])");
            }

            if (_diff.Table.Temporality != null)
            {
                sb.AppendLine(",")
                    .Append($"    PERIOD FOR SYSTEM_TIME ([{_diff.Table.Temporality.PeriodFieldFrom}], [{_diff.Table.Temporality.PeriodFieldTo}])")
                    .AppendLine()
                    .Append(") WITH (SYSTEM_VERSIONING = ON");
                if ((_diff.Table.Temporality.HistoryTable?.Length ?? 0) > 0)
                {
                    sb.Append($" (HISTORY_TABLE = {_diff.Table.Temporality.HistoryTable})");
                }
                sb.Append(')');
            }
            else
            {
                sb.AppendLine()
                    .Append(')');
            }

            // TODO: refs?
        }
    }
}
