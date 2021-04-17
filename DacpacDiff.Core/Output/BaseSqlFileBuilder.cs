using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DacpacDiff.Core.Output
{
    public abstract class BaseSqlFileBuilder : ISqlFileBuilder
    {
        public IOutputOptions? Options { get; set; }

        protected StringBuilder _sql = new StringBuilder();

        public ISqlFileBuilder Append(char value)
        {
            _sql.Append(value);
            return this;
        }
        public ISqlFileBuilder Append(string? value)
        {
            _sql.Append(value);
            return this;
        }
        public ISqlFileBuilder AppendFormat(string format, params object?[] args)
        {
            _sql.AppendFormat(format, args);
            return this;
        }
        public ISqlFileBuilder AppendGo()
        {
            _sql.EnsureLine().AppendLine("GO");
            return this;
        }
        public ISqlFileBuilder AppendIf(string? value, bool condition)
        {
            _sql.AppendIf(value, condition);
            return this;
        }
        public ISqlFileBuilder AppendLine()
        {
            _sql.AppendLine();
            return this;
        }
        public ISqlFileBuilder AppendLine(string? value)
        {
            _sql.AppendLine(value); return this;
        }
        public ISqlFileBuilder EnsureLine()
        {
            _sql.EnsureLine();
            return this;
        }

        public virtual string Flatten(string? sql, bool? flat = null)
        {
            if (flat == true || Options?.PrettyPrint != true)
            {
                sql = sql?.Replace(Environment.NewLine, " ")
                    .ReplaceAll("  ", " ");
            }
            return sql ?? string.Empty;
        }

        public abstract string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<ISqlFormattable> diffs);
    }
}
