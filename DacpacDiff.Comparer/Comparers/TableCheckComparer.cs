using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class TableCheckComparer : IModelComparer<TableCheckModel>
    {
        public IEnumerable<IDifference> Compare(TableCheckModel? lft, TableCheckModel? rgt)
        {
            var diffs = new List<IDifference>();
            if (lft == null)
            {
                diffs.Add(new DiffTableCheckDrop(rgt ?? lft));
            }
            else if (rgt == null)
            {
                diffs.Add(new DiffTableCheckCreate(lft));
            }
            else if (lft.Definition != rgt.Definition)
            {
                diffs.Add(new DiffTableCheckAlter(lft, rgt));
            }

            return diffs.ToArray();
        }
    }
}
