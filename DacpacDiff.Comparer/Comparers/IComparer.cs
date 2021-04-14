using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;
using System.Collections.Generic;

namespace DacpacDiff.Comparer.Comparers
{
    public interface IComparer<T>
        where T : IModel
    {
        IEnumerable<IDifference> Compare(T? lft, T? rgt);
    }
}
