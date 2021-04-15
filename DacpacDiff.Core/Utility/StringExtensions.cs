using System.Text.RegularExpressions;

namespace DacpacDiff.Core.Utility
{
    public static class StringExtensions
    {
        public static string ReplaceAll(this string input, string oldValue, string newValue)
        {
            while (input.Contains(oldValue))
            {
                input = input.Replace(oldValue, newValue);
            }
            return input;
        }

        public static bool TryMatch(this string input, string pattern, out Match match)
        {
            match = Regex.Match(input, pattern);
            return match.Success;
        }
    }
}
