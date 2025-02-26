using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class FieldDefaultModel(FieldModel field, string? name, string value)
    : IModel<FieldDefaultModel, FieldModel>, IDependentModel
{
    public FieldModel Field { get; } = field;
    public string Name { get; } = name ?? string.Empty;
    public bool IsSystemNamed { get; } = name?.Length is null or 0;
    public string FullName => $"{Field.FullName}:{(IsSystemNamed ? "*(unnamed)*" : $"[{Name}]")}";
    public string Value { get; } = value.ReduceBrackets();
    public string[] Dependencies { get; set; } = [];

    public override bool Equals(object? obj)
    {
        return obj is FieldDefaultModel field
            && Value.ScrubSQL() == field.Value.ScrubSQL();
    }

    public override int GetHashCode()
    {
        return new object?[]
        {
            FullName,
            IsSystemNamed,
            Value
        }.CalculateHashCode();
    }
}
