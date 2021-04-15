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

                var diffNum = $"[{(++count).ToString("D" + countMag)}] ";
                var progress = (double)(99.99 / objCount) * count;

                sqlHead.Append("-- ").Append(diffNum)
                    .Append(diff.Title).Append(": ").Append(diff.Name).AppendLine();
                AppendLine().Append($"RAISERROR('> ").Append(diffNum)
                    .Append(diff.Title).Append(": ").Append(diff.Name)
                    .AppendFormat(" ({0,5}%)", progress.ToString("0.00"))
                    .Append("', 0, 1) WITH NOWAIT; ");

                diffFormatter.Format(this);
                EnsureLine();
            }

            _sql.Insert(0, sqlHead.AppendLine("--").AppendLine().ToString());

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

-- Pre-flight checks
DECLARE @CurVersion VARCHAR(MAX)
IF (object_id('[dbo].[tfn_DatabaseVersion]') IS NULL) BEGIN
	SET @CurVersion = '(unknown)'
END ELSE BEGIN
	SELECT @CurVersion = [BuildNumber] FROM [dbo].tfn_DatabaseVersion()
END
IF (@CurVersion <> '{targetVersion}') BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Current version ', @CurVersion, ' does not match expected: {targetVersion}'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    SET NOEXEC ON;
END
GO
IF (@@TRANCOUNT <> 0) BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ')'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    SET NOEXEC ON;
END
BEGIN TRAN;
GO");
        }

        private void addScriptFoot()
        {
            AppendLine($@"
-- Complete
GO
IF (@@ERROR <> 0) BEGIN
    RAISERROR('Failed', 0, 1) WITH NOWAIT;
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END ELSE IF (@@TRANCOUNT <> 1) BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ')'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END
COMMIT;
GO

RAISERROR('Complete', 0, 1) WITH NOWAIT;
SELECT * FROM [dbo].[tfn_DatabaseVersion]();
SET NOEXEC OFF;");
        }
    }
}
