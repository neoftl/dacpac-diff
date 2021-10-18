using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffSynonymAlter : BaseMssqlDiffBlock<DiffSynonymAlter>
    {
        public MssqlDiffSynonymAlter(DiffSynonymAlter diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            sb.AppendLine($"DROP SYNONYM {_diff.Name}")
                .AppendGo()
                .AppendLine($"CREATE SYNONYM {_diff.Name} FOR {_diff.Synonym.BaseObject}");
        }
    }
}
