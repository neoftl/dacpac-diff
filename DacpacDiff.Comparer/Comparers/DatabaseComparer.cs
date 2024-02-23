using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;

namespace DacpacDiff.Comparer.Comparers;

public class DatabaseComparer : IModelComparer<DatabaseModel>
{
    private readonly IModelComparerFactory _comparerFactory;

    public DatabaseComparer(IModelComparerFactory comparerFactory)
    {
        _comparerFactory = comparerFactory;
    }

    public IEnumerable<IDifference> Compare(DatabaseModel? tgt, DatabaseModel? cur)
    {
        // TODO: others

        // Schemas
        var keys = (tgt?.Schemas.Keys ?? Array.Empty<string>())
            .Union(cur?.Schemas.Keys ?? Array.Empty<string>())
            .Distinct();
        var schCompr = _comparerFactory.GetComparer<SchemaModel>();
        var diffs1 = keys.SelectMany(k =>
            schCompr.Compare(tgt?.Schemas.Get(k), cur?.Schemas.Get(k))
        );

        return diffs1.ToArray();
    }
}
