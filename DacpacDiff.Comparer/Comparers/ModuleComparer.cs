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

                return new IDifference[] { new DiffObjectDrop(rgt) };
            }

            var diffs = new List<IDifference>();

            if (rgt != null && lft.Type != rgt.Type)
            {
                diffs.Add(new DiffObjectDrop(rgt));
                rgt = null;
            }

            if (rgt is null)
            {
                // TODO: Clustered index must be only one per object

                var diffCreate = new DiffModuleCreate(lft);
                diffs.Add(diffCreate);
                if (diffCreate.NeedsStub)
                {
                    diffs.Add(new DiffModuleAlter(lft));
                }
            }
            else if (!lft.IsSimilarDefinition(rgt))
            {
                if (lft.Type == ModuleModel.ModuleType.INDEX)
                {
                    return new IDifference[]
                    {
                        new DiffObjectDrop(rgt),
                        new DiffModuleCreate(lft)
                    };
                }

                diffs.Add(new DiffModuleAlter(lft));
            }

            return diffs.ToArray();
        }
    }
}
