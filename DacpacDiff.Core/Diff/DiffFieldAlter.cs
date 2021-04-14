using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Core.Diff
{
    public class DiffFieldAlter : IDifference, IDataLossChange
    {
        public FieldModel LeftField { get; }
        public FieldModel RightField { get; }

        public IModel Model => LeftField ?? RightField;
        public string Title => "Alter table field";
        public string Name => $"[{LeftField.Table.Schema.Name}].[{LeftField.Table.Name}].[{LeftField.Name}]";

        public DiffFieldAlter(FieldModel lft, FieldModel rgt)
        {
            LeftField = lft ?? throw new ArgumentNullException(nameof(lft));
            RightField = rgt ?? throw new ArgumentNullException(nameof(rgt));
        }

        // TODO: flawed; should be individual diffs
        public override string ToString()
        {
            var sql = new List<string>();

            // TODO: If field has dependencies (constraint, index, etc.), drop first then recreate (in the comparer)

            if ((LeftField.Computation?.Length ?? 0) > 0)
            {
                if (RightField.Table != null)
                {
                    sql.Add($"ALTER TABLE {RightField.Table.FullName} DROP COLUMN [{RightField.Name}]\r\n");
                }
                sql.Add($"ALTER TABLE {LeftField.Table.FullName} ADD {LeftField.GetAlterSql()}");
                return String.Join("\r\nGO\r\n", sql.ToArray());
            }

            // Main definition
            var alterSql = LeftField.GetAlterSql(!RightField.Nullable);
            if (alterSql != RightField.GetAlterSql(!RightField.Nullable))
            {
                sql.Add($"ALTER TABLE {LeftField.Table.FullName} ALTER COLUMN {alterSql}");

                if (!LeftField.Nullable && !LeftField.HasDefault && RightField.Nullable)
                {
                    sql[^1] += " -- NOTE: Cannot change to NOT NULL column";
                }
            }

            // Default
            if (!LeftField.IsDefaultMatch(RightField))
            {
                if (RightField.HasDefault)
                {
                    // Remove default
                    if (!RightField.IsSystemNamedDefault)
                    {
                        sql.Add($"ALTER TABLE {RightField.Table.FullName} DROP CONSTRAINT [{RightField.DefaultName}]");
                    }
                    else
                    {
                        sql.Add($"DECLARE @DropConstraintSql VARCHAR(MAX) = (SELECT CONCAT('ALTER TABLE {RightField.Table.FullName} DROP CONSTRAINT [', [name], ']') FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('{RightField.Table.FullName}') AND parent_column_id = {RightField.Order - 1}); EXEC (@DropConstraintSql)");
                    }
                }
                if (LeftField.HasDefault)
                {
                    // Add default
                    if (!LeftField.IsSystemNamedDefault)
                    {
                        sql.Add($"ALTER TABLE {LeftField.Table.FullName} ADD CONSTRAINT [{LeftField.DefaultName}] DEFAULT{LeftField.DefaultValue} FOR [{LeftField.Name}]");
                    }
                    else
                    {
                        sql.Add($"ALTER TABLE {LeftField.Table.FullName} ADD DEFAULT{LeftField.DefaultValue} FOR [{LeftField.Name}]");
                    }
                }
            }

            // Unique
            if (LeftField.IsUnique != RightField.IsUnique)
            {
                if (LeftField.IsUnique)
                {
                    // Make unique
                    if (!LeftField.IsSystemNamedUnique)
                    {
                        sql.Add($"ALTER TABLE {LeftField.Table.FullName} ADD CONSTRAINT [{LeftField.Unique}] UNIQUE ([{LeftField.Name}])");
                    }
                    else
                    {
                        sql.Add($"ALTER TABLE {LeftField.Table.FullName} ADD UNIQUE ([{LeftField.Name}])");
                    }
                }
                else
                {
                    // Make non-unique
                    if (!RightField.IsSystemNamedUnique)
                    {
                        sql.Add($"ALTER TABLE {LeftField.Table.FullName} DROP CONSTRAINT [{RightField.DefaultName}]");
                    }
                    else
                    {
                        // TODO
                    }
                }
            }

            // Reference
            var lftRef = LeftField.Ref is not null ? new RefModel(LeftField) : null;
            var rgtRef = RightField.Ref is not null ? new RefModel(RightField) : null;
            if (rgtRef is null || lftRef?.Equals(rgtRef) != false)
            {
                if (rgtRef is not null)
                {
                    sql.Add(rgtRef.GetDropSql());
                }
                if (lftRef is not null)
                {
                    sql.Add(lftRef.GetAddSql());
                }
            }

            // Cannot modify PKey
            if (sql.Count > 0 && RightField?.Table?.PrimaryKey?.Contains(RightField?.Name) == true)
            {
                // TODO: Need to replace whole table via temp
                sql.Insert(0, " NOTE: Cannot modify Primary Key column");
                sql = sql.Select(s => "--" + s).ToList();
            }

            return String.Join("\r\nGO\r\n", sql.ToArray());
        }

        public bool GetDataLossTable(out string tableName)
        {
            // TODO: More accurate test
            tableName = RightField.Table.FullName;
            return LeftField.Type != RightField.Type;
        }
    }
}
