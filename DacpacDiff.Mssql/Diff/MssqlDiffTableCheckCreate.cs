using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffTableCheckCreate : BaseMssqlDiffBlock<DiffTableCheckCreate>
    {
        public MssqlDiffTableCheckCreate(DiffTableCheckCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            sb.Append($"ALTER TABLE {_diff.TableCheck.Table.FullName} ADD ")
                .AppendIf($"CONSTRAINT [{_diff.TableCheck.Name}] ", !_diff.TableCheck.IsSystemNamed)
                .AppendLine($"CHECK ({_diff.TableCheck.Definition})");
        }
    }
}
