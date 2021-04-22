using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffTableCheckDrop : BaseMssqlDiffBlock<DiffTableCheckDrop>
    {
        public MssqlDiffTableCheckDrop(DiffTableCheckDrop diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            if (_diff.TableCheck.IsSystemNamed)
            {
                var cleanSql = _diff.TableCheck.Definition.Replace("'", "''")
                    .Replace("(", "").Replace(")", "").Replace(" ", "");
                sb.Append($"EXEC #usp_DropUnnamedCheckConstraint '{_diff.TableCheck.Table.FullName}', '{cleanSql}'");
            }
            else
            {
                sb.AppendLine($"ALTER TABLE {_diff.TableCheck.Table.FullName} DROP CONSTRAINT [{_diff.TableCheck.Name}]");
            }
        }
    }
}
