namespace DacpacDiff.Core.Model
{
    public class UserTypeModel : IModel<UserTypeModel, SchemaModel>, IModelInSchema
    {
        public static readonly UserTypeModel Empty = new UserTypeModel();

        public SchemaModel Schema { get; private set; } = SchemaModel.Empty;
        public string Name { get; set; }
        public string FullName => $"[{Schema.Name}].[{Name}]";
        public string Type { get; set; }
        public UserTypeFieldModel[] Fields { get; set; }

        public UserTypeModel SetState(SchemaModel schema, string name)
        {
            Schema = schema;
            Name = name;
            return this;
        }
    }
}
