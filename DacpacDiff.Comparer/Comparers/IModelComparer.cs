using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public interface IModelComparer<T> : IModelComparer
        where T : IModel
    {
        IEnumerable<IDifference> Compare(T? tgt, T? cur);
    }

    public interface IModelComparer
    {
    }
}
