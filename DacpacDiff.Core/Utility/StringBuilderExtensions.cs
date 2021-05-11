using System.Text;

namespace DacpacDiff.Core.Utility
{
    internal static class StringBuilderExtensions
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

        public static StringBuilder EnsureLine(this StringBuilder sb, int num = 1, string? nl = null)
        {
            nl ??= NL;
            if (sb.Length >= nl.Length)
            {
                for (var i = sb.Length - 1; num > 0; i -= nl.Length, --num)
                {
                    if (nl.Length == 2)
                    {
                        if (sb[i - 1] != nl[0] || sb[i] != nl[1])
                        {
                            break;
                        }
                    }
                    else if (sb[i] != nl[0])
                    {
                        break;
                    }
                }
            }
            while (num-- > 0)
            {
                sb.Append(nl);
            }
            return sb;
        }
    }
}
