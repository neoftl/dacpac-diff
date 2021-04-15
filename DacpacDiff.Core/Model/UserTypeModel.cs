using System;

namespace DacpacDiff.Core.Model
{
    public class UserTypeModel : IModel<UserTypeModel, SchemaModel>, IModelInSchema
    {
        public static readonly UserTypeModel Empty = new UserTypeModel();

        public SchemaModel Schema { get; }
        public string Name { get; set; }
        public string FullName => $"[{Schema.Name}].[{Name}]";
        public string Type { get; set; }
        public UserTypeFieldModel[] Fields { get; set; } = Array.Empty<UserTypeFieldModel>();

        private UserTypeModel()
        {
            Schema = SchemaModel.Empty;
        }
        public UserTypeModel(SchemaModel schema)
        {
            Schema = schema;
        }
    }
}
