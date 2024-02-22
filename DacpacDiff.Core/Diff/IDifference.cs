using DacpacDiff.Core.Model;
using DacpacDiff.Core.Output;

namespace DacpacDiff.Core.Diff;

public interface IDifference : ISqlFormattable
{
    IModel? Model { get; }
}

// When is a change not a change?
public interface INoopDifference : IDifference
{ }
