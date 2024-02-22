using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff;

public class MssqlDiffFieldAlter : BaseMssqlDiffBlock<DiffFieldAlter>
{
    public MssqlDiffFieldAlter(DiffFieldAlter diff)
        : base(diff)
    { }

    private static void appendFieldSql(FieldModel fld, ISqlFileBuilder sb, bool existingIsNull, bool asAlter)
    {
        sb.Append($"[{fld.Name}]");

        if (fld.Computation?.Length > 0)
        {
            sb.Append($" AS {fld.Computation}");
            return;
        }

        sb.Append($" {fld.Type}")
            .AppendIf(() => $" DEFAULT ({fld.DefaultValue})", fld.IsDefaultSystemNamed && !asAlter)
            .Append(!fld.Nullable && (!existingIsNull || fld.HasDefault) ? " NOT NULL" : " NULL");
    }

    protected override void GetFormat(ISqlFileBuilder sb)
    {
        var lft = _diff.LeftField;
        var rgt = _diff.RightField;

        // Change to/from computed column
        if ((lft.Computation?.Length > 0) != (rgt.Computation?.Length > 0))
        {
            sb.AppendLine(CommonMssql.ALTER_TABLE_DROP_COLUMN(rgt.Table.FullName, rgt.Name))
                .AppendLine()
                .Append($"ALTER TABLE {lft.Table.FullName} ADD ");
            appendFieldSql(lft, sb, false, false);
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
        var didDefault = false;
        if (!lft.IsSignatureMatch(rgt, false))
        {
            // Drop unnamed uniqueness
            if (rgt.IsUnique)
            {
                sb.Append("EXEC #usp_DropUnnamedUniqueConstraint ")
                    .Append($"'{rgt.Table.FullName}', ")
                    .Append($"'{rgt.Name}'")
                    .AppendGo();
                rgt.IsUnique = false;
            }

            // Alter
            sb.Append($"ALTER TABLE {lft.Table.FullName} ALTER COLUMN ");
            appendFieldSql(lft, sb, rgt.Nullable, true); // TODO: changing between nullability needs thinking about
            sb.AppendLine(!lft.Nullable && !lft.HasDefault && rgt.Nullable ? " -- NOTE: Cannot change to NOT NULL without default" : string.Empty)
                .AppendLine();
            didDefault = lft.IsDefaultSystemNamed;
        }

        // Default
        if (!lft.IsDefaultMatch(rgt) && lft.HasDefault && !didDefault)
        {
            // Add default
            sb.Append($"ALTER TABLE {lft.Table.FullName} ADD ")
                .AppendIf(() => $"CONSTRAINT [{lft.DefaultName}] ", !lft.IsDefaultSystemNamed)
                .AppendLine($"DEFAULT ({lft.DefaultValue}) FOR [{lft.Name}]");
        }

        // Make unique (drop is handled elsewhere)
        if (lft.IsUnique && !rgt.IsUnique)
        {
            sb.AppendLine($"ALTER TABLE {lft.Table.FullName} ADD UNIQUE ([{lft.Name}])");
        }

        // Reference
        var lftRef = lft.Ref != null ? new FieldRefModel(lft.Ref) : null;
        var rgtRef = rgt.Ref != null ? new FieldRefModel(rgt.Ref) : null;
        if (rgtRef is null || lftRef?.Equals(rgtRef) != true)
        {
            if (rgtRef != null)
            {
                if (rgtRef.IsSystemNamed || rgtRef.Name.Length == 0)
                {
                    sb.AppendLine($"-- Removing unnamed FKey: {rgtRef.Field.FullName} -> {rgtRef.TargetField.FullName}")
                        .AppendLine(CommonMssql.REF_GET_FKEYNAME(rgtRef.Table.FullName, rgtRef.Field.Name))
                        .AppendLine(CommonMssql.REF_GET_DROP_SQL(rgtRef.Table.FullName))
                        .AppendLine("EXEC (@DropConstraintSql)");
                }
                else
                {
                    sb.AppendLine(CommonMssql.ALTER_TABLE_DROP_CONSTRAINT(rgtRef.Table.FullName, rgtRef.Name));
                }
            }
            if (lftRef != null)
            {
                sb.Append($"ALTER TABLE {lftRef.Table.FullName} WITH NOCHECK ADD ")
                    .AppendIf(() => $"CONSTRAINT [{lftRef.Name}] ", !lftRef.IsSystemNamed)
                    .AppendLine($"FOREIGN KEY ([{lftRef.Field.Name}]) REFERENCES {lftRef.TargetField.Table.FullName} ([{lftRef.TargetField.Name}])");
            }
        }
    }
}
