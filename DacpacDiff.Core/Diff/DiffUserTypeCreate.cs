using DacpacDiff.Core.Model;
using System.Text;

namespace DacpacDiff.Core.Diff
{
    public class DiffUserTypeCreate : IDifference
    {
        public UserTypeModel UserType { get; }

        public IModel Model => UserType;
        public string Title => "Create user type";
        public string Name => UserType.FullName;

        public DiffUserTypeCreate(UserTypeModel utype)
        {
            UserType = utype;

            if (utype.Type == "TABLE")
            {
                foreach (var fld in utype.Fields)
                {
                    fld.SetState(utype, null);
                }
            }
        }
    }
}
