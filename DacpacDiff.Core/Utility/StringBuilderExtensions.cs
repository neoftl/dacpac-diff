using System.Text;

namespace DacpacDiff.Core.Utility
{
    public static class StringBuilderExtensions
    {
        public static readonly string NL = System.Environment.NewLine;

        public static StringBuilder AppendIf(this StringBuilder sb, string? value, bool condition)
        {
            if (condition)
            {
                sb.Append(value);
            }
            return sb;
        }

        public static StringBuilder EnsureLine(this StringBuilder sb)
        {
            if (sb.Length >= NL.Length
                && (sb[^1] != NL[^1] || (NL.Length == 2 && sb[^2] != NL[^2])))
            {
                sb.AppendLine();
            }
            return sb;
        }
    }
}
