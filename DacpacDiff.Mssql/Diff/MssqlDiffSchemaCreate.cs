using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffSchemaCreate : BaseMssqlDiffBlock<DiffSchemaCreate>
    {
        public MssqlDiffSchemaCreate(DiffSchemaCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            sb.AppendLine($"CREATE SCHEMA {_diff.Name} AUTHORIZATION [dbo]");
        }
    }
}
