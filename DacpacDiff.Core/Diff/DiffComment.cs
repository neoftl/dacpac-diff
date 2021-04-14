using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
{
    public class DiffComment : IDifference
    {
        public IModel? Model => null;
        public string? Title => null;
        public string Name => null;

        public string Comment { get; set; }
    }
}
