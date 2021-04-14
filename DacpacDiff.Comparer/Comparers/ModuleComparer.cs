using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public class ModuleComparer : IComparer<ModuleModel>
	{
        public IEnumerable<IDifference> Compare(ModuleModel? lft, ModuleModel? rgt)
		{
			var diffs = new List<IDifference>();
			if (lft == null)
			{
				diffs.Add(new DiffObjectDrop(rgt));
			}
			else
			{
				if (rgt != null && lft.Type != rgt.Type)
				{
					diffs.Add(new DiffObjectDrop(rgt));
					rgt = null;
				}

				DiffModuleCreate? diffCreate = null;
				if (rgt == null)
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
			}

			return diffs.ToArray();
		}
	}
}
