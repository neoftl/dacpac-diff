using DacpacDiff.Core.Changes;
using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Comparers;

public class ModuleComparer : IModelComparer<ModuleModel>
{
    public IEnumerable<IDifference> Compare(ModuleModel? lft, ModuleModel? rgt)
    {
        if (lft is null)
        {
            return rgt is null
                ? Array.Empty<IDifference>()
                : new[] { new DiffObjectDrop(rgt) }; // Dropped
        }
        if (rgt is null)
        {
            // Create
            // TODO: Can only have one clustered index per object
            return new[] { new DiffModuleCreate(lft) };
        }

        // Type changing, full recreate
        if (lft.Type != rgt.Type)
        {
            return new[] { new RecreateObject<ModuleModel>(lft, rgt) };
        }

        // Definition change, alter
        return lft.IsSimilarDefinition(rgt)
            ? Array.Empty<IDifference>()
            : new[] { new AlterObject<ModuleModel>(lft, rgt) };
    }
}
