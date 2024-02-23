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

    public IEnumerable<IDifference> Compare(SchemaModel? tgt, SchemaModel? cur)
    {
        var result = new List<IDifference>();

        // May be a drop/create
        if (tgt is null)
        {
            if (cur is null)
            {
                return Array.Empty<IDifference>();
            }

            result.Add(new DiffObjectDrop(cur));
        }
        else if (cur is null)
        {
            result.Add(new DiffSchemaCreate(tgt));
        }

        // Modules
        var keys = (tgt?.Modules.Keys ?? Array.Empty<string>())
            .Union(cur?.Modules.Keys ?? Array.Empty<string>())
            .Distinct();
        var modCompr = _comparerFactory.GetComparer<ModuleModel>();
        var diffs = keys.SelectMany(k =>
            modCompr.Compare(tgt?.Modules.Get(k), cur?.Modules.Get(k))
        );
        result.AddRange(diffs);

        // Synonyms
        keys = (tgt?.Synonyms.Keys ?? Array.Empty<string>())
            .Union(cur?.Synonyms.Keys ?? Array.Empty<string>())
            .Distinct();
        var synCompr = _comparerFactory.GetComparer<SynonymModel>();
        diffs = keys.SelectMany(k =>
            synCompr.Compare(tgt?.Synonyms.Get(k), cur?.Synonyms.Get(k))
        );
        result.AddRange(diffs);

        // Tables
        keys = (tgt?.Tables.Keys ?? Array.Empty<string>())
            .Union(cur?.Tables.Keys ?? Array.Empty<string>())
            .Distinct();
        var tblCompr = _comparerFactory.GetComparer<TableModel>();
        diffs = keys.SelectMany(k =>
            tblCompr.Compare(tgt?.Tables.Get(k), cur?.Tables.Get(k))
        );
        result.AddRange(diffs);

        // User Types
        keys = (tgt?.UserTypes.Keys ?? Array.Empty<string>())
            .Union(cur?.UserTypes.Keys ?? Array.Empty<string>())
            .Distinct();
        var utCompr = _comparerFactory.GetComparer<UserTypeModel>();
        diffs = keys.SelectMany(k =>
            utCompr.Compare(tgt?.UserTypes.Get(k), cur?.UserTypes.Get(k))
        );
        result.AddRange(diffs);

        return result.ToArray();
    }
}
