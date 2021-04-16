using System;

namespace DacpacDiff.Core.Model
{
    public class FieldModel : IModel<FieldModel, TableModel>, IDependentModel, IEquatable<FieldModel>
    {
        public static readonly FieldModel Empty = new FieldModel();

        public TableModel Table { get; }
        public string Name { get; }
        public string? Type { get; set; }
        public string? Computation { get; set; }

        public FieldDefaultModel? Default { get; set; }
        public bool HasDefault => Default?.Value is not null;
        public string? DefaultName => Default?.Name;
        public string? DefaultValue => Default?.Value;
        public bool IsDefaultSystemNamed => Default?.IsSystemNamed == true;

        public string? Unique { get; set; }
        public bool IsUnique => (Unique?.Length ?? 0) > 0;
        public string? UniqueName => IsUnique ? Unique : null;
        public bool IsUniqueSystemNamed { get; set; }

        public int Order { get; set; }
        public bool Nullable { get; set; }
        public bool Identity { get; set; }

        public RefModel? Ref { get; set; }
        public bool HasReference => Ref is not null;
        public string? RefName => Ref?.Name;
        public string? RefTargetTable => Ref?.TargetTable;
        public string? RefTargetField => Ref?.TargetField;
        public bool IsNamedReference => Ref?.IsSystemNamed ?? false;

        public string[] Dependents { get; set; } = Array.Empty<string>();

        private FieldModel()
        {
            Table = TableModel.Empty;
            Name = string.Empty;
        }
        public FieldModel(TableModel table, string name)
        {
            Table = table;
            Name = name;
        }

        public bool Equals(FieldModel? other)
        {
            if (other is null)
            {
                return false;
            }

            bool eq<T>(Func<FieldModel, T?> fn) where T : IEquatable<T>
            {
                var l = fn(this);
                var r = fn(other);
                return (l is null && r is null) || l?.Equals(r) == true;
            }
            return eq(m => m.Table.FullName)
                && eq(m => m.Name)
                && eq(m => m.Type)
                && eq(m => m.Computation)
                && IsDefaultMatch(other)
                && eq(m => m.IsUnique)
                && eq(m => m.UniqueName)
                && eq(m => m.IsUniqueSystemNamed)
                //&& eq(m => m.Order) // TODO: Table field ordering to separate option and diff
                && eq(m => m.Nullable)
                && eq(m => m.Identity)
                && eq(m => m.HasReference)
                && eq(m => m.RefName)
                && eq(m => m.RefTargetTable)
                && eq(m => m.RefTargetField)
                && eq(m => m.IsNamedReference);
        }
        public override bool Equals(object? obj) => Equals(obj as FieldModel);

        public bool IsSignatureMatch(FieldModel other)
        {
            bool eq<T>(Func<FieldModel, T?> fn) where T : IEquatable<T>
            {
                var l = fn(this);
                var r = fn(other);
                return (l is null && r is null) || l?.Equals(r) == true;
            }
            return eq(m => m.Type)
                && eq(m => m.Computation)
                && IsDefaultMatch(other)
                && eq(m => m.IsUnique);
        }

        public override int GetHashCode()
        {
            // TODO
            return base.GetHashCode();
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

            if (field.IsDefaultSystemNamed && IsDefaultSystemNamed)
            {
                return true;
            }
            return field.DefaultName == DefaultName;
        }
    }
}
