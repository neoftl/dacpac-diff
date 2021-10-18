using System;
using System.Collections.Generic;

namespace DacpacDiff.Core.Output
{
    public interface ISqlFileBuilder
    {
        IOutputOptions? Options { get; set; }

        string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<ISqlFormattable> objs);

        ISqlFileBuilder Append(char value);
        ISqlFileBuilder Append(string? value);
        ISqlFileBuilder AppendFormat(string format, params object?[] args);
        ISqlFileBuilder AppendGo();
        ISqlFileBuilder AppendIf(Func<string?> value, bool condition);
        ISqlFileBuilder AppendLine();
        ISqlFileBuilder AppendLine(string? value);
        ISqlFileBuilder EnsureLine(int num = 1);

        string Flatten(string? sql, bool? flat = null);
    }
}
