using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class FieldComparer : IModelComparer<FieldModel>
    {
        public IEnumerable<IDifference> Compare(FieldModel? lft, FieldModel? rgt)
        {
            // May be a drop/create
            if (lft is null)
            {
                if (rgt is null)
                {
                    return Array.Empty<IDifference>();
                }

                return new[] { new DiffFieldDrop(rgt) };
            }
            if (rgt is null)
            {
                return new[] { new DiffFieldCreate(lft) };
            }

            // TODO: If field has dependencies (constraint, index, etc.), drop first then recreate
            // TODO: drop/create/alter ref can be separate

            // Change field
            if (!lft.Equals(rgt))
            {
                return new[] { new DiffFieldAlter(lft, rgt) };
            }

            return Array.Empty<IDifference>();
        }
    }
}
