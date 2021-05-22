namespace DacpacDiff.Core.Model
{
    public class UserTypeFieldModel : IModel<UserTypeFieldModel, UserTypeModel>
    {
        public UserTypeModel UserType { get; }
        public string Name { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Computation { get; set; } = string.Empty;
        public string? Default { get; set; }
        public int Order { get; set; }
        public bool Nullable { get; set; }
        public bool Identity { get; set; }
        public bool IsUnique { get; set; }
        public bool IsPrimaryKey { get; set; }

        private UserTypeFieldModel()
        {
            UserType = UserTypeModel.Empty;
            Name = string.Empty;
        }
        public UserTypeFieldModel(UserTypeModel userType, string name)
        {
            UserType = userType;
            Name = name;
        }
    }
}
