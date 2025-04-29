using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;

namespace DacpacDiff.Mssql;

public static class CommonMssql
{
    public static string ALTER_TABLE_DROP_COLUMN(string tableFullName, string fieldName)
        => "ALTER TABLE {0} DROP COLUMN [{1}]".Format(tableFullName, fieldName);

    public static string ALTER_TABLE_DROP_CONSTRAINT(string tableFullName, string fieldName)
        => "ALTER TABLE {0} DROP CONSTRAINT [{1}]".Format(tableFullName, fieldName);

    public static ISqlFileBuilder ALTER_TABLE_DROP_FIELD_REF(this ISqlFileBuilder sb, FieldRefModel fRef)
    {
        if (fRef.IsSystemNamed || fRef.Name.Length == 0)
        {
            sb.AppendLine($"-- Removing unnamed FKey: {fRef.Field.FullName} -> {fRef.TargetField.FullName}")
                .AppendLine(REF_GET_FKEYNAME(fRef.Table.FullName, fRef.Field.Name))
                .AppendLine(REF_GET_DROP_SQL(fRef.Table.FullName))
                .AppendLine("EXEC (@DropConstraintSql)");
        }
        else
        {
            sb.AppendLine(ALTER_TABLE_DROP_CONSTRAINT(fRef.Table.FullName, fRef.Name));
        }
        return sb;
    }

    public static ISqlFileBuilder DROP_UNNAMED_UNIQUE(this ISqlFileBuilder sb, FieldModel field)
        => sb.DROP_UNNAMED_UNIQUE(field.Table.FullName, field.Name);
    public static ISqlFileBuilder DROP_UNNAMED_UNIQUE(this ISqlFileBuilder sb, string tableFullName, string fieldName)
        => sb.AppendFormat("EXEC #usp_DropUnnamedUniqueConstraint '{0}', '{1}'", tableFullName, fieldName).AppendGo();

    public static string REF_GET_DROP_SQL(string tableFullName)
        => "DECLARE @DropConstraintSql VARCHAR(MAX) = CONCAT('ALTER TABLE {0} DROP CONSTRAINT ', QUOTENAME(@FKeyName))".Format(tableFullName);

    public static string REF_GET_FKEYNAME(string tableFullName, string fieldName)
        => "DECLARE @FKeyName VARCHAR(MAX) = (SELECT FK.[name] FROM sys.foreign_keys FK JOIN sys.foreign_key_columns KC ON KC.[constraint_object_id] = FK.[object_id] JOIN sys.columns C ON C.[object_id] = FK.[parent_object_id] AND C.[column_id] = KC.[parent_column_id] WHERE FK.[parent_object_id] = OBJECT_ID('{0}') AND FK.[type] = 'F' AND C.[name] = '{1}')".Format(tableFullName, fieldName); // TODO: Shouldn't this check the target as well?
}
