using System.Text;

namespace DacpacDiff.Core.Model
{
    public class UserTypeFieldModel : IModel<UserTypeFieldModel, UserTypeModel>
    {
        public UserTypeModel UserType { get; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Computation { get; set; }
        public string Default { get; set; }
        public string Unique { get; set; }
        public int Order { get; set; }
        public bool Nullable { get; set; }
        public bool Identity { get; set; }
        public bool IsPrimaryKey { get; set; }

        private UserTypeFieldModel()
        {
        }
        public UserTypeFieldModel(UserTypeModel userType)
        {
            UserType = userType;
        }

        // TODO: To MSSQL library
        public string GetTableSql()
        {
            var sql = new StringBuilder($"[{Name}]");

            if ((Computation?.Length ?? 0) > 0)
            {
                sql.Append($" AS {Computation}");
            }
            else
            {
                sql.Append($" {Type}");

                if (Nullable)
                {
                    sql.Append(" NULL");
                }
                else
                {
                    sql.Append(" NOT NULL");
                }
            }

            if (Identity)
            {
                sql.Append(" IDENTITY(1,1)");
            }

            if (IsPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");
            }

            if ((Default?.Length ?? 0) > 0)
            {
                sql.Append($" DEFAULT{Default}");
            }

            if ((Unique?.Length ?? 0) > 0)
            {
                sql.Append(" UNIQUE");
            }

            return sql.ToString();
        }
    }
}
