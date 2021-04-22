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
            var diffDrop = new MssqlDiffTableCheckDrop(new DiffTableCheckDrop(_diff.RightTableCheck));
            var diffCreate = new MssqlDiffTableCheckCreate(new DiffTableCheckCreate(_diff.LeftTableCheck));
            
            sb.Append(diffDrop.ToString())
                .AppendGo()
                .Append(diffCreate.ToString());
        }
    }
}
