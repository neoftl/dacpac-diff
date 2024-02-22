using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class TableModel : IModel, IDependentModel, IModelInSchema, IEquatable<TableModel>
{
    public static readonly TableModel Empty = new();

    public SchemaModel Schema { get; }
    public DatabaseModel Db => Schema.Db;

    public string Name { get; }
    public string FullName => $"[{Schema.Name}].[{Name}]";

    public IList<TableCheckModel> Checks { get; set; } = new List<TableCheckModel>();
    public FieldModel[] Fields { get; set; } = Array.Empty<FieldModel>();
    public FieldModel[] PrimaryKeys => Fields.Where(f => f.IsPrimaryKey).ToArray();
    public string? PrimaryKeyName { get; set; }
    public bool IsPrimaryKeySystemNamed => !(PrimaryKeyName?.Length > 0);
    public bool IsPrimaryKeyUnclustered { get; set; }
    public TemporalityModel Temporality { get; set; } = TemporalityModel.Empty;
    public string[] Dependencies => Fields.SelectMany(f => f.Dependencies).Distinct().ToArray();

    private TableModel()
    {
        Schema = SchemaModel.Empty;
        Name = string.Empty;
    }
    public TableModel(SchemaModel schema, string name)
    {
        Schema = schema;
        Name = name;
    }

    public override int GetHashCode()
    {
        return new object?[]
        {
            FullName
        }.CalculateHashCode();
    }

    public bool Equals(TableModel? other)
    {
        return this.IsEqual(other,
            m => m.FullName,
            m => m.Checks,
            m => m.Fields,
            m => m.PrimaryKeys,
            m => m.PrimaryKeyName,
            m => m.IsPrimaryKeyUnclustered,
            m => m.Temporality);
    }
    public override bool Equals(object? obj) => Equals(obj as TableModel);
}
