using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DacpacDiff.Mssql.Output
{
    /// <summary>
    /// Converts a set of diffs to valid MSSQL
    /// </summary>
    public class MssqlFileFormat : IFileFormat
    {
        public static readonly string SECTION_START = @"BEGIN TRAN;
IF (@@TRANCOUNT <> 2) BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ')'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END";
        public static readonly string SECTION_END = @"IF (@@ERROR <> 0) BEGIN
    RAISERROR('Failed', 0, 1) WITH NOWAIT;
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END ELSE IF (@@TRANCOUNT <> 2) BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ')'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END;
COMMIT";

        public MssqlFileFormat()
        {
        }

        private static string flatten(string sql, bool flat)
        {
            return flat ? sql.Replace("\r\n", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ") : sql;
        }

        public string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<IDifference> diffs, bool withDataLossCheck, bool flat = true)
        {
            var diffCount = diffs.Count(d => (d.Title?.Length ?? 0) > 0);
            var countMag = 1 + (int)Math.Log10(diffCount);

            var sqlHead = new StringBuilder();
            var sqlBody = new StringBuilder();

            sqlHead.Append(getScriptHead(leftFileName, rightFileName, diffs));
            sqlBody.Append(getScriptStart(targetVersion));

            var count = 0;
            foreach (var diff in diffs)
            {
                if ((diff.Title?.Length ?? 0) == 0)
                {
                    sqlBody.Append(diff.ToString())
                        .AppendLine();
                    continue;
                }

                var diffNum = $"[{(++count).ToString("D" + countMag)}] ";
                var progress = (double)(99.99 / diffCount) * count;

                sqlHead.Append("-- ").Append(diffNum)
                    .Append(diff.Title).Append(": ").Append(diff.Name)
                    .AppendLine();

                sqlBody.Append($"\r\nRAISERROR('> ").Append(diffNum)
                    .Append(diff.Title).Append(": ").Append(diff.Name)
                    .AppendFormat(" ({0,5}%)", progress.ToString("0.00"))
                    .Append("', 0, 1) WITH NOWAIT; ")
                    .Append(flat ? string.Empty : "\r\n")
                    .Append(flatten(SECTION_START, flat))
                    .AppendLine("\r\nGO\r\n");

                if (withDataLossCheck && diff is IDataLossChange d && d.GetDataLossTable(out var tableName))
                {
                    sqlBody.AppendLine()
                        .AppendLine($"IF EXISTS (SELECT 1 FROM {tableName}) BEGIN")
                        .AppendLine($"    RAISERROR('WARNING! This change may cause dataloss to {tableName}. Verify and remove this error block to continue.', 0, 1) WITH NOWAIT;")
                        .AppendLine("    IF (@@TRANCOUNT > 0) ROLLBACK;")
                        .AppendLine("    SET NOEXEC ON;")
                        .AppendLine("END")
                        .AppendLine();
                }

                sqlBody.Append(diff.ToString())
                    .AppendLine("\r\n\r\nGO")
                    .Append(flatten(SECTION_END, flat))
                    .AppendLine("\r\nGO");
            }

            sqlHead.Append(sqlBody)
                .Append(getScriptFoot());
            return sqlHead.ToString();
        }

        private static string getScriptHead(string leftFileName, string rightFileName, IEnumerable<IDifference> diffs)
        {
            var diffCount = diffs.Count(d => (d.Title?.Length ?? 0) > 0);
            return $@"-- Delta upgrade from {leftFileName} to {rightFileName}
-- Generated {DateTime.UtcNow}
--
-- Changes ({diffCount}):
";
        }

        private static string getScriptStart(string targetVersion)
        {
            return $@"--

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET NOEXEC OFF;
SET QUOTED_IDENTIFIER ON;
GO
DECLARE @CurVersion VARCHAR(MAX)
IF (object_id('[dbo].[tfn_DatabaseVersion]') IS NULL) BEGIN
	SET @CurVersion = '00000000.0000'
END ELSE BEGIN
	SELECT @CurVersion = [BuildNumber] FROM [dbo].tfn_DatabaseVersion()
END
IF (@CurVersion <> '{targetVersion}') BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Target version (', @CurVersion, ') does not match expected: {targetVersion}'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    SET NOEXEC ON;
END
GO
IF (@@TRANCOUNT <> 0) BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ')'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    SET NOEXEC ON;
END
BEGIN TRAN;
GO
";
        }

        private static string getScriptFoot()
        {
            return $@"
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
SET NOEXEC OFF;";
        }
    }
}
