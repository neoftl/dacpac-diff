using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff;

public class MssqlDiffModuleAlter : BaseMssqlDiffBlock<DiffModuleAlter>
{
    public MssqlDiffModuleAlter(DiffModuleAlter diff)
        : base(diff)
    {
    }

    protected override void GetFormat(ISqlFileBuilder sb)
    {
        string sql;
        if (_diff.AsDropCreate)
        {
            var dropDiff = new DiffObjectDrop(_diff.Module);
            sql = new MssqlDiffObjectDrop(dropDiff).ToString();
            sb.Append(sql).EnsureLine().AppendGo();
        }

        sql = new MssqlDiffModuleCreate(new DiffModuleCreate(_diff.Module))
        {
            DoAsAlter = !_diff.AsDropCreate,
            UseStub = false
        }.ToString();
        sb.Append(sql).EnsureLine();
    }
}