using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff;

public class MssqlDiffObjectDrop(DiffObjectDrop diff)
    : BaseMssqlDiffBlock<DiffObjectDrop>(diff)
{
    protected override void GetFormat(ISqlFileBuilder sb)
    {
        if (_diff.Model is IndexModuleModel idx)
        {
            sb.Append($"DROP INDEX [{idx.Name}] ON {idx.IndexedObjectFullName}");
        }
        else if (_diff.Model is UniqueConstraintModel uqMod)
        {
            if (uqMod.IsSystemNamed)
            {
                sb.DROP_UNNAMED_UNIQUE(uqMod.DefiningObjectFullName, string.Join(",", uqMod.Columns));
            }
            else
            {
                sb.Append("ALTER TABLE ").Append(uqMod.DefiningObjectFullName)
                    .Append(" DROP CONSTRAINT ")
                    .Append($"[{uqMod.Name}]")
                    .AppendGo();
            }
        }
        else
        {
            if (_diff.Model is TableModel tbl && tbl.Temporality != TemporalityModel.Empty)
            {
                sb.AppendLine($"ALTER TABLE {tbl.FullName} SET (SYSTEM_VERSIONING = OFF)");
                sb.AppendLine($"DROP TABLE {tbl.Temporality.HistoryTable}");
            }

            sb.Append($"DROP {_diff.Type} {_diff.Name}");
        }
    }
}
