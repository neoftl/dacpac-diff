using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IFormatProvider = DacpacDiff.Core.IFormatProvider;

namespace DacpacDiff.Mssql
{
    /// <summary>
    /// Converts a set of diffs to valid MSSQL
    /// </summary>
    public class MssqlFileBuilder : BaseSqlFileBuilder
    {
        private readonly IFormatProvider _formatProvider;

        public MssqlFileBuilder(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public override string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<ISqlFormattable> objs)
        {
            var objCount = objs.Count(d => (d.Title?.Length ?? 0) > 0);
            var countMag = 1 + (int)Math.Log10(objCount);

            var sqlHead = new StringBuilder();

            sqlHead.AppendLine($@"-- Delta upgrade from {rightFileName} to {leftFileName}
-- Generated {DateTime.UtcNow}
--
-- Changes ({objCount}):");

            addScriptStart(targetVersion);

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

                sqlHead.AppendFormat("-- [{0}] {1}: {2}", diffNum, diff.Title, diffName).AppendLine();
                AppendLine().AppendFormat("#print 0, '> [{0}] {1}: {2} ({3}%%)'", diffNum, diff.Title, diff.Name, progress.ToString("0.00")).AppendLine();

                diffFormatter.Format(this);
                EnsureLine();
            }

            _sql.Insert(0, sqlHead.AppendLine("--").AppendLine().ToString());

            // TODO: when to refresh modules?

            addScriptFoot();
            return _sql.ToString();
        }

