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
            var lft = _diff.LeftTableCheck;
            var rgt = _diff.RightTableCheck;
            
            sb.AppendLine($"ALTER TABLE [{rgt.Table.Schema.Name}].[{rgt.Table.Name}] DROP CONSTRAINT [{rgt.Name}]")
                .AppendGo()
                .AppendLine($"ALTER TABLE [{lft.Table.Schema.Name}].[{lft.Table.Name}] ADD CONSTRAINT [{lft.Name}] CHECK {lft.Definition}");
        }
    }
}
