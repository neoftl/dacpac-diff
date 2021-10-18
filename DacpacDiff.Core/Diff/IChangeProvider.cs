using System.Collections.Generic;

namespace DacpacDiff.Core.Diff
{
    /// <summary>
    /// If this difference is included, it may supply additional changes
    /// </summary>
    public interface IChangeProvider
    {
        IEnumerable<IDifference> GetAdditionalChanges();
    }
}
