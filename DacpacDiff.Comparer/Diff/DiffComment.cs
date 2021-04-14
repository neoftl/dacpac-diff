using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Model;

namespace DacpacDiff.Comparer.Diff
{
    public class DiffComment : IDifference
    {
        public IModel? Model => null;
        public string? Title => null;
        public string Name => null;

        public string Comment { get; set; }

        public override string ToString()
        {
            if (Comment.Length == 0)
            {
                return string.Empty;
            }
            var res = "-- " + Comment.Replace("\r\n", "\r\n-- ");
            if (res.StartsWith("-- \r\n--"))
            {
                res = res[3..];
            }
            return res;
        }
    }
}
