using System;

namespace DacpacDiff.Core.Model
{
    public class FieldDefaultModel : IModel<FieldDefaultModel, FieldModel>, IDependentModel
    {
        public FieldModel Field { get; }
        public string Name { get; }
        public bool IsSystemNamed { get; }
        public string Value { get; }
        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public FieldDefaultModel(FieldModel field, string? name, string value)
        {
            Field = field;
            Name = name ?? string.Empty;
            IsSystemNamed = Name.Length == 0;
            Value = value;

            while (Value.Length > 2 && Value[0] == '(' && Value[^1] == ')') // TODO: if both are odd
            {
                Value = Value[1..^1];
            }
        }
    }
}
