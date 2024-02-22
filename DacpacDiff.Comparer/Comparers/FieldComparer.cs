using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Comparers;

public class FieldComparer : IModelComparer<FieldModel>
{
    public IEnumerable<IDifference> Compare(FieldModel? lft, FieldModel? rgt)
    {
        // May be a drop/create
        if (lft is null)
        {
            return rgt is null
                ? Array.Empty<IDifference>()
                : new[] { new DiffFieldDrop(rgt) };
        }
        if (rgt is null)
        {
            return new[] { new DiffFieldCreate(lft) };
        }

        // May be a change
        return lft.Equals(rgt)
            ? Array.Empty<IDifference>()
            : new[] { new DiffFieldAlter(lft, rgt) };
    }
}
