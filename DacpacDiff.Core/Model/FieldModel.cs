using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class FieldModel : IModel<FieldModel, TableModel>, IDependentModel, IEquatable<FieldModel>
{
    public static readonly FieldModel Empty = new();

    public TableModel Table { get; }
    public SchemaModel Schema => Table.Schema;
    public DatabaseModel Db => Schema.Db;

    public string Name { get; }
    public string FullName => $"{Table.FullName}.[{Name}]";
    public string? Type { get; set; }
    public string? Collation { get; set; }
    public string? Computation { get; set; }

    public FieldDefaultModel? Default { get; set; }
    public bool HasDefault => Default?.Value is not null;
    public string? DefaultName => Default?.Name;
    public string? DefaultValue => Default?.Value;
    public bool IsDefaultSystemNamed => Default?.IsSystemNamed == true;

    public int Order { get; set; }
    public bool Nullable { get; set; }
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool Identity { get; set; } // TODO: seed,inc

    public FieldRefModel? Ref { get; set; }
    public bool HasReference => Ref is not null;

    public string[] Dependencies => (Default?.Dependencies ?? Array.Empty<string>())
        .Append(Ref?.TargetField.Table == Table ? null : Ref?.TargetField.Table.Name)
        .NotNull().ToArray();
    public IModel[] Dependents => Db.FindAllDependents(this).ToArray();

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
    public FieldModel(FieldModel field)
    {
        Table = field.Table;
        Name = field.Name;
        Type = field.Type;
        Collation = field.Collation;
        Computation = field.Computation;
        Order = field.Order;
        Nullable = field.Nullable;
        IsUnique = field.IsUnique;
        IsPrimaryKey = field.IsPrimaryKey;
        Identity = field.Identity;

        if (field.Default != null)
        {
            Default = new FieldDefaultModel(this, field.Default.Name, field.Default.Value);
        }
        if (field.Ref != null)
        {
            Ref = new FieldRefModel(this, field.Ref.TargetField)
            {
                Name = field.Ref.Name,
                IsSystemNamed = field.Ref.IsSystemNamed,
            };
        }
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
        return eq(m => m.FullName)
            && eq(m => m.Type)
            && eq(m => m.Collation)
            && eq(m => m.Computation?.ScrubSQL())
            && IsDefaultMatch(other)
            && eq(m => m.IsUnique)
            //&& eq(m => m.Order) // TODO: Table field ordering to separate option and diff
            && eq(m => m.Nullable)
            && eq(m => m.Identity)
            && (Ref is null) == (other.Ref is null) && Ref?.Equals(other.Ref) != false;
    }
    public override bool Equals(object? obj) => Equals(obj as FieldModel);

    public bool IsSignatureMatch(FieldModel other, bool checkDefault)
    {
        bool eq<T>(Func<FieldModel, T?> fn) where T : IEquatable<T>
        {
            var l = fn(this);
            var r = fn(other);
            return (l is null && r is null) || l?.Equals(r) == true;
        }
        return eq(m => m.Type)
            && eq(m => m.Collation)
            && eq(m => m.Computation)
            && eq(m => m.Nullable)
            && (!checkDefault || IsDefaultMatch(other));
    }

    public override int GetHashCode()
    {
        return new object?[]
        {
            FullName,
            Type,
            Collation,
            Computation,
            Default,
            IsUnique,
            Order,
            Nullable,
            Identity,
            Ref
        }.CalculateHashCode();
    }

    public bool IsDefaultMatch(FieldModel field)
    {
        if (DefaultValue != null)
        {
            if (field.DefaultValue == null)
            {
                return false;
            }

            var dbL = DefaultValue.ScrubSQL();
            var dbR = field.DefaultValue.ScrubSQL();
            return dbL == dbR;
        }

        return !field.HasDefault;
    }
}
