using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffFieldCreate : BaseMssqlDiffBlock<DiffFieldCreate>
    {
        public MssqlDiffFieldCreate(DiffFieldCreate diff)
            : base(diff)
        { }

        private static void appendFieldSql(FieldModel fld, ISqlFileBuilder sb)
        {
            sb.Append($"[{fld.Name}]");

            if ((fld.Computation?.Length ?? 0) > 0)
            {
                sb.Append($" AS {fld.Computation}");
                return;
            }

            sb.Append($" {fld.Type}")
                .AppendIf($" DEFAULT ({fld.DefaultValue})", fld.HasDefault)
                .Append(!fld.Nullable && fld.HasDefault ? " NOT NULL" : " NULL")
                .AppendIf($" REFERENCES {fld.Ref?.TargetField.Table.FullName} ([{fld.Ref?.TargetField.Name}])", fld.Ref?.IsSystemNamed == true);
        }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            // TODO: unique

            var fld = _diff.Field;
            sb.Append($"ALTER TABLE {fld.Table.FullName} ADD ");
            appendFieldSql(fld, sb);
            sb.AppendIf(" -- NOTE: Cannot create NOT NULL column", !fld.Nullable && !fld.HasDefault)
                .AppendLine();
        }
    }
}
