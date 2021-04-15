using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffSynonymCreate : BaseMssqlDiffBlock<DiffSynonymCreate>
    {
        public MssqlDiffSynonymCreate(DiffSynonymCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            sb.AppendLine($"CREATE SYNONYM {_diff.Name} FOR {_diff.Synonym.BaseObject}");
        }
    }
}
