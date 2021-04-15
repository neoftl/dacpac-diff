using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class SynonymComparer : IModelComparer<SynonymModel>
    {
        public IEnumerable<IDifference> Compare(SynonymModel? lft, SynonymModel? rgt)
        {
            // May be a drop/create
            if (lft is null)
            {
                if (rgt is null)
                {
                    return Array.Empty<IDifference>();
                }

                return new[] { new DiffObjectDrop(rgt) };
            }
            if (rgt is null)
            {
                return new[] { new DiffSynonymCreate(lft) };
            }

            // Alter
            if (lft.BaseObject != rgt.BaseObject)
            {
                return new[] { new DiffSynonymAlter(lft) };
            }

            return Array.Empty<IDifference>();
        }
    }
}
