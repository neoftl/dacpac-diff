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

        public IEnumerable<IDifference> Compare(TableModel? tgt, TableModel? cur)
        {
            // May be a drop/create
            if (tgt is null)
            {
                if (cur is null)
                {
                    return Array.Empty<IDifference>();
                }

                // TODO: drop refs
                // TODO: drop indexes?

                // Drop checks
                return cur.Checks.Select(c => (IDifference)new DiffTableCheckDrop(c))
                    .Append(new DiffObjectDrop(cur)).ToArray();
            }
            if (cur is null)
            {
                return new IDifference[] { new DiffTableCreate(tgt) };
            }

            var result = new List<IDifference>();

            // TODO: Change primary key
            // TODO: Refs
            // TODO: Change temporality?
            // TODO: Change field order as a specific diff (optional)

            // Fields
            var tgtFields = tgt.Fields.ToDictionary(f => f.Name);
            var curFields = cur.Fields.ToDictionary(f => f.Name);
            var fldCompr = _comparerFactory.GetComparer<FieldModel>();
            var diffs = tgtFields.Keys.Union(curFields.Keys).Distinct().SelectMany(k =>
                fldCompr.Compare(tgtFields.Get(k), curFields.GetValueOrDefault(k))
            );
            result.AddRange(diffs);

            // Checks
            var chkCompr = _comparerFactory.GetComparer<TableCheckModel>();
            var curChecks = cur.Checks.ToList();
            foreach (var chkL in tgt.Checks)
            {
                var chkR = curChecks.FirstOrDefault(c => chkL.IsSystemNamed && c.IsSystemNamed ? c.Definition.ScrubSQL() == chkL.Definition.ScrubSQL() : c.Name == chkL.Name);
                if (chkR != null)
                {
                    curChecks.Remove(chkR);
                }
                result.AddRange(chkCompr.Compare(chkL, chkR));
            }
            foreach (var chkR in curChecks)
            {
                result.AddRange(chkCompr.Compare(null, chkR));
            }

            // TODO: If any changes, do we need to drop all indexes and recreate / rebuild

            return result.ToArray();
        }
    }
}
