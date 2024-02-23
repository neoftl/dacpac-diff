using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Comparers;

public class FieldComparer : IModelComparer<FieldModel>
{
    public IEnumerable<IDifference> Compare(FieldModel? tgt, FieldModel? cur)
    {
        // May be a drop/create
        if (tgt is null)
        {
            return cur is null
                ? Array.Empty<IDifference>()
                : new[] { new DiffFieldDrop(cur) };
        }
        if (cur is null)
        {
            return new[] { new DiffFieldCreate(tgt) };
        }

        // May be a change
        return tgt.Equals(cur)
            ? Array.Empty<IDifference>()
            : new[] { new DiffFieldAlter(tgt, cur) };
    }
}
