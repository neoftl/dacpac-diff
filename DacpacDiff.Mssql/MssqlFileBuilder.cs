using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System.Text;
using IFormatProvider = DacpacDiff.Core.IFormatProvider;

namespace DacpacDiff.Mssql;

/// <summary>
/// Converts a set of diffs to valid MSSQL
/// </summary>
// TODO: ability to disable each section at the top of the file (temp table)
public class MssqlFileBuilder : BaseSqlFileBuilder
{
    private readonly IFormatProvider _formatProvider;

    public MssqlFileBuilder(IFormatProvider formatProvider)
    {
        _formatProvider = formatProvider;
    }

    public override string Generate(string targetFileName, string currentFileName, string targetVersion, IEnumerable<ISqlFormattable> objs)
    {
        var objCount = objs.Count(d => (d.Title?.Length ?? 0) > 0);
        var countMag = 1 + (int)Math.Log10(objCount);

        _sql.Clear();
        var sqlHead = new StringBuilder();

        sqlHead.AppendLine($@"-- Delta upgrade from {currentFileName} to {targetFileName}
-- Generated {DateTime.UtcNow}
--
-- Changes ({objCount}):");
        if (Options?.ChangeDisableOption == true)
        {
            sqlHead.AppendLine("DROP TABLE IF EXISTS #Changes; SELECT * INTO #Changes FROM (VALUES");
        }

        var count = 0;
        foreach (var diff in objs)
        {
            var diffFormatter = _formatProvider.GetSqlFormatter(diff);

            if ((diff.Title?.Length ?? 0) == 0)
            {
                diffFormatter.Format(this);
                EnsureLine();
                continue;
            }

            var diffNum = (++count).ToString("D" + countMag);
            var progress = (double)(99.99 / objCount) * count;

            var diffName = diff.Name;
            if (diff is IDataLossChange dlc && dlc.GetDataLossTable(out _))
            {
                diffName += " (potential data-loss)";
            }

            if (Options?.ChangeDisableOption == true)
            {
                sqlHead.AppendFormat("    {0}({1}, 1)", count > 1 ? ',' : ' ', diffNum);
            }
            sqlHead.AppendFormat("-- [{0}] {1}: {2}", diffNum, diff.Title, diffName).AppendLine();
            AppendLine().AppendFormat("#print 0, '> [{0}] {1}: {2} ({3}%%)'; EXEC #SetCI 1, {0}", diffNum, diff.Title, diff.Name, progress.ToString("0.00")).AppendLine();

            diffFormatter.Format(this);
            EnsureLine();
        }

        if (Options?.ChangeDisableOption == true)
        {
            sqlHead.AppendLine(") V ([ChangeNum], [Include])");
        }

        var sqlBody = _sql.ToString();
        sqlHead.AppendLine("--").AppendLine();
        addScriptStart(sqlHead, targetVersion, sqlBody, Options?.ChangeDisableOption == true);
        _sql.Insert(0, sqlHead.ToString());

        // TODO: when to refresh modules?

        addScriptFoot(_sql);
        return _sql.ToString();
    }

    private void addScriptStart(StringBuilder sb, string targetVersion, string sqlBody, bool canDisableChanges)
    {
        sb.AppendLine(@"SET NOCOUNT ON
SET XACT_ABORT ON
SET NOEXEC OFF
SET QUOTED_IDENTIFIER ON
SET CONTEXT_INFO 0
GO");

        sb.AppendLine().AppendLine(Flatten(@"
-- Release framework
CREATE OR ALTER PROC #print @t BIT, @s1 NVARCHAR(max), @s2 NVARCHAR(max) = NULL, @s3 NVARCHAR(max) = NULL, @s4 NVARCHAR(max) = NULL, @s5 NVARCHAR(max) = NULL, @s6 NVARCHAR(max) = NULL, @s7 NVARCHAR(max) = NULL, @s8 NVARCHAR(max) = NULL, @s9 NVARCHAR(max) = NULL AS BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT(@s1, @s2, @s3, @s4, @s5, @s6, @s7, @s8, @s9)
    DECLARE @L INT = IIF(@t = 0, 0, 15)
    RAISERROR(@M, @L, 1) WITH NOWAIT
END
GO
CREATE OR ALTER PROC #SetCI (@IsActive BIT, @ChangeNum INT)
AS
    DECLARE @CI VARBINARY(128) = CONVERT(BINARY(1), @IsActive) + CONVERT(BINARY(4), @ChangeNum)
    SET CONTEXT_INFO @CI
GO
CREATE OR ALTER FUNCTION dbo.tmpIsActive()
RETURNS BIT
AS BEGIN
    RETURN (SELECT CONVERT(BIT, SUBSTRING(CONTEXT_INFO(), 1, 1)))
END
GO
DROP PROC IF EXISTS #flag_TransactionError
GO"));

        if (canDisableChanges)
        {
            sb.AppendLine(Flatten(@"
CREATE OR ALTER FUNCTION dbo.tmpGetContextChangeNum()
RETURNS INT
AS BEGIN
    RETURN (SELECT CONVERT(INT, SUBSTRING(CONTEXT_INFO(), 2, 4)))
END
GO
CREATE OR ALTER PROC #IsChangeActive (@ChangeNum BIGINT)
AS
    RETURN ISNULL((SELECT TOP 1 [Include] FROM #Changes WHERE [ChangeNum] = @ChangeNum), 0)
GO
DELETE FROM #Changes WHERE [ChangeNum] = 0
INSERT INTO #Changes ([ChangeNum], [Include]) SELECT 0, 1
GO

CREATE OR ALTER PROC #usp_CheckState (@T INT) AS BEGIN
    DECLARE @err INT = @@ERROR, @IsActive BIT = 0, @ChangeNum INT = dbo.tmpGetContextChangeNum()
    EXEC @IsActive = #IsChangeActive 0
    IF (@IsActive = 1) BEGIN
        IF (@err = 0 AND @@TRANCOUNT = @T) BEGIN
            EXEC @IsActive = #IsChangeActive @ChangeNum
            IF (@IsActive = 0) EXEC #print 0, 'Skipping: Change item ', @ChangeNum, ' has been disabled'
            EXEC #SetCI @IsActive, @ChangeNum
            RETURN 1
        END
        IF (OBJECT_ID('tempdb..#flag_TransactionError') IS NULL) BEGIN
            DECLARE @msg VARCHAR(MAX) = IIF(@err <> 0, CONCAT('Failed due to error (', @err, ')'), CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ' <> ', @T, ')'))
            EXEC #print 1, @msg
            EXEC ('CREATE PROC #flag_TransactionError AS SELECT 1')
        END
    END
    DELETE FROM #Changes WHERE [ChangeNum] = 0
    EXEC #SetCI 0, @ChangeNum
    RETURN -1
END
GO"));
        }
        else
        {
            sb.AppendLine(Flatten(@"
CREATE OR ALTER PROC #usp_CheckState (@T INT) AS BEGIN
    DECLARE @err INT = @@ERROR
    IF (@err <> 0 OR @@TRANCOUNT <> @T) BEGIN
        IF (OBJECT_ID('tempdb..#flag_TransactionError') IS NULL) BEGIN
            DECLARE @msg VARCHAR(MAX) = IIF(@err <> 0, CONCAT('Failed due to error (', @err, ')'), CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ' <> ', @T, ')'))
            EXEC #print 1, @msg
            EXEC ('CREATE PROC #flag_TransactionError AS SELECT 1')
        END
        EXEC #SetCI 0, 0
        RETURN -1
    END
    RETURN 1
END
GO"));
        }

        sb.AppendLine($@"
-- Pre-flight checks
DECLARE @CurVersion VARCHAR(MAX) = '(unknown)'
IF (object_id('[dbo].[tfn_DatabaseVersion]') IS NOT NULL) BEGIN
	SELECT @CurVersion = [BuildNumber] FROM [dbo].tfn_DatabaseVersion()
END
IF (@CurVersion <> '{targetVersion}') BEGIN
    EXEC #print 1, 'Failed: Current version ', @CurVersion, ' does not match expected: {targetVersion}'
    SET NOEXEC ON
END ELSE BEGIN
    EXEC #SetCI 1, 0
END
GO");

        if (sqlBody.Contains("#usp_DropUnnamedCheckConstraint"))
        {
            sb.AppendLine(Flatten(@"
CREATE OR ALTER PROC #usp_DropUnnamedCheckConstraint(@parentTable NVARCHAR(max), @defSql CHAR(16)) AS BEGIN
    DECLARE @chkName VARCHAR(MAX) = (SELECT TOP 1 [name] FROM sys.check_constraints WHERE [parent_object_id] = OBJECT_ID(@parentTable) AND [type] = 'C' AND HASHBYTES('MD5', REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER(CONVERT(VARCHAR(MAX), [definition])), '(', ''), ')', ''), '[', ''), ']', ''), ' ', '')) = @defSql AND [is_system_named] = 1)
    IF (@chkName IS NULL) BEGIN
        EXEC #print 1, '[WARN] Could not locate system-named check constraint on ', @parentTable, '. Manual clean-up may be required.'
    END ELSE BEGIN
        DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @chkName, ']')
        EXEC (@sql)
        EXEC #print 0, '[NOTE] Dropped system-named check constraint [', @chkName, '] on ', @parentTable
    END
END
GO"));
        }

        if (sqlBody.Contains("#usp_DropUnnamedDefault"))
        {
            sb.AppendLine(Flatten(@"
CREATE OR ALTER PROC #usp_DropUnnamedDefault(@parentTable NVARCHAR(max), @colName VARCHAR(255)) AS BEGIN
    DECLARE @chkName VARCHAR(MAX) = (SELECT TOP 1 DF.[name] FROM sys.default_constraints DF JOIN sys.all_columns C ON C.[object_id] = DF.[parent_object_id] AND C.[column_id] = DF.[parent_column_id] WHERE DF.[parent_object_id] = OBJECT_ID(@parentTable) AND DF.[type] = 'D' AND DF.[is_system_named] = 1 AND C.[name] = @colName)
    IF (@chkName IS NULL) BEGIN
        EXEC #print 1, '[WARN] Could not locate system-named check constraint on ', @parentTable, '. Manual clean-up may be required.'
    END ELSE BEGIN
        DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @chkName, ']')
        EXEC (@sql)
        EXEC #print 0, '[NOTE] Dropped system-named default [', @chkName, '] on ', @parentTable
    END
END
GO"));
        }

        if (sqlBody.Contains("#usp_DropUnnamedUniqueConstraint"))
        {
            sb.AppendLine(Flatten(@"
CREATE OR ALTER PROC #usp_DropUnnamedUniqueConstraint(@parentTable NVARCHAR(max), @columns NVARCHAR(MAX)) AS BEGIN
	DECLARE @uqName VARCHAR(MAX) = (SELECT TOP 1 KC.[name] FROM sys.key_constraints KC JOIN sys.index_columns IC ON IC.[object_id] = KC.[parent_object_id] AND IC.[index_id] = KC.[unique_index_id] JOIN sys.columns TC ON TC.[object_id] = IC.[object_id] AND TC.[column_id] = IC.[column_id] WHERE KC.[parent_object_id] = OBJECT_ID(@parentTable) AND KC.[type] = 'UQ' GROUP BY KC.[name] HAVING COUNT(1) - SUM(IIF(CHARINDEX(',' + TC.[name] + ',', ',' + @columns + ',') > 0, 1, 0)) = 0)
	IF (@uqName IS NULL) BEGIN
		EXEC #print 1, '[WARN] Could not locate system-named check constraint on ', @parentTable, ' matching column list. Manual clean-up may be required.'
	END ELSE BEGIN
		DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @uqName, ']')
		EXEC (@sql)
		EXEC #print 0, '[NOTE] Dropped system-named unique constraint [', @uqName, '] on ', @parentTable
	END
END
GO"));
        }

        sb.AppendLine().AppendLine(Flatten(@"
-- Starting
SET NOEXEC OFF
EXEC #usp_CheckState 0
BEGIN TRAN
IF (dbo.tmpIsActive() = 0) SET NOEXEC ON
GO"));
    }

    private static void addScriptFoot(StringBuilder sb)
    {
        sb.AppendLine($@"
-- Complete
GO
EXEC #usp_CheckState 1; IF (dbo.tmpIsActive() = 0) SET NOEXEC ON
GO
COMMIT
GO

EXEC #print 0, 'Complete'
SELECT * FROM [dbo].[tfn_DatabaseVersion]()
GO
SET NOEXEC OFF
GO
IF (@@TRANCOUNT > 0) ROLLBACK
GO
DROP FUNCTION IF EXISTS dbo.tmpGetContextChangeNum
DROP FUNCTION IF EXISTS dbo.tmpIsActive");
    }
}
