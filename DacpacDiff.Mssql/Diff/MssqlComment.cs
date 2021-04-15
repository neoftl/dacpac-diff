using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using System;
using System.Text;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlComment : IDiffFormatter
    {
        private readonly DiffComment _diff;

        public MssqlComment(DiffComment diff)
        {
            _diff = diff ?? throw new ArgumentNullException(nameof(diff));
        }

        public StringBuilder Format(StringBuilder sb, bool checkForDataLoss, bool prettyPrint)
        {
            if (_diff.Comment.Length > 0)
            {
                var res = "-- " + _diff.Comment.Replace("\r\n", "\r\n-- ");
                if (res.StartsWith("-- \r\n--"))
                {
                    res = res[3..];
                }
                sb.AppendLine(res);
            }
            return sb;
        }

        public override string ToString() => _diff.Comment;
    }
}
