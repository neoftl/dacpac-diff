using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff;

public class MssqlDiffFieldAlter(DiffFieldAlter diff)
    : BaseMssqlDiffBlock<DiffFieldAlter>(diff)
{
    private static void appendFieldSql(FieldModel fld, ISqlFileBuilder sb, bool existingIsNull, bool asAlter)
    {
        sb.Append($"[{fld.Name}]");

        if (fld.Computation?.Length > 0)
        {
            sb.Append($" AS {fld.Computation}");
            return;
        }

        sb.Append($" {fld.Type}")
            .AppendIf(() => " COLLATE " + fld.Collation, fld.Collation != null)
            .Append(!fld.Nullable && (!existingIsNull || fld.HasDefault) ? " NOT NULL" : " NULL")
            .AppendIf(() => $" DEFAULT ({fld.DefaultValue})", fld.IsDefaultSystemNamed && !asAlter);
    }

    protected override void GetFormat(ISqlFileBuilder sb)
    {
        var tgt = _diff.TargetField;
        var cur = _diff.CurrentField;

        // Change to/from computed column
        if (_diff.Has(DiffFieldAlter.Change.Computed, DiffFieldAlter.Change.ComputedUnset))
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
        if (_diff.Has(DiffFieldAlter.Change.DefaultUnset))
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

        var makeUnique = _diff.Has(DiffFieldAlter.Change.Unique);

        // Main definition
        if (_diff.Has(
                DiffFieldAlter.Change.Type,
                DiffFieldAlter.Change.Collation, DiffFieldAlter.Change.CollationUnset,
                DiffFieldAlter.Change.Computed, DiffFieldAlter.Change.ComputedUnset,
                DiffFieldAlter.Change.Nullable
            ))
        {
            // Drop unnamed uniqueness
            if (cur.IsUnique)
            {
                sb.DROP_UNNAMED_UNIQUE(cur);
                makeUnique = true;
            }

            // Alter
            sb.Append($"ALTER TABLE {tgt.Table.FullName} ALTER COLUMN ");
            appendFieldSql(tgt, sb, cur.Nullable, true); // TODO: changing between nullability needs thinking about
            sb.AppendLine(!tgt.Nullable && !tgt.HasDefault && cur.Nullable ? " -- NOTE: Cannot change to NOT NULL without default" : string.Empty)
                .AppendLine();
        }

        // Default
        if (_diff.Has(DiffFieldAlter.Change.Default))
        {
            // Add default
            sb.Append($"ALTER TABLE {tgt.Table.FullName} ADD ")
                .AppendIf(() => $"CONSTRAINT [{tgt.DefaultName}] ", !tgt.IsDefaultSystemNamed)
                .AppendLine($"DEFAULT ({tgt.DefaultValue}) FOR [{tgt.Name}]");
        }

        // Make unique (drop is handled elsewhere)
        if (makeUnique)
        {
            sb.AppendLine($"ALTER TABLE {tgt.Table.FullName} ADD UNIQUE ([{tgt.Name}])");
        }

        // Reference
        if (_diff.Has(DiffFieldAlter.Change.ReferenceUnset))
        {
            sb.ALTER_TABLE_DROP_FIELD_REF(cur.Ref!);
        }
        if (_diff.Has(DiffFieldAlter.Change.Reference))
        {
            sb.Append($"ALTER TABLE {tgt.Ref!.Table.FullName} WITH NOCHECK ADD ")
                .AppendIf(() => $"CONSTRAINT [{tgt.Ref.Name}] ", !tgt.Ref.IsSystemNamed)
                .AppendLine($"FOREIGN KEY ([{tgt.Ref.Field.Name}]) REFERENCES {tgt.Ref.TargetField.Table.FullName} ([{tgt.Ref.TargetField.Name}])");
        }
    }
}
