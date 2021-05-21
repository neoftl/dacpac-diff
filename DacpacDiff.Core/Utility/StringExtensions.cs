using System.Text.RegularExpressions;

namespace DacpacDiff.Core.Utility
{
    internal static class StringExtensions
    {
        public static string Format(this string format, params object[] args) => string.Format(format, args);

        /// <summary>
        /// Remove unnecessary parenthesis from well-formed SQL.
        /// </summary>
        /// <example>"(a)" => "a", but "(a),(b)" is unchanged</example>
        public static string ReduceBrackets(this string sql)
        {
            // Must start and end in brackes
            if (sql.Length < 2 || sql[0] != '(' || sql[^1] != ')')
            {
                return sql;
            }

            int startScore = 0, scoreAtLastChar = 0;
            var score = 1; // We know it starts '('
            var tsql = sql.Replace(" ", "");
            for (var i = 1; i < tsql.Length - 1; ++i)
            {
                var chr = tsql[i];
                if (chr == '(')
                {
                    ++score;
                }
                else if (chr == ')')
                {
                    if (--score == 0)
                    {
                        return sql; // No brackets to remove
                    }
                    if (score < startScore)
                    {
                        startScore = score;
                    }
                }
                else if (startScore == 0)
                {
                    startScore = score;
                    scoreAtLastChar = score;
                }
                else
                {
                    scoreAtLastChar = startScore;
                }
            }

            return sql[(scoreAtLastChar)..^(scoreAtLastChar)];
        }

        /// <summary>
        /// Makes SQL easier to compare by removing subjective characters.
        /// </summary>
        public static string ScrubSQL(this string sql)
        {
            return sql.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "")
                .Replace("(", "").Replace(")", "")
                .Replace("[", "").Replace("]", "")
                .ToLower();
        }

        public static bool TryMatch(this string input, string pattern, out Match match)
        {
            match = Regex.Match(input, pattern);
            return match.Success;
        }
    }
}
