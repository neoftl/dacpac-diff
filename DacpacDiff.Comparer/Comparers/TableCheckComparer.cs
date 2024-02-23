using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class TableCheckComparer : IModelComparer<TableCheckModel>
    {
        public IEnumerable<IDifference> Compare(TableCheckModel? tgt, TableCheckModel? cur)
        {
            // May be a drop/create
            if (tgt is null)
            {
                if (cur is null)
                {
                    return Array.Empty<IDifference>();
                }

                return new[] { new DiffTableCheckDrop(cur) };
            }
            if (cur is null)
            {
                return new[] { new DiffTableCheckCreate(tgt) };
            }

            // Alter
            if (tgt.Definition.ScrubSQL() != cur.Definition.ScrubSQL())
            {
                return new[] { new DiffTableCheckAlter(tgt, cur) };
            }

            return Array.Empty<IDifference>();
        }
    }
}
