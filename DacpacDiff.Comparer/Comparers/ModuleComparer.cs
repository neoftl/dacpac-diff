using DacpacDiff.Core.Changes;
using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Comparers;

public class ModuleComparer : IModelComparer<ModuleModel>
{
    public IEnumerable<IDifference> Compare(ModuleModel? tgt, ModuleModel? cur)
    {
        if (tgt is null)
        {
            return cur is null
                ? Array.Empty<IDifference>()
                : new[] { new DiffObjectDrop(cur) }; // Dropped
        }
        if (cur is null)
        {
            // Create
            // TODO: Can only have one clustered index per object
            return new[] { new DiffModuleCreate(tgt) };
        }

        // Type changing, full recreate
        if (tgt.Type != cur.Type)
        {
            return new[] { new RecreateObject<ModuleModel>(tgt, cur) };
        }

        // Definition change, alter
        return tgt.IsSimilarDefinition(cur)
            ? Array.Empty<IDifference>()
            : new[] { new AlterObject<ModuleModel>(tgt, cur) };
    }
}
