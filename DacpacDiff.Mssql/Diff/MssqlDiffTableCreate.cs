using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
using System.Linq;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffTableCreate : BaseMssqlDiffBlock<DiffTableCreate>
    {
        public MssqlDiffTableCreate(DiffTableCreate diff)
            : base(diff)
        { }

        private static void appendFieldSql(FieldModel fld, ISqlFileBuilder sb)
        {
            sb.Append($"[{fld.Name}]");

            if ((fld.Computation?.Length ?? 0) > 0)
            {
                sb.Append($" AS {fld.Computation}");
            }
            else
            {
                sb.Append($" {fld.Type}");

                if (fld.Table.Temporality?.PeriodFieldFrom == fld.Name)
                {
                    sb.Append(" GENERATED ALWAYS AS ROW START");
                    return;
                }
                if (fld.Table.Temporality?.PeriodFieldTo == fld.Name)
                {
                    sb.Append(" GENERATED ALWAYS AS ROW END");
                    return;
                }

                sb.Append(!fld.Nullable ? " NOT NULL" : " NULL")
                    .AppendIf(() => $" DEFAULT ({fld.DefaultValue})", fld.HasDefault)
                    .AppendIf(() => $" CONSTRAINT [{fld.Table.PrimaryKeyName}]", fld.IsPrimaryKey && fld.Table.PrimaryKeys.Length == 1 && !fld.Table.IsPrimaryKeySystemNamed)
                    .AppendIf(() => " PRIMARY KEY", fld.IsPrimaryKey && fld.Table.PrimaryKeys.Length == 1 && !fld.Table.IsPrimaryKeyUnclustered)
                    .AppendIf(() => " IDENTITY(1,1)", fld.Identity);
            }

            sb.AppendIf(() => " UNIQUE", fld.IsUnique)
                .AppendIf(() => $" CONSTRAINT [{fld.Ref?.Name}]", fld.Ref?.IsSystemNamed == false)
                .AppendIf(() => $" REFERENCES {fld.Ref?.TargetField.Table.FullName} ([{fld.Ref?.TargetField.Name}])", fld.Ref != null);
        }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            sb.AppendLine($"CREATE TABLE {_diff.Table.FullName}")
                .Append("(");

            var first = true;
            foreach (var fld in _diff.Table.Fields.OrderBy(f => f.Order))
            {
                sb.AppendIf(() => ",", !first)
                    .AppendLine()
                    .Append("    ");
                appendFieldSql(fld, sb);
                first = false;
            }

            if (_diff.Table.PrimaryKeys.Length > 1 || _diff.Table.IsPrimaryKeyUnclustered)
            {
                // TODO: Named primary key
                sb.AppendLine(",")
                    .Append("    ")
                    .AppendIf(() => $"CONSTRAINT [{_diff.Table.PrimaryKeyName}] ", !_diff.Table.IsPrimaryKeySystemNamed)
                    .Append("PRIMARY KEY ")
                    .AppendIf(() => "NONCLUSTERED ", _diff.Table.IsPrimaryKeyUnclustered)
                    .Append($"([{string.Join("], [", _diff.Table.PrimaryKeys.Select(f => f.Name))}])");
            }

            if (_diff.Table.Temporality.PeriodFieldFrom != null)
            {
                sb.AppendLine(",")
                    .AppendLine($"    PERIOD FOR SYSTEM_TIME ([{_diff.Table.Temporality.PeriodFieldFrom}], [{_diff.Table.Temporality.PeriodFieldTo}])")
                    .Append(") WITH (SYSTEM_VERSIONING = ON")
                    .AppendIf(() => $" (HISTORY_TABLE = {_diff.Table.Temporality.HistoryTable})", (_diff.Table.Temporality.HistoryTable?.Length ?? 0) > 0)
                    .Append(')');
            }
            else
            {
                sb.AppendLine()
                    .Append(')');
            }
        }
    }
}
