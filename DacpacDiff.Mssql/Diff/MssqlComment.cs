using DacpacDiff.Core.Output;
using System;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlComment : ISqlFormatter
    {
        private readonly SqlComment _diff;

        public MssqlComment(SqlComment diff)
        {
            _diff = diff ?? throw new ArgumentNullException(nameof(diff));
        }

        public void Format(ISqlFileBuilder sb)
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
        }

        public override string ToString() => _diff.Comment;
    }
}
