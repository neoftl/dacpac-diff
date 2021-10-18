using DacpacDiff.Core.Changes;
using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class ModuleComparer : IModelComparer<ModuleModel>
    {
        public IEnumerable<IDifference> Compare(ModuleModel? lft, ModuleModel? rgt)
        {
            if (lft is null)
            {
                if (rgt is null)
                {
                    return Array.Empty<IDifference>();
                }

                // Dropped
                return new IDifference[] { new DiffObjectDrop(rgt) };
            }
            if (rgt is null)
            {
                // Create
                // TODO: Can only have one clustered index per object
                return new IDifference[] { new DiffModuleCreate(lft) };
            }

            // Type changing, full recreate
            if (lft.Type != rgt.Type)
            {
                return new IDifference[] { new RecreateObject<ModuleModel>(lft, rgt) };
            }

            // Definition change, alter
            if (!lft.IsSimilarDefinition(rgt))
            {
                return new IDifference[] { new AlterObject<ModuleModel>(lft, rgt) };
            }

            return Array.Empty<IDifference>();
        }
    }
}
