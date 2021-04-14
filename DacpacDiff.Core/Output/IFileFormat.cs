using DacpacDiff.Core.Diff;
using System.Collections.Generic;

namespace DacpacDiff.Core.Output
{
    public interface IFileFormat
    {
        string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<IDifference> diffs, bool withDataLossCheck, bool flat = true);
    }
}
