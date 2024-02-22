using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;

namespace DacpacDiff.Comparer.Comparers;

public class SchemaComparer : IModelComparer<SchemaModel>
{
    private readonly IModelComparerFactory _comparerFactory;

    public SchemaComparer(IModelComparerFactory comparerFactory)
    {
        _comparerFactory = comparerFactory;
    }

    public IEnumerable<IDifference> Compare(SchemaModel? lft, SchemaModel? rgt)
    {
        var result = new List<IDifference>();

        // May be a drop/create
        if (lft is null)
        {
            if (rgt is null)
            {
                return Array.Empty<IDifference>();
            }

            result.Add(new DiffObjectDrop(rgt));
        }
        else if (rgt is null)
        {
            result.Add(new DiffSchemaCreate(lft));
        }

        // Modules
        var keys = (lft?.Modules.Keys ?? Array.Empty<string>())
            .Union(rgt?.Modules.Keys ?? Array.Empty<string>())
            .Distinct();
        var modCompr = _comparerFactory.GetComparer<ModuleModel>();
        var diffs = keys.SelectMany(k =>
            modCompr.Compare(lft?.Modules.Get(k), rgt?.Modules.Get(k))
        );
        result.AddRange(diffs);

        // Synonyms
        keys = (lft?.Synonyms.Keys ?? Array.Empty<string>())
            .Union(rgt?.Synonyms.Keys ?? Array.Empty<string>())
            .Distinct();
        var synCompr = _comparerFactory.GetComparer<SynonymModel>();
        diffs = keys.SelectMany(k =>
            synCompr.Compare(lft?.Synonyms.Get(k), rgt?.Synonyms.Get(k))
        );
        result.AddRange(diffs);

        // Tables
        keys = (lft?.Tables.Keys ?? Array.Empty<string>())
            .Union(rgt?.Tables.Keys ?? Array.Empty<string>())
            .Distinct();
        var tblCompr = _comparerFactory.GetComparer<TableModel>();
        diffs = keys.SelectMany(k =>
            tblCompr.Compare(lft?.Tables.Get(k), rgt?.Tables.Get(k))
        );
        result.AddRange(diffs);

        // User Types
        keys = (lft?.UserTypes.Keys ?? Array.Empty<string>())
            .Union(rgt?.UserTypes.Keys ?? Array.Empty<string>())
            .Distinct();
        var utCompr = _comparerFactory.GetComparer<UserTypeModel>();
        diffs = keys.SelectMany(k =>
            utCompr.Compare(lft?.UserTypes.Get(k), rgt?.UserTypes.Get(k))
        );
        result.AddRange(diffs);

        return result.ToArray();
    }
}
