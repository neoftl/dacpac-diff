using DacpacDiff.Core.Output;
using System;

namespace DacpacDiff.Mssql.Diff
{
    public class MssqlComment : ISqlFormatter
    {
        private readonly SqlComment _diff;

        public MssqlComment(SqlComment sql)
        {
            _diff = sql ?? throw new ArgumentNullException(nameof(sql));
        }

        public void Format(ISqlFileBuilder sb)
        {
            if (_diff.Comment.Length > 0)
            {
                var res = "-- " + _diff.Comment.Trim().Replace("\r\n", "\r\n-- ");
                sb.AppendLine(res);
            }
        }

        public override string ToString() => _diff.Comment;
    }
}
