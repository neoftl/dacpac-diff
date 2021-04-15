using DacpacDiff.Core.Diff;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlSchemaCreate : BaseMssqlDiffBlock<DiffSchemaCreate>
    {
        public MssqlSchemaCreate(DiffSchemaCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(StringBuilder sb, bool checkForDataLoss, bool prettyPrint)
        {
            sb.AppendLine($"CREATE SCHEMA {_diff.Name} AUTHORIZATION [dbo]");
        }
    }
}
