using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public abstract class BaseMssqlDiffBlock<T> : IDiffFormatter
        where T : IDifference
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

        protected readonly T _diff;

        protected BaseMssqlDiffBlock(T diff)
        {
            _diff = diff;
        }

        public StringBuilder Format(StringBuilder sb, bool checkForDataLoss, bool prettyPrint)
        {
            sb.Append(MssqlFileFormat.Flatten(SECTION_START, !prettyPrint))
                .EnsureLine().AppendLine("GO").AppendLine();
            
            if (checkForDataLoss && _diff is IDataLossChange d && d.GetDataLossTable(out var tableName))
            {
                sb.AppendLine()
                    .AppendLine($"IF EXISTS (SELECT TOP 1 1 FROM {tableName}) BEGIN")
                    .AppendLine($"    RAISERROR('WARNING! This change may cause dataloss to {tableName}. Verify and remove this error block to continue.', 0, 1) WITH NOWAIT;")
                    .AppendLine("    IF (@@TRANCOUNT > 0) ROLLBACK;")
                    .AppendLine("    SET NOEXEC ON;")
                    .AppendLine("END").AppendLine();
            }

            GetFormat(sb, checkForDataLoss, prettyPrint);

            sb.EnsureLine().AppendLine().AppendLine("GO")
                .Append(MssqlFileFormat.Flatten(SECTION_END, !prettyPrint))
                .EnsureLine().AppendLine("GO");

            return sb;
        }

        protected abstract void GetFormat(StringBuilder str, bool checkForDataLoss, bool prettyPrint);
    }
}
