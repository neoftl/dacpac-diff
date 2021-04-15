using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffRefCreate : BaseMssqlDiffBlock<DiffRefCreate>
    {
        public MssqlDiffRefCreate(DiffRefCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            sb.AppendLine(new RefModel(_diff.Field).GetAddSql());
        }
    }
}
