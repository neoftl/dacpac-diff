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
            sb.Append($"ALTER TABLE {_diff.Ref.Table.FullName} WITH NOCHECK ADD ")
                .AppendIf(() => $"CONSTRAINT [{_diff.Ref.Name}] ", !_diff.Ref.IsSystemNamed)
                .AppendLine($"FOREIGN KEY ([{_diff.Ref.Field.Name}]) REFERENCES {_diff.Ref.TargetField.Table.FullName} ([{_diff.Ref.TargetField.Name}])");
        }
    }
}