        private void addScriptStart(string targetVersion)
        {
            AppendLine(@"SET NOCOUNT ON
SET XACT_ABORT ON
SET NOEXEC OFF
SET QUOTED_IDENTIFIER ON
GO");

            AppendLine(Flatten(@"CREATE OR ALTER PROCEDURE #print @t BIT, @s1 NVARCHAR(max), @s2 NVARCHAR(max) = NULL, @s3 NVARCHAR(max) = NULL, @s4 NVARCHAR(max) = NULL, @s5 NVARCHAR(max) = NULL, @s6 NVARCHAR(max) = NULL, @s7 NVARCHAR(max) = NULL, @s8 NVARCHAR(max) = NULL, @s9 NVARCHAR(max) = NULL AS BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT(@s1, @s2, @s3, @s4, @s5, @s6, @s7, @s8, @s9)
    DECLARE @L INT = IIF(@t = 0, 0, 15)
    RAISERROR(@M, @L, 1) WITH NOWAIT
END
GO"));

        AppendLine($@"-- Pre-flight checks
DECLARE @CurVersion VARCHAR(MAX) = '(unknown)'
IF (object_id('[dbo].[tfn_DatabaseVersion]') IS NOT NULL) BEGIN
	SELECT @CurVersion = [BuildNumber] FROM [dbo].tfn_DatabaseVersion()
END
IF (@CurVersion <> '{targetVersion}') BEGIN
    EXEC #print 1, 'Failed: Current version ', @CurVersion, ' does not match expected: {targetVersion}'
    SET NOEXEC ON
END
GO");

            AppendLine(Flatten(@"
-- Temporary release helpers
CREATE OR ALTER PROCEDURE #flag_IsRunning AS RETURN 1
GO
CREATE OR ALTER PROCEDURE #usp_DropUnnamedCheckConstraint(@parentTable NVARCHAR(max), @defSql CHAR(16)) AS BEGIN
    DECLARE @chkName VARCHAR(MAX) = (SELECT TOP 1 [name] FROM sys.check_constraints WHERE [parent_object_id] = OBJECT_ID(@parentTable) AND [type] = 'C' AND HASHBYTES('MD5', REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER(CONVERT(VARCHAR(MAX), [definition])), '(', ''), ')', ''), '[', ''), ']', ''), ' ', '')) = @defSql AND [is_system_named] = 1)
    IF (@chkName IS NULL) BEGIN
        EXEC #print 1, '[WARN] Could not locate system-named check constraint on ', @parentTable, '. Manual clean-up may be required.'
    END ELSE BEGIN
        DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @chkName, ']')
        EXEC (@sql)
        EXEC #print 0, '[NOTE] Dropped system-named check constraint [', @chkName, '] on ', @parentTable
    END
END
GO
CREATE OR ALTER PROCEDURE #usp_DropUnnamedDefault(@parentTable NVARCHAR(max), @colName VARCHAR(255)) AS BEGIN
    DECLARE @chkName VARCHAR(MAX) = (SELECT TOP 1 DF.[name] FROM sys.default_constraints DF JOIN sys.all_columns C ON C.[object_id] = DF.[parent_object_id] AND C.[column_id] = DF.[parent_column_id] WHERE DF.[parent_object_id] = OBJECT_ID(@parentTable) AND DF.[type] = 'D' AND DF.[is_system_named] = 1 AND C.[name] = @colName)
    IF (@chkName IS NULL) BEGIN
        EXEC #print 1, '[WARN] Could not locate system-named check constraint on ', @parentTable, '. Manual clean-up may be required.'
    END ELSE BEGIN
        DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @chkName, ']')
        EXEC (@sql)
        EXEC #print 0, '[NOTE] Dropped system-named check constraint [', @chkName, '] on ', @parentTable
    END
END
GO
CREATE OR ALTER PROCEDURE #usp_DropUnnamedUniqueConstraint(@parentTable NVARCHAR(max), @columns NVARCHAR(MAX))
AS BEGIN
	DECLARE @uqName VARCHAR(MAX) = (SELECT TOP 1 KC.[name] FROM sys.key_constraints KC JOIN sys.index_columns IC ON IC.[object_id] = KC.[parent_object_id] AND IC.[index_id] = KC.[unique_index_id] JOIN sys.columns TC ON TC.[object_id] = IC.[object_id] AND TC.[column_id] = IC.[column_id] WHERE KC.[parent_object_id] = OBJECT_ID(@parentTable) AND KC.[type] = 'UQ' GROUP BY KC.[name] HAVING COUNT(1) - SUM(IIF(CHARINDEX(',' + TC.[name] + ',', @columns) > 0, 1, 0)) = 0)
	IF (@uqName IS NULL) BEGIN
		EXEC #print 1, '[WARN] Could not locate system-named check constraint on ', @parentTable, ' matching column list. Manual clean-up may be required.'
	END ELSE BEGIN
		DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @uqName, ']')
		EXEC (@sql)
		EXEC #print 0, '[NOTE] Dropped system-named check constraint [', @uqName, '] on ', @parentTable
	END
END
GO
CREATE OR ALTER PROCEDURE #usp_CheckState @C INT AS BEGIN
    DECLARE @err INT = @@ERROR
    IF (@err = 0) BEGIN
        IF (@@TRANCOUNT = @C) RETURN 1
        EXEC #print 1, 'Failed: Transaction mismatch (', @@TRANCOUNT, ' <> ', @C, ')'
    END ELSE BEGIN
        EXEC #print 1, 'Failed due to error (', @err, ')'
    END
    IF (@@TRANCOUNT > 0) ROLLBACK
    DROP PROCEDURE #flag_IsRunning
    RETURN -1
END
GO
CREATE OR ALTER FUNCTION ufn_IsRunning () RETURNS BIT AS BEGIN
    RETURN IIF(object_id('tempdb..#flag_IsRunning') IS NULL, 0, 1)
END
GO
EXEC #usp_CheckState 0
IF (dbo.ufn_IsRunning() = 0) SET NOEXEC ON
GO
BEGIN TRAN
GO"));
        }

        private void addScriptFoot()
        {
            AppendLine($@"
-- Complete
GO
EXEC #usp_CheckState 1
IF (dbo.ufn_IsRunning() = 0) SET NOEXEC ON
GO
COMMIT
GO
DROP FUNCTION IF EXISTS dbo.ufn_IsRunning
GO

EXEC #print 0, 'Complete'
SELECT * FROM [dbo].[tfn_DatabaseVersion]()
GO
SET NOEXEC OFF");
        }
    }
}
