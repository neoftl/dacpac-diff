using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DacpacDiff.Core.Model
{
    public class FieldModel : IModel<FieldModel, TableModel>, IDependentModel
    {
        public static readonly FieldModel Empty = new FieldModel();

        public TableModel Table { get; set; } = TableModel.Empty;
        public string Name { get; set; }
        public string Type { get; set; }
        public string? Computation { get; set; }

        public IDictionary<string, (bool isSystemNamed, string defaultValue)> DefaultConstraint { get; set; }
        public bool HasDefault => (DefaultConstraint?.Count ?? 0) > 0;
        public string? DefaultName => HasDefault ? DefaultConstraint.SingleOrDefault().Key : null;
        public string? DefaultValue => HasDefault ? DefaultConstraint.SingleOrDefault().Value.defaultValue : null;
        public bool IsSystemNamedDefault => HasDefault && DefaultConstraint.SingleOrDefault().Value.isSystemNamed;

        public string? Unique { get; set; }
        public bool IsSystemNamedUnique { get; set; }
        public bool IsUnique => (Unique?.Length ?? 0) > 0;
        public string? UniqueName => IsUnique ? Unique : null;
        public int Order { get; set; }
        public bool Nullable { get; set; }
        public bool Identity { get; set; }

        public RefModel? Ref { get; set; }
        public bool HasReference => Ref is null;
        public string? RefName => Ref?.Name;
        public string? RefTargetTable => Ref?.TargetTable;
        public string? RefTargetField => Ref?.TargetField;
        public bool IsNamedReference => Ref?.IsSystemNamed ?? false;

        public string[] Dependents { get; set; }

        public FieldModel SetState(TableModel table, string name)
        {
            Table = table;

            if (Table.Temporality?.PeriodFieldFrom == Name || Table.Temporality?.PeriodFieldTo == Name)
            {
                Nullable = true;
                DefaultConstraint = null;
            }

            return this;
        }

        public bool IsDefaultMatch(FieldModel field)
        {
            if (!field.HasDefault && !HasDefault)
            {
                return true;
            }
            if (field.HasDefault != HasDefault)
            {
                return false;
            }

            var dbL = DefaultValue;
            while (dbL?.Length > 2 && dbL[0] == '(' && dbL[^1] == ')')
            {
                dbL = dbL[1..^1];
            }
            var dbR = field.DefaultValue;
            while (dbR?.Length > 2 && dbR[0] == '(' && dbR[^1] == ')')
            {
                dbR = dbR[1..^1];
            }

            if (dbL != dbR)
            {
                return false;
            }

            if (field.IsSystemNamedDefault && IsSystemNamedDefault)
            {
                return true;
            }
            return field.DefaultName == DefaultName;
        }

        public string ToDefinition()
        {
            var sql = new StringBuilder($"[{Name}]");

            if ((Computation?.Length ?? 0) > 0)
            {
                sql.Append($" AS {Computation}");
            }
            else
            {
                sql.Append($" {Type}");

                if (Table.Temporality?.PeriodFieldFrom == Name)
                {
                    sql.Append(" GENERATED ALWAYS AS ROW START");
                    return sql.ToString();
                }
                if (Table.Temporality?.PeriodFieldTo == Name)
                {
                    sql.Append(" GENERATED ALWAYS AS ROW END");
                    return sql.ToString();
                }

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

            if (Table.PrimaryKey?.Length == 1 && Table.PrimaryKey[0] == Name)
            {
                sql.Append(" PRIMARY KEY");
            }

            if (HasDefault)
            {
                sql.Append($" DEFAULT{DefaultValue}");
            }

            if ((Unique?.Length ?? 0) > 0)
            {
                sql.Append(" UNIQUE");
            }

            if (Ref != null)
            {
                sql.Append($" REFERENCES {Ref.TargetTable} ([{Ref.TargetField}])");
            }

            return sql.ToString();
        }

        public string GetTableFieldSql()
        {
            var sql = new StringBuilder($"[{Name}]");

            if ((Computation?.Length ?? 0) > 0)
            {
                sql.Append($" AS {Computation}");
            }
            else
            {
                sql.Append($" {Type}");

                if (Table.Temporality?.PeriodFieldFrom == Name)
                {
                    sql.Append(" GENERATED ALWAYS AS ROW START");
                    return sql.ToString();
                }
                if (Table.Temporality?.PeriodFieldTo == Name)
                {
                    sql.Append(" GENERATED ALWAYS AS ROW END");
                    return sql.ToString();
                }

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

            if (HasDefault)
            {
                sql.Append($" DEFAULT{DefaultValue}");
            }

            if ((Unique?.Length ?? 0) > 0)
            {
                sql.Append(" UNIQUE");
            }

            return sql.ToString();
        }

        public string GetAlterSql(bool isCurrentlyNotNullable = false)
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
                else if (isCurrentlyNotNullable)
                {
                    sql.Append(" NOT NULL");
                }
                else if (!HasDefault)
                {
                    sql.Append(" NULL");
                    // TODO: warning?
                }
                else
                {
                    sql.Append(" NOT NULL");
                }
            }

            return sql.ToString();
        }
    }
}
