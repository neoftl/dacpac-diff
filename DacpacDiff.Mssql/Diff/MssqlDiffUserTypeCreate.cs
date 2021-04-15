using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffUserTypeCreate : BaseMssqlDiffBlock<DiffUserTypeCreate>
    {
        public MssqlDiffUserTypeCreate(DiffUserTypeCreate diff)
            : base(diff)
        { }

        protected override void GetFormat(ISqlFileBuilder sb)
        {
            if (_diff.UserType.Type != "TABLE")
            {
                sb.AppendLine($"CREATE TYPE {_diff.UserType.FullName} FROM {_diff.UserType.Type}");
                return;
            }

            sb.AppendLine($"CREATE TYPE {_diff.UserType.FullName} AS TABLE")
                .Append('(');

            var first = true;
            foreach (var fld in _diff.UserType.Fields)
            {
                sb.AppendIf(",", first)
                    .AppendLine()
                    .Append("    ").Append(fld.GetTableSql());
                first = false;
            }

            sb.AppendLine().Append(")");
        }
    }
}
