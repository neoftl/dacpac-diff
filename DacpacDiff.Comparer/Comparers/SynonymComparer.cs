using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class SynonymComparer : IComparer<SynonymModel>
    {
        public IEnumerable<IDifference> Compare(SynonymModel? lft, SynonymModel? rgt)
        {
            // May be a drop/create
            if (lft == null)
            {
                return new[] { new DiffObjectDrop(rgt) };
            }
            if (rgt == null)
            {
                return new[] { new DiffSynonymCreate(lft) };
            }

            // Alter
            if (lft.BaseObject != rgt.BaseObject)
            {
                return new[] { new DiffSynonymAlter(lft) };
            }

            return System.Array.Empty<IDifference>();
        }
    }
}
