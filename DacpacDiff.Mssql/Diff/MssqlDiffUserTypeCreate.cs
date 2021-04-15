using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlDiffUserTypeCreate : BaseMssqlDiffBlock<DiffUserTypeCreate>
    {
        public MssqlDiffUserTypeCreate(DiffUserTypeCreate diff)
            : base(diff)
        { }
        
        private static void appendFieldSql(UserTypeFieldModel fld, ISqlFileBuilder sb)
        {
            sb.Append($"[{fld.Name}]");

            if ((fld.Computation?.Length ?? 0) > 0)
            {
                sb.Append($" AS {fld.Computation}");
            }
            else
            {
                sb.Append($" {fld.Type}")
                    .Append(fld.Nullable ? " NULL" : " NOT NULL ")
                    .AppendIf($" DEFAULT{fld.Default}", (fld.Default?.Length ?? 0) > 0)
                    .AppendIf(" PRIMARY KEY", fld.IsPrimaryKey)
                    .AppendIf(" IDENTITY(1,1)", fld.Identity);
            }

            sb.AppendIf(" UNIQUE", (fld.Unique?.Length ?? 0) > 0);
        }

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
                    .Append("    ");
                appendFieldSql(fld, sb);
                first = false;
            }

            sb.AppendLine().Append(")");
        }
    }
}
