using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class SynonymComparer : IModelComparer<SynonymModel>
    {
        public IEnumerable<IDifference> Compare(SynonymModel? tgt, SynonymModel? cur)
        {
            // May be a drop/create
            if (tgt is null)
            {
                if (cur is null)
                {
                    return Array.Empty<IDifference>();
                }

                return new[] { new DiffObjectDrop(cur) };
            }
            if (cur is null)
            {
                return new[] { new DiffSynonymCreate(tgt) };
            }

            // Alter
            if (tgt.BaseObject != cur.BaseObject)
            {
                return new[] { new DiffSynonymAlter(tgt) };
            }

            return Array.Empty<IDifference>();
        }
    }
}
