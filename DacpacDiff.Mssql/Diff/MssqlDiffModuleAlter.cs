using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

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
            var sql = new MssqlDiffModuleCreate(new DiffModuleCreate(_diff.Module))
            {
                DoAsAlter = true,
                UseStub = false
            }.ToString();

            sb.Append(sql).EnsureLine();
        }
    }
}