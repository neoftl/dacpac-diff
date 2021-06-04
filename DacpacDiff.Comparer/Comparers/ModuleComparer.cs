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

            var diffs = new List<IDifference>();

            // Type changing, drop existing
            if (rgt != null && lft.Type != rgt.Type)
            {
                diffs.Add(new DiffObjectDrop(rgt));
                rgt = null;
            }

            if (rgt is null)
            {
                // Create
                // TODO: Clustered index must be only one per object
                diffs.Add(new DiffModuleCreate(lft));
            }
            else if (!lft.IsSimilarDefinition(rgt))
            {
                // Alter index is a recreate
                if (lft.Type == ModuleModel.ModuleType.INDEX)
                {
                    return new IDifference[] { new RecreateObject<ModuleModel>(lft, rgt) };
                }

                // Alter other module types
                diffs.Add(new RecreateObject<ModuleModel>(lft, rgt));
            }

            return diffs;
        }
    }
}
