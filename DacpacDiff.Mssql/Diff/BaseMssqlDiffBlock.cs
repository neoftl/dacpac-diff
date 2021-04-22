using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Mssql.Diff
{
    public abstract class BaseMssqlDiffBlock<T> : ISqlFormatter
        where T : IDifference
    {
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
            sb.AppendLine("BEGIN TRAN")
                .AppendLine(sb.Flatten(@"DECLARE @R INT
EXEC @R = #usp_BlockStart
IF (@R <> 1) SET NOEXEC ON"))
                .AppendGo().AppendLine();

            string? datalossTable = null;
            var isDataLossChange = _diff is IDataLossChange d && d.GetDataLossTable(out datalossTable);
            if (sb.Options?.DisableDatalossCheck != true && isDataLossChange)
            {
                sb.EnsureLine(2)
                    .AppendLine($"IF EXISTS (SELECT TOP 1 1 FROM {datalossTable}) BEGIN")
                    .AppendLine($"    RAISERROR('WARNING! This change may cause dataloss to {datalossTable}. Verify and remove this error block to continue.', 15, 1) WITH NOWAIT")
                    .AppendLine("    IF (@@TRANCOUNT > 0) ROLLBACK")
                    .AppendLine("    SET NOEXEC ON")
                    .AppendLine("END").AppendLine();
            }

            sb.Append(_sql);

            sb.EnsureLine(2)
                .AppendGo()
                .AppendLine(sb.Flatten(@"IF (@@ERROR <> 0) BEGIN
    EXEC #print 'Failed'
    IF (@@TRANCOUNT > 0) ROLLBACK
    SET NOEXEC ON
END ELSE IF (@@TRANCOUNT <> 2) BEGIN
    EXEC #print 'Failed: Transaction mismatch (', @@TRANCOUNT, ')'
    IF (@@TRANCOUNT > 0) ROLLBACK
    SET NOEXEC ON
END"))
                .AppendLine("COMMIT")
                .AppendGo();
        }

        protected abstract void GetFormat(ISqlFileBuilder sb);

        public override string ToString() => _sql;
    }
}
