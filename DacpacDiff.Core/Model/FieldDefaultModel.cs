namespace DacpacDiff.Core.Model
{
    public class FieldDefaultModel : IModel<FieldDefaultModel, FieldModel>
    {
        public FieldModel Field { get; }
        public string Name { get; }
        public bool IsSystemNamed { get; set; }
        public string? Value { get; set; }

        public FieldDefaultModel(FieldModel field, string name)
        {
            Field = field;
            Name = name;
        }
    }
}
