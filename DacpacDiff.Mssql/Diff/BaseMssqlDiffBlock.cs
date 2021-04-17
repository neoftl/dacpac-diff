using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Mssql.Diff
{
    public abstract class BaseMssqlDiffBlock<T> : ISqlFormatter
        where T : IDifference
    {
        // Boilerplate to ensure we have a per-block transaction
        public static readonly string SECTION_START = @"BEGIN TRAN;
IF (@@TRANCOUNT <> 2) BEGIN
    DECLARE @M NVARCHAR(MAX) = CONCAT('Failed: Transaction mismatch (', @@TRANCOUNT, ')'); RAISERROR(@M, 0, 1) WITH NOWAIT;
    IF (@@TRANCOUNT > 0) ROLLBACK;
    SET NOEXEC ON;
END";

        // Boilerplace to ensure the per-block transaction survived and no errors
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
        private readonly string _sql;

        private class NullSqlBuilder : BaseSqlFileBuilder
        {
            public override string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<ISqlFormattable> diffs)
                => throw new NotImplementedException();

            public override string ToString() => _sql.ToString();
        }

        protected BaseMssqlDiffBlock(T diff)
        {
            _diff = diff ?? throw new ArgumentNullException(nameof(diff));

            var sb = new NullSqlBuilder();
            GetFormat(sb);
            _sql = sb.ToString();
        }

        public void Format(ISqlFileBuilder sb)
        {
            sb.Append(sb.Flatten(SECTION_START))
                .EnsureLine().AppendGo().AppendLine();

            string? datalossTable = null;
            var isDataLossChange = _diff is IDataLossChange d && d.GetDataLossTable(out datalossTable);
            if (sb.Options?.DisableDatalossCheck != true && isDataLossChange)
            {
                sb.AppendLine()
                    .AppendLine($"IF EXISTS (SELECT TOP 1 1 FROM {datalossTable}) BEGIN")
                    .AppendLine($"    RAISERROR('WARNING! This change may cause dataloss to {datalossTable}. Verify and remove this error block to continue.', 0, 1) WITH NOWAIT;")
                    .AppendLine("    IF (@@TRANCOUNT > 0) ROLLBACK;")
                    .AppendLine("    SET NOEXEC ON;")
                    .AppendLine("END").AppendLine();
            }

            sb.Append(_sql);

            sb.EnsureLine().AppendLine()
                .AppendGo()
                .Append(sb.Flatten(SECTION_END))
                .EnsureLine().AppendGo();
        }

        protected abstract void GetFormat(ISqlFileBuilder sb);

        public override string ToString() => _sql;
    }
}
