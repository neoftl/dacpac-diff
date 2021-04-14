using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Core
{
    public interface IFormatProvider
    {
        string FormatName { get; }

        IDiffFormatter GetDiffFormatter(IDifference diff);
        IFileFormat GetOutputGenerator();
    }
}