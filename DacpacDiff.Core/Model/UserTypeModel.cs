using System;

namespace DacpacDiff.Core.Model
{
    public class UserTypeModel : IModel<UserTypeModel, SchemaModel>, IModelInSchema
    {
        public static readonly UserTypeModel Empty = new();

        public SchemaModel Schema { get; }
        public string Name { get; }
        public string FullName => $"[{Schema.Name}].[{Name}]";
        public string Type { get; set; } = string.Empty;
        public UserTypeFieldModel[] Fields { get; set; } = Array.Empty<UserTypeFieldModel>();

        private UserTypeModel()
        {
            Schema = SchemaModel.Empty;
            Name = string.Empty;
        }
        public UserTypeModel(SchemaModel schema, string name)
        {
            Schema = schema;
            Name = name;
        }
    }
}
