using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System.Diagnostics.CodeAnalysis;

namespace DacpacDiff.Mssql.Diff;

public abstract class BaseMssqlDiffBlock<T> : ISqlFormatter
    where T : IDifference
{
    protected readonly T _diff;

    private class NullSqlBuilder : BaseSqlFileBuilder
    {
        [ExcludeFromCodeCoverage(Justification = "Not application logic")]
        public override string Generate(string targetFileName, string currentFileName, string targetVersion, IEnumerable<ISqlFormattable> diffs)
            => throw new NotImplementedException();

        public override string ToString() => _sql.ToString();
    }

    protected BaseMssqlDiffBlock(T diff)
    {
        _diff = diff ?? throw new ArgumentNullException(nameof(diff));
    }

    public void Format(ISqlFileBuilder sb)
    {
        sb.AppendLine(sb.Flatten(@"EXEC #usp_CheckState 1
BEGIN TRAN
IF (dbo.tmpIsActive() = 0) SET NOEXEC ON"))
            .AppendGo().AppendLine();

        string? datalossTable = null;
        var isDataLossChange = _diff is IDataLossChange d && d.GetDataLossTable(out datalossTable);
        if (sb.Options?.DisableDatalossCheck != true && isDataLossChange)
        {
            sb.EnsureLine(2)
                .AppendLine($"IF EXISTS (SELECT TOP 1 1 FROM {datalossTable}) BEGIN")
                .AppendLine($"    EXEC #print 1, '[WARN] This change may cause dataloss to {datalossTable}. Verify and remove this error block to continue.'")
                .AppendLine("    IF (@@TRANCOUNT > 0) ROLLBACK")
                .AppendLine("    SET NOEXEC ON")
                .AppendLine("END").AppendLine();
        }

        sb.Append(ToString());

        sb.EnsureLine(2)
            .AppendGo()
            .AppendLine(sb.Flatten(@"SET NOEXEC OFF
EXEC #usp_CheckState 2
IF (dbo.tmpIsActive() = 0) SET NOEXEC ON
COMMIT"))
            .AppendGo();
    }

    protected abstract void GetFormat(ISqlFileBuilder sb);

    protected string? _sql = null;
    public override string ToString()
    {
        if (_sql == null)
        {
            var sb = new NullSqlBuilder();
            GetFormat(sb);
            _sql = sb.ToString().Trim();
        }
        return _sql;
    }
}
