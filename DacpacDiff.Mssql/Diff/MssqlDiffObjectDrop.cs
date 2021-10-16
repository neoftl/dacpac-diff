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
                sb.Append($"DROP INDEX [{idx.Name}] ON {idx.IndexedObjectFullName}");
            }
            else if (_diff.Model is UniqueConstraintModel uqMod)
            {
                if (uqMod.IsSystemNamed)
                {
                    sb.Append("EXEC #usp_DropUnnamedUniqueConstraint ")
                        .Append($"'{uqMod.DefiningObjectFullName}', ")
                        .Append($"',{string.Join(",", uqMod.Columns)},'")
                        .AppendGo();
                }
                else
                {
                    sb.Append("ALTER TABLE ").Append(uqMod.DefiningObjectFullName)
                        .Append(" DROP CONSTRAINT ")
                        .AppendLine($"[{uqMod.Name}]")
                        .AppendGo();
                }
            }
            else
            {
                sb.Append($"DROP {_diff.Type} {_diff.Name}");
            }
        }
    }
}
