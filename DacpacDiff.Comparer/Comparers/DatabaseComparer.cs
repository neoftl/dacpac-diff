using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DacpacDiff.Comparer.Comparers
{
    public class DatabaseComparer : IComparer<DatabaseModel>
    {
        private readonly IComparerFactory _comparerFactory;

        public DatabaseComparer(IComparerFactory comparerFactory)
        {
            _comparerFactory = comparerFactory;
        }

        public IEnumerable<IDifference> Compare(DatabaseModel? lft, DatabaseModel? rgt)
        {
            // TODO: others

            // Schemas
            var keys = (lft?.Schemas?.Keys ?? Array.Empty<string>())
                .Union(rgt?.Schemas.Keys ?? Array.Empty<string>())
                .Distinct();
            var schCompr = _comparerFactory.GetComparer<SchemaModel>();
            var diffs1 = keys.SelectMany(k =>
                schCompr.Compare(lft?.Schemas?.Get(k)?.SetState(lft, k), rgt?.Schemas?.Get(k)?.SetState(rgt, k)) ?? Array.Empty<IDifference>()
            );

            return diffs1.ToArray();
        }
    }
}
