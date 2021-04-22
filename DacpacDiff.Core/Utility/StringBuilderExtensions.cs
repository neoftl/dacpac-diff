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

        public static StringBuilder EnsureLine(this StringBuilder sb, int num = 1)
        {
            if (sb.Length >= NL.Length * num)
            {
                for (var i = sb.Length - 1; num > 0; i -= NL.Length, --num)
                {
                    if (NL.Length == 2)
                    {
                        if (sb[i - 1] != NL[0] || sb[i] != NL[1])
                        {
                            break;
                        }
                    }
                    else if (sb[i] != NL[0])
                    {
                        break;
                    }
                }
                while (num-- > 0)
                {
                    sb.AppendLine();
                }
            }
            return sb;
        }
    }
}
