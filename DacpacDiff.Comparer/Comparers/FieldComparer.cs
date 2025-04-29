using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using DacpacDiff.Core.Utility;

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
                : [new DiffFieldDrop(cur)];
        }
        if (cur is null)
        {
            return [new DiffFieldCreate(tgt)];
        }

        // Resolve changes
        var changes = new List<DiffFieldAlter.Change>();
        checkChange(m => m.Type, DiffFieldAlter.Change.Type);
        checkChange(m => m.Collation, DiffFieldAlter.Change.Collation, DiffFieldAlter.Change.CollationUnset);
        checkChange(m => m.Computation?.ScrubSQL(), DiffFieldAlter.Change.Computed, DiffFieldAlter.Change.ComputedUnset);
        checkChange(m => m.DefaultValue?.ScrubSQL(), DiffFieldAlter.Change.Default, DiffFieldAlter.Change.DefaultUnset);
        checkChange(m => m.IsUnique, DiffFieldAlter.Change.Unique, DiffFieldAlter.Change.UniqueUnset);
        checkChange(m => m.Nullable, DiffFieldAlter.Change.Nullable, DiffFieldAlter.Change.NullableUnset);
        checkChange(m => m.Identity, DiffFieldAlter.Change.Identity, DiffFieldAlter.Change.IdentityUnset);
        checkChange(m => m.Ref, DiffFieldAlter.Change.Reference, DiffFieldAlter.Change.ReferenceUnset);

        // May be a change
        return changes.Count == 0
            ? Array.Empty<IDifference>()
            : [new DiffFieldAlter(tgt, cur) { Changes = changes.ToArray() }];

        void checkChange<T>(Func<FieldModel, T?> fn, DiffFieldAlter.Change change, DiffFieldAlter.Change? unset = null)
        {
            var l = fn(tgt);
            var r = fn(cur);
            if (l is null || ((l is bool bl) && !bl && (r is bool br) && br))
            {
                if (r is not null)
                {
                    changes.Add(unset ?? change);
                }
            }
            else if (!l.Equals(r))
            {
                changes.Add(change);
            }
        }
    }
}
