using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DacpacDiff.Core.Output
{
    public abstract class BaseSqlFileBuilder : ISqlFileBuilder
    {
        public IOutputOptions? Options { get; set; }

        protected readonly StringBuilder _sql = new();

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
        public ISqlFileBuilder EnsureLine(int num = 1)
        {
            _sql.EnsureLine(num);
            return this;
        }

        public virtual string Flatten(string? sql, bool? flat = null)
        {
            if (sql != null && (flat == true || Options?.PrettyPrint != true))
            {
                sql = string.Join("", sql.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim().Trim(';'))
                    .Select(l =>
                    {
                        if (l == "GO")
                        {
                            return Environment.NewLine + l + Environment.NewLine;
                        }
                        if (l.StartsWith("--"))
                        {
                            return l + Environment.NewLine;
                        }
                        return $"{l}; ";
                    }));
                sql = string.Join(Environment.NewLine, sql.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim().Trim(';')));
            }
            return sql ?? string.Empty;
        }

        public abstract string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<ISqlFormattable> diffs);

        public override string ToString() => _sql.ToString();
    }
}
