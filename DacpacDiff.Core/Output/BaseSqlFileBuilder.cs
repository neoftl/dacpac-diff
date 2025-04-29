using DacpacDiff.Core.Utility;
using System.Text;

namespace DacpacDiff.Core.Output;

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
    public ISqlFileBuilder AppendIf(Func<string?> value, bool condition)
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

    public ISqlFileBuilder Remove(int count)
    {
        count = Math.Min(count, _sql.Length);
        _sql.Remove(_sql.Length - count, count);
        return this;
    }

    public virtual string Flatten(string? sql, bool? flat = null)
    {
        if (sql != null && (flat == true || Options?.PrettyPrint != true))
        {
            sql = string.Join("", sql.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim().Trim(';'))
                .Select(l =>
                {
                    if (l is "GO")
                    {
                        return Environment.NewLine + l + Environment.NewLine;
                    }
                    if (l == "BEGIN" || l.EndsWith(" BEGIN"))
                    {
                        return $"{l} ";
                    }

                    var wd = l.Split(' ', 2).First();
                    return wd switch
                    {
                        "--" => l + Environment.NewLine,
                        "AS" or "CREATE" or "END" or "RETURNS" => $"{l} ",
                        _ => $"{l}; "
                    };
                }));
            sql = string.Join(Environment.NewLine, sql.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim().Trim(';')));
        }
        return sql ?? string.Empty;
    }

    public abstract string Generate(string targetFileName, string currentFileName, string targetVersion, IEnumerable<ISqlFormattable> diffs);

    public override string ToString() => _sql.ToString();
}
