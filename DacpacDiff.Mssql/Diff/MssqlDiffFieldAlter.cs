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
        var tgt = _diff.TargetField;
        var cur = _diff.CurrentField;

        // Change to/from computed column
        if ((tgt.Computation?.Length > 0) != (cur.Computation?.Length > 0))
        {
            sb.AppendLine(CommonMssql.ALTER_TABLE_DROP_COLUMN(cur.Table.FullName, cur.Name))
                .AppendLine()
                .Append($"ALTER TABLE {tgt.Table.FullName} ADD ");
            appendFieldSql(tgt, sb, false, false);
            sb.EnsureLine();
            return;
        }

        // TODO
        //// Cannot modify PKey
        //if (sql.Count > 0 && cur?.Table?.PrimaryKey?.Contains(cur?.Name) == true)
        //{
        //    // TODO: Need to replace whole table via temp
        //    sql.Insert(0, " NOTE: Cannot modify Primary Key column");
        //    sql = sql.Select(s => "--" + s).ToList();
        //}

        // Remove existing default
        if (!tgt.IsDefaultMatch(cur) && cur.HasDefault)
        {
            // Remove default
            if (cur.IsDefaultSystemNamed)
            {
                sb.AppendLine($"EXEC #usp_DropUnnamedDefault '{cur.Table.FullName}', '{cur.Name}'");
            }
            else
            {
                sb.AppendLine($"ALTER TABLE {cur.Table.FullName} DROP CONSTRAINT [{cur.DefaultName}]");
            }
            sb.AppendLine();
        }

        // Main definition
        if (!tgt.IsSignatureMatch(cur, false))
        {
            // Drop unnamed uniqueness
            if (cur.IsUnique)
            {
                sb.Append("EXEC #usp_DropUnnamedUniqueConstraint ")
                    .Append($"'{cur.Table.FullName}', ")
                    .Append($"'{cur.Name}'")
                    .AppendGo();
                cur.IsUnique = false;
            }

            // Alter
            sb.Append($"ALTER TABLE {tgt.Table.FullName} ALTER COLUMN ");
            appendFieldSql(tgt, sb, cur.Nullable, true); // TODO: changing between nullability needs thinking about
            sb.AppendLine(!tgt.Nullable && !tgt.HasDefault && cur.Nullable ? " -- NOTE: Cannot change to NOT NULL without default" : string.Empty)
                .AppendLine();
        }

        // Default
        if (!tgt.IsDefaultMatch(cur) && tgt.HasDefault)
        {
            // Add default
            sb.Append($"ALTER TABLE {tgt.Table.FullName} ADD ")
                .AppendIf(() => $"CONSTRAINT [{tgt.DefaultName}] ", !tgt.IsDefaultSystemNamed)
                .AppendLine($"DEFAULT ({tgt.DefaultValue}) FOR [{tgt.Name}]");
        }

        // Make unique (drop is handled elsewhere)
        if (tgt.IsUnique && !cur.IsUnique)
        {
            sb.AppendLine($"ALTER TABLE {tgt.Table.FullName} ADD UNIQUE ([{tgt.Name}])");
        }

        // Reference
        var tgtRef = tgt.Ref != null ? new FieldRefModel(tgt.Ref) : null;
        var curRef = cur.Ref != null ? new FieldRefModel(cur.Ref) : null;
        if (curRef is null || tgtRef?.Equals(curRef) != true)
        {
            if (curRef != null)
            {
                if (curRef.IsSystemNamed || curRef.Name.Length == 0)
                {
                    sb.AppendLine($"-- Removing unnamed FKey: {curRef.Field.FullName} -> {curRef.TargetField.FullName}")
                        .AppendLine(CommonMssql.REF_GET_FKEYNAME(curRef.Table.FullName, curRef.Field.Name))
                        .AppendLine(CommonMssql.REF_GET_DROP_SQL(curRef.Table.FullName))
                        .AppendLine("EXEC (@DropConstraintSql)");
                }
                else
                {
                    sb.AppendLine(CommonMssql.ALTER_TABLE_DROP_CONSTRAINT(curRef.Table.FullName, curRef.Name));
                }
            }
            if (tgtRef != null)
            {
                sb.Append($"ALTER TABLE {tgtRef.Table.FullName} WITH NOCHECK ADD ")
                    .AppendIf(() => $"CONSTRAINT [{tgtRef.Name}] ", !tgtRef.IsSystemNamed)
                    .AppendLine($"FOREIGN KEY ([{tgtRef.Field.Name}]) REFERENCES {tgtRef.TargetField.Table.FullName} ([{tgtRef.TargetField.Name}])");
            }
        }
    }
}
