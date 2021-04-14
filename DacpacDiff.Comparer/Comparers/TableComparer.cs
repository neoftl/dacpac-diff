using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers
{
    public class TableComparer : IComparer<TableModel>
    {
        private readonly IComparerFactory _comparerFactory;

        public TableComparer(IComparerFactory comparerFactory)
        {
            _comparerFactory = comparerFactory;
        }

        public IEnumerable<IDifference> Compare(TableModel? lft, TableModel? rgt)
        {
            var diffs = new List<IDifference>();

            // May be a drop/create
            if (lft == null)
            {
                // TODO: drop refs
                // TODO: drop indexes?

                // Drop checks
                diffs.AddRange(rgt.Checks?.Select(c => new DiffTableCheckDrop(c)).ToArray() ?? Array.Empty<IDifference>());

                diffs.Add(new DiffObjectDrop(rgt));
                return diffs.ToArray();
            }
            if (rgt == null)
            {
                diffs.Add(new DiffTableCreate(lft));
                diffs.AddRange(lft.Fields.Where(f => f.HasReference).Select(f => new DiffRefCreate(f)));
                return diffs.ToArray();
            }

            // TODO: Change primary key
            // TODO: Refs
            // TODO: Change temporality?

            // Fields
            var fldCompr = _comparerFactory.GetComparer<FieldModel>();
            foreach (var fldL in lft.Fields ?? Array.Empty<FieldModel>())
            {
                fldL.SetState(lft, null);
                var fldR = rgt.Fields?.FirstOrDefault(f => f.Name == fldL.Name);
                fldR?.SetState(rgt, null);
                diffs.AddRange(fldCompr.Compare(fldL, fldR) ?? Array.Empty<IDifference>());
            }
            foreach (var fldR in rgt.Fields ?? Array.Empty<FieldModel>())
            {
                fldR.SetState(rgt, null);
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
                chkL.SetState(lft, null);
                var fldR = rgt.Checks?.FirstOrDefault(c => c.Name == chkL.Name);
                fldR?.SetState(rgt, null);
                diffs.AddRange(chkCompr.Compare(chkL, fldR) ?? Array.Empty<IDifference>());
            }
            foreach (var chkR in rgt.Checks ?? Array.Empty<TableCheckModel>())
            {
                chkR.SetState(rgt, null);
                if (!(lft.Checks?.Any(c => c.Name == chkR.Name) ?? false))
                {
                    diffs.AddRange(chkCompr.Compare(null, chkR) ?? Array.Empty<IDifference>());
                }
            }

            return diffs.ToArray();
        }
    }
}
