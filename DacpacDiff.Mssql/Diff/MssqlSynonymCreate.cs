using DacpacDiff.Core.Diff;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlSynonymCreate : BaseMssqlDiffBlock<DiffSynonymCreate>
    {
        public MssqlSynonymCreate(DiffSynonymCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(StringBuilder sb, bool checkForDataLoss, bool prettyPrint)
        {
            sb.AppendLine($"CREATE SYNONYM {_diff.Name} FOR {_diff.Synonym.BaseObject}");
        }
    }
}
