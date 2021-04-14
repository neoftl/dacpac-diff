using System.Text.RegularExpressions;

namespace DacpacDiff.Core.Utility
{
    public static class RegexExtensions
    {
        public static bool TryMatch(this string input, string pattern, out Match match)
        {
            match = Regex.Match(input, pattern);
            return match.Success;
        }
    }
}
