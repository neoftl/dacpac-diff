using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffFieldDrop : BaseMssqlDiffBlock<DiffFieldDrop>
    {
        public MssqlDiffFieldDrop(DiffFieldDrop diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            // TODO: ref
         
            var fld = _diff.Field;
            sb.AppendLine($"ALTER TABLE {fld.Table.FullName} DROP COLUMN [{fld.Name}]");
        }
    }
}
