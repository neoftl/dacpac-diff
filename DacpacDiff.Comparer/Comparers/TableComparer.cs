using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
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
                return rgt.Checks.Select(c => (IDifference)new DiffTableCheckDrop(c))
                    .Append(new DiffObjectDrop(rgt)).ToArray();
            }
            if (rgt is null)
            {
                return new IDifference[] { new DiffTableCreate(lft) };
            }

            var result = new List<IDifference>();

            // TODO: Change primary key
            // TODO: Refs
            // TODO: Change temporality?
            // TODO: Change field order as a specific diff (optional)

            // Fields
            var lftFields = lft.Fields.ToDictionary(f => f.Name);
            var rgtFields = rgt.Fields.ToDictionary(f => f.Name);
            var fldCompr = _comparerFactory.GetComparer<FieldModel>();
            var diffs = lftFields.Keys.Union(rgtFields.Keys).Distinct().SelectMany(k =>
                fldCompr.Compare(lftFields.Get(k), rgtFields.GetValueOrDefault(k))
            );
            result.AddRange(diffs);

            // Checks
            var chkCompr = _comparerFactory.GetComparer<TableCheckModel>();
            var rgtChecks = rgt.Checks.ToList();
            foreach (var chkL in lft.Checks)
            {
                var chkR = rgtChecks.FirstOrDefault(c => chkL.IsSystemNamed && c.IsSystemNamed ? c.Definition == chkL.Definition : c.Name == chkL.Name);
                if (chkR != null)
                {
                    rgtChecks.Remove(chkR);
                }
                result.AddRange(chkCompr.Compare(chkL, chkR));
            }
            foreach (var chkR in rgtChecks)
            {
                result.AddRange(chkCompr.Compare(null, chkR));
            }

            // TODO: If any changes, do we need to drop all indexes and recreate / rebuild

            return result.ToArray();
        }
    }
}
