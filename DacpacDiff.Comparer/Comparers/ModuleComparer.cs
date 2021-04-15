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
            var diffs = new List<IDifference>();
            if (lft is null)
            {
                if (rgt is null)
                {
                    return Array.Empty<IDifference>();
                }

                diffs.Add(new DiffObjectDrop(rgt));
                return diffs.ToArray();
            }

            if (rgt is not null && lft.Type != rgt.Type)
            {
                diffs.Add(new DiffObjectDrop(rgt));
                rgt = null;
            }

            DiffModuleCreate? diffCreate = null;
            if (rgt is null)
            {
                // TODO: Clustered index must be only one per object

                diffCreate = new DiffModuleCreate(lft);
                diffs.Add(diffCreate);
            }
            else if (!lft.IsSimilarDefinition(rgt))
            {
                if (lft.Type == ModuleModel.ModuleType.INDEX)
                {
                    diffs.Add(new DiffObjectDrop(rgt));
                    diffCreate = new DiffModuleCreate(lft);
                    diffs.Add(diffCreate);
                }
                else
                {
                    diffs.Add(new DiffModuleAlter(lft));
                }
            }
            if (diffCreate?.NeedsStub ?? false)
            {
                diffs.Add(new DiffModuleAlter(lft));
            }

            return diffs.ToArray();
        }
    }
}
