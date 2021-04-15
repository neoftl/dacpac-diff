using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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

            fixFieldRef(lft);
            fixFieldRef(rgt);
            
            // TODO: If field has dependencies (constraint, index, etc.), drop first then recreate

            // Change field
            if (!lft.Equals(rgt))
            {
                return new[] { new DiffFieldAlter(lft, rgt) };
            }

            return Array.Empty<IDifference>();
        }

        private static void fixFieldRef(FieldModel fld)
        {
            if (fld.Table.Refs.Count > 0)
            {
                // Copy named reference to field
                var tref = fld.Table.Refs.Values.Where(r => r.TargetField == fld.Name).FirstOrDefault();
                if (tref is not null)
                {
                    fld.Ref = new RefModel(tref);
                }
            }
        }
    }
}
