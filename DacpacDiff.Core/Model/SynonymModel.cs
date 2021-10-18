namespace DacpacDiff.Core.Model
{
    public class SynonymModel : IModel<SynonymModel, SchemaModel>, IDependentModel, IModelInSchema
    {
        public SchemaModel Schema { get; }
        public string Name { get; }
        public string FullName => $"[{Schema.Name}].[{Name}]";
        public string BaseObject { get; }
        public string[] Dependencies => new[] { BaseObject };

        private SynonymModel()
        {
            Schema = SchemaModel.Empty;
            Name = string.Empty;
            BaseObject = string.Empty;
        }
        public SynonymModel(SchemaModel schema, string name, string baseObject)
        {
            Schema = schema;
            Name = name;
            BaseObject = baseObject;
        }
    }
}
