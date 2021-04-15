using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffFieldCreate : BaseMssqlDiffBlock<DiffFieldCreate>
    {
        public MssqlDiffFieldCreate(DiffFieldCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            // TODO: ref
            // TODO: default
            // TODO: unique

            var fld = _diff.Field;
            sb.Append($"ALTER TABLE {fld.Table.FullName} ADD COLUMN [{fld.GetAlterSql()}]")
                .AppendIf(" -- NOTE: Cannot create NOT NULL column", !fld.Nullable && !fld.HasDefault)
                .AppendLine();
        }
    }
}
