using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
{
    public interface IDifference
    {
        IModel? Model { get; }
        string? Title { get; }
        string Name { get; }
    }
}
