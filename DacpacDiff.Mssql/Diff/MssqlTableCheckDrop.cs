using DacpacDiff.Core.Diff;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlTableCheckDrop : BaseMssqlDiffBlock<DiffTableCheckDrop>
    {
        public MssqlTableCheckDrop(DiffTableCheckDrop diff)
            : base(diff)
        { }

        protected override void GetFormat(StringBuilder sb, bool checkForDataLoss, bool prettyPrint)
        {
            if (_diff.TableCheck.IsSystemNamed)
            {
                sb.AppendLine($"DECLARE @DropConstraintSql VARCHAR(MAX) = (SELECT TOP 1 CONCAT('ALTER TABLE {_diff.TableCheck.Table.FullName} DROP CONSTRAINT [', [name], ']') FROM sys.check_constraints WHERE [parent_object_id] = OBJECT_ID('{_diff.TableCheck.Table.FullName}') AND [type] = 'C' AND [definition] = '{_diff.TableCheck.Definition}' AND [is_system_named] = 1); EXEC (@DropConstraintSql)");
            }
            else
            {
                sb.AppendLine($"ALTER TABLE {_diff.TableCheck.Table.FullName} DROP CONSTRAINT [{_diff.TableCheck.Name}]");
            }
        }
    }
}
