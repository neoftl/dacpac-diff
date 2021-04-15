using System.Collections.Generic;

namespace DacpacDiff.Core.Output
{
    public interface ISqlFileBuilder
    {
        bool DataLossChecks { get; set; }
        bool PrettyPrint { get; set; }

        string Generate(string leftFileName, string rightFileName, string targetVersion, IEnumerable<ISqlFormattable> objs);

        ISqlFileBuilder Append(char value);
        ISqlFileBuilder Append(string? value);
        ISqlFileBuilder AppendFormat(string format, params object?[] args);
        ISqlFileBuilder AppendGo();
        ISqlFileBuilder AppendIf(string? value, bool condition);
        ISqlFileBuilder AppendLine();
        ISqlFileBuilder AppendLine(string? value);
        ISqlFileBuilder EnsureLine();

        string Flatten(string? sql, bool flat = true);
    }
}
