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

        private static void appendFieldSql(FieldModel fld, ISqlFileBuilder sb)
        {
            sb.Append($"[{fld.Name}]");

            if ((fld.Computation?.Length ?? 0) > 0)
            {
                sb.Append($" AS {fld.Computation}");
                return;
            }

            sb.Append($" {fld.Type}")
                .AppendIf($" DEFAULT ({fld.DefaultValue})", fld.HasDefault && !fld.IsDefaultSystemNamed)
                .Append(!fld.Nullable && fld.HasDefault ? " NOT NULL" : " NULL");
        }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            var lft = _diff.LeftField;
            var rgt = _diff.RightField;

            if ((lft.Computation?.Length ?? 0) > 0)
            {
                sb.AppendLine($"ALTER TABLE {rgt.Table.FullName} DROP COLUMN [{rgt.Name}]")
                    .AppendLine()
                    .Append($"ALTER TABLE {lft.Table.FullName} ADD ");
                appendFieldSql(lft, sb);
                sb.EnsureLine();
                return;
            }

            // TODO
            //// Cannot modify PKey
            //if (sql.Count > 0 && rgt?.Table?.PrimaryKey?.Contains(rgt?.Name) == true)
            //{
            //    // TODO: Need to replace whole table via temp
            //    sql.Insert(0, " NOTE: Cannot modify Primary Key column");
            //    sql = sql.Select(s => "--" + s).ToList();
            //}

            // Remove existing default
            if (!lft.IsDefaultMatch(rgt) && rgt.HasDefault)
            {
                // Remove default
                if (rgt.IsDefaultSystemNamed)
                {
                    sb.AppendLine($"EXEC #usp_DropUnnamedDefault '{rgt.Table.FullName}', '{rgt.Name}'");
                }
                else
                {
                    sb.AppendLine($"ALTER TABLE {rgt.Table.FullName} DROP CONSTRAINT [{rgt.DefaultName}]");
                }
                sb.AppendLine();
            }

            // Main definition
            if (!lft.IsSignatureMatch(rgt))
            {
                sb.Append($"ALTER TABLE {lft.Table.FullName} ALTER COLUMN ");
                appendFieldSql(lft, sb); // TODO: changing between nullability needs thinking about
                sb.AppendLine(!lft.Nullable && !lft.HasDefault && rgt.Nullable ? " -- NOTE: Cannot change to NOT NULL column" : string.Empty)
                    .AppendLine();
            }

            // Default
            if (!lft.IsDefaultMatch(rgt) && lft.HasDefault)
            {
                // Add default
                sb.Append($"ALTER TABLE {lft.Table.FullName} ADD ")
                    .AppendIf($"CONSTRAINT [{lft.DefaultName}] ", !lft.IsDefaultSystemNamed)
                    .AppendLine($"DEFAULT ({lft.DefaultValue}) FOR [{lft.Name}]");
            }

            // Unique
            if (lft.IsUnique != rgt.IsUnique)
            {
                if (lft.IsUnique)
                {
                    // Make unique
                    sb.Append($"ALTER TABLE {lft.Table.FullName} ADD ")
                        .AppendLine($"UNIQUE ([{lft.Name}])");
                }
            }

            // Reference
            var lftRef = lft.Ref is not null ? new FieldRefModel(lft.Ref) : null;
            var rgtRef = rgt.Ref is not null ? new FieldRefModel(rgt.Ref) : null;
            if (rgtRef is null || lftRef?.Equals(rgtRef) != false)
            {
                if (rgtRef is not null)
                {
                    if (!rgtRef.IsSystemNamed || (rgtRef.Name?.Length ?? 0) == 0)
                    {
                        sb.AppendLine($"-- Removing unnamed FKey: {rgtRef.Field.FullName} -> {rgtRef.TargetField.FullName}")
                            .AppendLine($"DECLARE @FKeyName VARCHAR(MAX) = (SELECT FK.[name] FROM sys.foreign_keys FK JOIN sys.foreign_key_columns KC ON KC.[constraint_object_id] = FK.[object_id] JOIN sys.columns C ON C.[object_id] = FK.[parent_object_id] AND C.[column_id] = KC.[parent_column_id] WHERE FK.[parent_object_id] = OBJECT_ID('{rgtRef.Table.FullName}') AND FK.[type] = 'F' AND C.[name] = '{rgtRef.Field}')") // TODO: Shouldn't this check the target as well?
                            .AppendLine($"DECLARE @DropConstraintSql VARCHAR(MAX) = CONCAT('ALTER TABLE {rgtRef.Table.FullName} DROP CONSTRAINT ', QUOTENAME(@FKeyName))")
                            .AppendLine("EXEC (@DropConstraintSql)");
                    }
                    else
                    {
                        sb.AppendLine($"ALTER TABLE {rgtRef.Table.FullName} DROP CONSTRAINT [{rgtRef.Name}]");
                    }
                }
                if (lftRef is not null)
                {
                    sb.Append($"ALTER TABLE {lftRef.Table.FullName} WITH NOCHECK ADD ")
                        .AppendIf($"CONSTRAINT [{lftRef.Name}] FOREIGN KEY ([{lftRef.Field}]) ", lftRef.IsSystemNamed)
                        .AppendLine($"FOREIGN KEY ([{lftRef.Field}]) REFERENCES {lftRef.TargetField.Table.FullName} ([{lftRef.TargetField.Name}])");
                }
            }
        }
    }
}
