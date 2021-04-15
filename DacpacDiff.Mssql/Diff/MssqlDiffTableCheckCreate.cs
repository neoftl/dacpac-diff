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
            sb.Append($"ALTER TABLE {_diff.TableCheck.Table.FullName} ")
                .AppendIf($"ADD CONSTRAINT [{_diff.TableCheck.Name}] ", !_diff.TableCheck.IsSystemNamed)
                .AppendLine($"ADD CHECK {_diff.TableCheck.Definition}");
        }
    }
}
