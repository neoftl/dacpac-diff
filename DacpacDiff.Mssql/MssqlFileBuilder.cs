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

                sqlHead.AppendFormat("-- [{0}] {1}: {2}", diffNum, diff.Title, diff.Name).AppendLine();
                AppendLine().AppendFormat("#print '> [{0}] {1}: {2} ({3}%%)'", diffNum, diff.Title, diff.Name, progress.ToString("0.00")).AppendLine();

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
            AppendLine($@"SET NOCOUNT ON;
SET XACT_ABORT ON;
SET NOEXEC OFF;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE #print(@s1 NVARCHAR(max), @s2 NVARCHAR(max) = NULL, @s3 NVARCHAR(max) = NULL, @s4 NVARCHAR(max) = NULL, @s5 NVARCHAR(max) = NULL, @s6 NVARCHAR(max) = NULL, @s7 NVARCHAR(max) = NULL, @s8 NVARCHAR(max) = NULL, @s9 NVARCHAR(max) = NULL) AS DECLARE @M NVARCHAR(MAX) = CONCAT(@s1, @s2, @s3, @s4, @s5, @s6, @s7, @s8, @s9); RAISERROR(@M, 0, 1) WITH NOWAIT
GO

-- Pre-flight checks
DECLARE @CurVersion VARCHAR(MAX)
IF (object_id('[dbo].[tfn_DatabaseVersion]') IS NULL) BEGIN
	SET @CurVersion = '(unknown)'
END ELSE BEGIN
	SELECT @CurVersion = [BuildNumber] FROM [dbo].tfn_DatabaseVersion()
END
IF (@CurVersion <> '{targetVersion}') BEGIN
    EXEC #print 'Failed: Current version ', @CurVersion, ' does not match expected: {targetVersion}'
    SET NOEXEC ON;
END
GO
IF (@@TRANCOUNT <> 0) BEGIN
    EXEC #print 'Failed: Transaction mismatch (', @@TRANCOUNT, ')'
    SET NOEXEC ON;
END
;BEGIN TRAN
GO

-- Temporary release helpers
CREATE OR ALTER PROCEDURE #usp_DropUnnamedCheckConstraint(@parentTable NVARCHAR(max), @defSql NVARCHAR(max)) AS BEGIN
    DECLARE @chkName VARCHAR(MAX) = (SELECT TOP 1 [name] FROM sys.check_constraints
        WHERE [parent_object_id] = OBJECT_ID(@parentTable) AND [type] = 'C' AND REPLACE(REPLACE(REPLACE([definition], '(', ''), ')', ''), ' ', '') = @defSql AND [is_system_named] = 1)
    IF (@chkName IS NULL) BEGIN
        EXEC #print '[WARN] Could not locate system-named check constraint on ', @parentTable, '. Manual clean-up may be required.'
    END ELSE BEGIN
        DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @chkName, ']')
        EXEC (@sql)
        EXEC #print '[NOTE] Dropped system-named check constraint [', @chkName, '] on ', @parentTable
    END
END
GO
CREATE OR ALTER PROCEDURE #usp_DropUnnamedDefault(@parentTable NVARCHAR(max), @colName VARCHAR(255)) AS BEGIN
    DECLARE @chkName VARCHAR(MAX) = (SELECT TOP 1 DF.[name] FROM sys.default_constraints DF
        JOIN sys.all_columns C ON C.[object_id] = DF.[parent_object_id] AND C.[column_id] = DF.[parent_column_id]
        WHERE DF.[parent_object_id] = OBJECT_ID(@parentTable) AND DF.[type] = 'D' AND DF.[is_system_named] = 1 AND C.[name] = @colName)
    IF (@chkName IS NULL) BEGIN
        EXEC #print '[WARN] Could not locate system-named check constraint on ', @parentTable, '. Manual clean-up may be required.'
    END ELSE BEGIN
        DECLARE @sql VARCHAR(MAX) = CONCAT('ALTER TABLE ', @parentTable, ' DROP CONSTRAINT [', @chkName, ']')
        EXEC (@sql)
        EXEC #print '[NOTE] Dropped system-named check constraint [', @chkName, '] on ', @parentTable
    END
END
GO");
        }

        private void addScriptFoot()
        {
            AppendLine($@"
-- Complete
GO
DROP FUNCTION IF EXISTS tmpsfn_CleanSQL
GO
IF (@@ERROR <> 0) BEGIN
    EXEC #print 'Failed'
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END ELSE IF (@@TRANCOUNT <> 1) BEGIN
    EXEC #print 'Failed: Transaction mismatch (', @@TRANCOUNT, ')'
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END
;COMMIT
GO

#print 'Complete'
SELECT * FROM [dbo].[tfn_DatabaseVersion]()
SET NOEXEC OFF;");
        }
    }
}
