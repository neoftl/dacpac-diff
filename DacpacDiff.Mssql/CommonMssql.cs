using DacpacDiff.Core.Utility;

namespace DacpacDiff.Mssql
{
    public static class CommonMssql
    {
        public static string ALTER_TABLE_DROP_COLUMN(string tableFullName, string fieldName)
            => "ALTER TABLE {0} DROP COLUMN [{1}]".Format(tableFullName, fieldName);
        public static string ALTER_TABLE_DROP_CONSTRAINT(string tableFullName, string fieldName)
            => "ALTER TABLE {0} DROP CONSTRAINT [{1}]".Format(tableFullName, fieldName);

        public static string REF_GET_FKEYNAME(string tableFullName, string fieldName)
            => "DECLARE @FKeyName VARCHAR(MAX) = (SELECT FK.[name] FROM sys.foreign_keys FK JOIN sys.foreign_key_columns KC ON KC.[constraint_object_id] = FK.[object_id] JOIN sys.columns C ON C.[object_id] = FK.[parent_object_id] AND C.[column_id] = KC.[parent_column_id] WHERE FK.[parent_object_id] = OBJECT_ID('{0}') AND FK.[type] = 'F' AND C.[name] = '{1}')".Format(tableFullName, fieldName); // TODO: Shouldn't this check the target as well?
        public static string REF_GET_DROP_SQL(string tableFullName)
            => "DECLARE @DropConstraintSql VARCHAR(MAX) = CONCAT('ALTER TABLE {0} DROP CONSTRAINT ', QUOTENAME(@FKeyName))".Format(tableFullName);
    }
}
