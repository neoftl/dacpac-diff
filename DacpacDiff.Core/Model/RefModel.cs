using System;

namespace DacpacDiff.Core.Model
{
    public class RefModel : IModel<RefModel, TableModel>, IEquatable<RefModel>
    {
        public static readonly RefModel Empty = new RefModel();

        public TableModel Table { get; private set; } = TableModel.Empty;
        public string Name { get; set; }
        public bool IsSystemNamed { get; }

        public string Field { get; }
        public string TargetTable { get; }
        public string TargetField { get; }

        private RefModel()
        {
            Name = string.Empty;
            Field = string.Empty;
            TargetTable = string.Empty;
            TargetField = string.Empty;
        }
        public RefModel(RefModel tref)
        {
            Table = tref.Table;
            Name = tref.Name;
            IsSystemNamed = tref.IsSystemNamed;
            Field = tref.Field;
            TargetField = tref.TargetField;
            TargetTable = tref.TargetTable;
        }
        public RefModel(FieldModel field)
        {
            Table = field.Table;
            Name = field.Ref?.Name ?? string.Empty;
            IsSystemNamed = field.Ref?.IsSystemNamed == true;
            Field = field.Name;
            TargetTable = field.Ref?.TargetTable ?? string.Empty;
            TargetField = field.Ref?.TargetField ?? string.Empty;
        }

        // TODO: To MSSQL library
        public string GetAddSql()
        {
            if (!IsSystemNamed)
            {
                return $"ALTER TABLE {Table.FullName} WITH NOCHECK ADD FOREIGN KEY ([{Field}]) REFERENCES {TargetTable} ([{TargetField}])";
            }
            return $"ALTER TABLE {Table.FullName} WITH NOCHECK ADD CONSTRAINT [{Name}] FOREIGN KEY ([{Field}]) REFERENCES {TargetTable} ([{TargetField}])";
        }

        // TODO: To MSSQL library
        public string GetDropSql()
        {
            if (!IsSystemNamed || (Name?.Length ?? 0) == 0)
            {
                return $"DECLARE @DropConstraintSql VARCHAR(MAX) = (SELECT CONCAT('ALTER TABLE {Table.FullName} DROP CONSTRAINT [', FK.[name], ']') FROM sys.foreign_keys FK JOIN sys.foreign_key_columns KC ON KC.[constraint_object_id] = FK.[object_id] JOIN sys.columns C ON C.[object_id] = FK.[parent_object_id] AND C.[column_id] = KC.[parent_column_id] WHERE FK.[parent_object_id] = OBJECT_ID('{Table.FullName}') AND FK.[type] = 'F' AND C.[name] = '{Field}'); EXEC (@DropConstraintSql)";
            }
            return $"ALTER TABLE {Table.FullName} DROP CONSTRAINT [{Name}]";
        }

        public bool Equals(RefModel? rgt)
        {
            return Field == rgt?.Field
                && Table.FullName == rgt.Table.FullName
                && TargetTable == rgt.TargetTable
                && TargetField == rgt.TargetField
                && (!IsSystemNamed ? !rgt.IsSystemNamed : Name == rgt.Name);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as RefModel);
        }

        public override int GetHashCode()
        {
            // TODO
            return base.GetHashCode();
        }
    }
}
