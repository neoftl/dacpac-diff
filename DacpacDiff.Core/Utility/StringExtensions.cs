using System.Text.RegularExpressions;

namespace DacpacDiff.Core.Utility
{
    public static class StringExtensions
    {
        public static string ReplaceAll(this string input, string oldValue, string newValue)
        {
            string result;
            do
            {
                result = input.Replace(oldValue, newValue);
            } while (result != input);
            return result;
        }

        public static bool TryMatch(this string input, string pattern, out Match match)
        {
            match = Regex.Match(input, pattern);
            return match.Success;
        }
    }
}
