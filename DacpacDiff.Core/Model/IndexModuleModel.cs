using DacpacDiff.Core.Utility;
using System.Runtime.Intrinsics.X86;

namespace DacpacDiff.Core.Model;

public class IndexModuleModel : ModuleModel
{
    public bool IsClustered { get; set; }
    public bool IsUnique { get; set; }

    public string IndexedObjectFullName { get; init; } = string.Empty;
    public IModel? IndexedObject { get; private set; }

    public string[] IndexedColumns { get; set; } = Array.Empty<string>();
    public string[] IncludedColumns { get; set; } = Array.Empty<string>();
    public IEnumerable<string> AllColumns => IndexedColumns.Concat(IncludedColumns);
    public string? Condition { get; set; }

    public override string[] Dependencies => IndexedColumns.Concat(IncludedColumns).Select(c => $"{IndexedObjectFullName}.[{c}]").ToArray();

    public IndexModuleModel(SchemaModel schema, string name)
        : base(schema, name, ModuleType.INDEX)
    {
    }

    public override bool IsSimilarDefinition(ModuleModel other)
    {
        if (other is not IndexModuleModel idx)
        {
            return false;
        }

        return this.IsEqual(idx,
            m => m.IsClustered,
            m => m.IsUnique,
            m => m.IndexedObjectFullName,
            m => m.IndexedColumns,
            m => m.IncludedColumns,
            m => m.Condition?.ScrubSQL());
    }

    public bool MapTarget(DatabaseModel db)
    {
        IndexedObject = db.Get(IndexedObjectFullName);
        if (IndexedObject == null)
        {
            return false;
        }

        if (IndexedObject is TableModel tbl)
        {
            return IndexedColumns.All(c => tbl.Fields.Any(f => f.Name == c))
                && IncludedColumns.All(c => tbl.Fields.Any(f => f.Name == c));
        }

        throw new NotImplementedException("Index check against " + IndexedObject.GetType().FullName);
    }
}
