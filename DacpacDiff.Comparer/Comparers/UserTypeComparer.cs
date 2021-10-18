using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class UserTypeComparer : IModelComparer<UserTypeModel>
    {
        public IEnumerable<IDifference> Compare(UserTypeModel? lft, UserTypeModel? rgt)
        {
            var diffs = new List<IDifference>();

            // May be a drop/create
            if (lft == null)
            {
                // TODO
            }
            else if (rgt == null)
            {
                diffs.Add(new DiffUserTypeCreate(lft));
            }
            else
            {
                // TODO: alter
            }

            return diffs.ToArray();
        }
    }
}
