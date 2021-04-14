using System.Text;

namespace DacpacDiff.Core.Utility
{
    public static class StringBuilderExtensions
    {
        public static readonly string NL = System.Environment.NewLine;

        public static StringBuilder EnsureLine(this StringBuilder sb)
        {
            if (sb[^1] != NL[^1]
                || (NL.Length == 2 && sb[^2] != NL[^2]))
            {
                sb.AppendLine();
            }
            return sb;
        }
    }
}
