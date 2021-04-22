using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class TableCheckComparer : IModelComparer<TableCheckModel>
    {
        public IEnumerable<IDifference> Compare(TableCheckModel? lft, TableCheckModel? rgt)
        {
            // May be a drop/create
            if (lft is null)
            {
                if (rgt is null)
                {
                    return Array.Empty<IDifference>();
                }

                return new[] { new DiffTableCheckDrop(rgt) };
            }
            if (rgt is null)
            {
                return new[] { new DiffTableCheckCreate(lft) };
            }

            // Alter
            if (lft.Definition.ScrubSQL() != rgt.Definition.ScrubSQL())
            {
                return new[] { new DiffTableCheckAlter(lft, rgt) };
            }

            return Array.Empty<IDifference>();
        }
    }
}
