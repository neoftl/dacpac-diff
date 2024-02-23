using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class UserTypeComparer : IModelComparer<UserTypeModel>
    {
        public IEnumerable<IDifference> Compare(UserTypeModel? tgt, UserTypeModel? cur)
        {
            var diffs = new List<IDifference>();

            // May be a drop/create
            if (tgt == null)
            {
                // TODO
            }
            else if (cur == null)
            {
                diffs.Add(new DiffUserTypeCreate(tgt));
            }
            else
            {
                // TODO: alter
            }

            return diffs.ToArray();
        }
    }
}
