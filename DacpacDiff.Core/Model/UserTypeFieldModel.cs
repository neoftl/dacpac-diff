namespace DacpacDiff.Core.Model
{
    public class UserTypeFieldModel : IModel<UserTypeFieldModel, UserTypeModel>
    {
        public UserTypeModel UserType { get; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Computation { get; set; }
        public string? Default { get; set; }
        public string Unique { get; set; }
        public int Order { get; set; }
        public bool Nullable { get; set; }
        public bool Identity { get; set; }
        public bool IsPrimaryKey { get; set; }

        private UserTypeFieldModel()
        {
            UserType = UserTypeModel.Empty;
        }
        public UserTypeFieldModel(UserTypeModel userType)
        {
            UserType = userType;
        }
    }
}
