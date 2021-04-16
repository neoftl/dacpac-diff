namespace DacpacDiff.Core.Model
{
    public class FieldDefaultModel : IModel<FieldDefaultModel, FieldModel>
    {
        public FieldModel Field { get; }
        public string Name { get; }
        public bool IsSystemNamed { get; set; }
        public string Value { get; }

        public FieldDefaultModel(FieldModel field, string? name, string value)
        {
            Field = field;
            Name = name ?? string.Empty;
            IsSystemNamed = Name.Length == 0;
            Value = value;

            if (Value.Length > 2 && Value[0] == '(' && Value[^1] == ')')
            {
                Value = Value[1..^1];
            }
        }
    }
}
