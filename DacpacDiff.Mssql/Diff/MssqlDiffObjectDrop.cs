using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffObjectDrop : BaseMssqlDiffBlock<DiffObjectDrop>
    {
        public MssqlDiffObjectDrop(DiffObjectDrop diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            if (_diff.Model is IndexModuleModel idx)
            {
                sb.Append($"DROP INDEX [{idx.Name}] ON {idx.IndexedObject}");
            }
            else
            {
                sb.Append($"DROP {_diff.Type} {_diff.Name}");
            }
        }
    }
}
