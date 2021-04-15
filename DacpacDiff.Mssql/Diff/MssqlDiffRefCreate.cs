using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffRefCreate : BaseMssqlDiffBlock<DiffRefCreate>
    {
        public MssqlDiffRefCreate(DiffRefCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            var fref = _diff.Field.Ref;
            if (fref is null)
            {
                return;
            }

            sb.Append($"ALTER TABLE {fref.Table.FullName} WITH NOCHECK ADD ")
                .AppendIf($"CONSTRAINT [{fref.Name}] FOREIGN KEY ([{fref.Field}]) ", fref.IsSystemNamed)
                .AppendLine($"FOREIGN KEY ([{fref.Field}]) REFERENCES {fref.TargetTable} ([{fref.TargetField}])");
        }
    }
}
