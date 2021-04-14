namespace DacpacDiff.Core.Model
{
    public class SynonymModel : IModel<SynonymModel, SchemaModel>, IDependentModel, IModelInSchema
    {
        public SchemaModel Schema { get; set; } = SchemaModel.Empty;
        public string Name { get; set; }
        public string FullName => $"[{Schema.Name}].[{Name}]";
        public string? BaseObject { get; set; }
        public string[] Dependents { get; set; }

        public SynonymModel SetState(SchemaModel schema, string name)
        {
            Schema = schema;
            Name = name;
            return this;
        }
    }
}
