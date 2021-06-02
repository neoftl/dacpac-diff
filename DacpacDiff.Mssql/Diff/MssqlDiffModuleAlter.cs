using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffModuleAlter : BaseMssqlDiffBlock<DiffModuleAlter>
    {
        public MssqlDiffModuleAlter(DiffModuleAlter diff)
            : base(diff)
        {
        }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            var sql = new MssqlDiffModuleCreate(new DiffModuleCreate(_diff.Module)).ToString();

            sql.TryMatch(@"(?im)^CREATE\s", out var m);
            sql = sql[0..m.Index] + "ALTER " + sql[(m.Index + m.Length)..];

            sb.Append(sql).EnsureLine();
        }
    }
}