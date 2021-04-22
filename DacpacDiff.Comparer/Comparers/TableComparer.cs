using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers
{
    public class TableComparer : IModelComparer<TableModel>
    {
        private readonly IModelComparerFactory _comparerFactory;

        public TableComparer(IModelComparerFactory comparerFactory)
        {
            _comparerFactory = comparerFactory;
        }

        public IEnumerable<IDifference> Compare(TableModel? lft, TableModel? rgt)
        {
            var diffs = new List<IDifference>();

            // May be a drop/create
            if (lft is null)
            {
                if (rgt is null)
                {
                    return Array.Empty<IDifference>();
                }

                // TODO: drop refs
                // TODO: drop indexes?

                // Drop checks
                diffs.AddRange(rgt.Checks?.Select(c => new DiffTableCheckDrop(c)).ToArray() ?? Array.Empty<IDifference>());

                diffs.Add(new DiffObjectDrop(rgt));
                return diffs.ToArray();
            }
            if (rgt is null)
            {
                diffs.Add(new DiffTableCreate(lft));

                // Will need to create all named references separately
                diffs.AddRange(lft.Fields.Where(f => f.Ref?.IsSystemNamed == false && f.Ref is not null)
                    .Select(f => new DiffRefCreate(f.Ref ?? FieldRefModel.Empty)));

                return diffs.ToArray();
            }

            // TODO: Change primary key
            // TODO: Refs
            // TODO: Change temporality?

            // Fields
            var fldCompr = _comparerFactory.GetComparer<FieldModel>();
            foreach (var fldL in lft.Fields ?? Array.Empty<FieldModel>())
            {
                var fldR = rgt.Fields?.FirstOrDefault(f => f.Name == fldL.Name);
                diffs.AddRange(fldCompr.Compare(fldL, fldR) ?? Array.Empty<IDifference>());
            }
            foreach (var fldR in rgt.Fields ?? Array.Empty<FieldModel>())
            {
                if (!(lft.Fields?.Any(f => f.Name == f.Name) ?? false))
                {
                    diffs.AddRange(fldCompr.Compare(null, fldR) ?? Array.Empty<IDifference>());
                }
            }

            // Checks
            // TODO: support unnamed
            var chkCompr = _comparerFactory.GetComparer<TableCheckModel>();
            foreach (var chkL in lft.Checks ?? Array.Empty<TableCheckModel>())
            {
                var fldR = rgt.Checks?.FirstOrDefault(c => c.Name == chkL.Name);
                diffs.AddRange(chkCompr.Compare(chkL, fldR) ?? Array.Empty<IDifference>());
            }
            foreach (var chkR in rgt.Checks ?? Array.Empty<TableCheckModel>())
            {
                if (!(lft.Checks?.Any(c => c.Name == chkR.Name) ?? false))
                {
                    diffs.AddRange(chkCompr.Compare(null, chkR) ?? Array.Empty<IDifference>());
                }
            }
            
            // TODO: If any changes, do we need to drop all indexes and recreate?

            return diffs.ToArray();
        }
    }
}
