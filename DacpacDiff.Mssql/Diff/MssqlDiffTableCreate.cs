using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System;
using System.Linq;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffTableCreate : BaseMssqlDiffBlock<DiffTableCreate>
    {
        public MssqlDiffTableCreate(DiffTableCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            sb.AppendLine($"CREATE TABLE {_diff.Table.FullName}")
                .AppendLine("(");

            var first = true;
            foreach (var fld in _diff.Table.Fields.OrderBy(f => f.Order))
            {
                sb.AppendIf("," + Environment.NewLine, !first);
                first = false;

                var ln = fld.GetTableFieldSql();
                sb.Append($"    {ln}");
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
