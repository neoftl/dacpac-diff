using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Text;

namespace DacpacDiff.Comparer.Diff
{
    public class DiffUserTypeCreate : IDifference
    {
        public UserTypeModel UserType { get; }

        public IModel Model => UserType;
        public string Title => "Create user type";
        public string Name => UserType.FullName;

        public DiffUserTypeCreate(UserTypeModel utype)
        {
            UserType = utype;

            if (utype.Type == "TABLE")
            {
                foreach (var fld in utype.Fields)
                {
                    fld.SetState(utype, null);
                }
            }
        }

        public override string ToString()
        {
            var sql = new StringBuilder();

            if (UserType.Type == "TABLE")
            {
                sql.Append("CREATE TYPE ")
                    .Append(UserType.FullName)
                    .Append(" AS TABLE\r\n")
                    .Append("(");

                var first = true;
                foreach (var fld in UserType.Fields)
                {
                    sql.Append(first ? "\r\n" : ",\r\n")
                        .Append("    ")
                        .Append(fld.GetTableSql());
                    first = false;
                }

                sql.Append("\r\n)");
            }
            else
            {
                sql.Append("CREATE TYPE ")
                    .Append(UserType.FullName)
                    .Append(" FROM ")
                    .Append(UserType.Type);
            }

            return sql.ToString();
        }
    }
}
