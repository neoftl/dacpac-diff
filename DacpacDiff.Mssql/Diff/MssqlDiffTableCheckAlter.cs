using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffTableCheckAlter : BaseMssqlDiffBlock<DiffTableCheckAlter>
    {
        public MssqlDiffTableCheckAlter(DiffTableCheckAlter diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            var diffDrop = new MssqlDiffTableCheckDrop(new DiffTableCheckDrop(_diff.CurrentTableCheck));
            var diffCreate = new MssqlDiffTableCheckCreate(new DiffTableCheckCreate(_diff.TargetTableCheck));
            
            sb.Append(diffDrop.ToString())
                .EnsureLine(2)
                .Append(diffCreate.ToString());
        }
    }
}
