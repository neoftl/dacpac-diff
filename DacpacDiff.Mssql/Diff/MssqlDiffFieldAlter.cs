using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlFieldAlter : BaseMssqlDiffBlock<DiffFieldAlter>
    {
        public MssqlFieldAlter(DiffFieldAlter diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            var lft = _diff.LeftField;
            var rgt = _diff.RightField;

            // TODO: If field has dependencies (constraint, index, etc.), drop first then recreate (in the comparer)

            if ((lft.Computation?.Length ?? 0) > 0)
            {
                sb.AppendLine($"ALTER TABLE {rgt.Table.FullName} DROP COLUMN [{rgt.Name}]")
                    .AppendLine("GO")
                    .AppendLine($"ALTER TABLE {lft.Table.FullName} ADD {lft.GetAlterSql()}")
                    .AppendLine("GO");
            }
            
            // TODO
            //// Cannot modify PKey
            //if (sql.Count > 0 && rgt?.Table?.PrimaryKey?.Contains(rgt?.Name) == true)
            //{
            //    // TODO: Need to replace whole table via temp
            //    sql.Insert(0, " NOTE: Cannot modify Primary Key column");
            //    sql = sql.Select(s => "--" + s).ToList();
            //}

            // Main definition
            var alterSql = lft.GetAlterSql(!rgt.Nullable);
            if (alterSql != rgt.GetAlterSql(!rgt.Nullable))
            {
                sb.Append($"ALTER TABLE {lft.Table.FullName} ALTER COLUMN {alterSql}")
                    .AppendLine(!lft.Nullable && !lft.HasDefault && rgt.Nullable ? " -- NOTE: Cannot change to NOT NULL column" : string.Empty)
                    .AppendLine("GO");
            }

            // Default
            if (!lft.IsDefaultMatch(rgt))
            {
                if (rgt.HasDefault)
                {
                    // Remove default
                    if (!rgt.IsSystemNamedDefault)
                    {
                        sb.AppendLine($"ALTER TABLE {rgt.Table.FullName} DROP CONSTRAINT [{rgt.DefaultName}]");
                    }
                    else
                    {
                        sb.AppendLine($"DECLARE @DropConstraintSql VARCHAR(MAX) = (SELECT CONCAT('ALTER TABLE {rgt.Table.FullName} DROP CONSTRAINT [', [name], ']') FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('{rgt.Table.FullName}') AND parent_column_id = {rgt.Order - 1}); EXEC (@DropConstraintSql)");
                    }
                }
                if (lft.HasDefault)
                {
                    // Add default
                    sb.Append($"ALTER TABLE {lft.Table.FullName} ADD ")
                        .AppendIf($"CONSTRAINT [{lft.DefaultName}] ", !lft.IsSystemNamedDefault)
                        .AppendLine($"DEFAULT{lft.DefaultValue} FOR [{lft.Name}]");
                }
            }

            // Unique
            if (lft.IsUnique != rgt.IsUnique)
            {
                if (lft.IsUnique)
                {
                    // Make unique
                    sb.Append($"ALTER TABLE {lft.Table.FullName} ADD ")
                        .AppendIf($"CONSTRAINT [{lft.Unique}] ", !lft.IsSystemNamedUnique)
                        .AppendLine($"UNIQUE ([{lft.Name}]");
                }
                else
                {
                    // Make non-unique
                    if (!rgt.IsSystemNamedUnique)
                    {
                        sb.AppendLine($"ALTER TABLE {lft.Table.FullName} DROP CONSTRAINT [{rgt.DefaultName}]");
                    }
                    else
                    {
                        // TODO
                    }
                }
            }

            // Reference
            var lftRef = lft.Ref is not null ? new RefModel(lft) : null;
            var rgtRef = rgt.Ref is not null ? new RefModel(rgt) : null;
            if (rgtRef is null || lftRef?.Equals(rgtRef) != false)
            {
                if (rgtRef is not null)
                {
                    sb.AppendLine(rgtRef.GetDropSql());
                }
                if (lftRef is not null)
                {
                    sb.AppendLine(lftRef.GetAddSql());
                }
            }
        }
    }
}
