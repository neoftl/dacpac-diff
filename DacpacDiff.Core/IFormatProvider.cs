using DacpacDiff.Core.Output;

namespace DacpacDiff.Core
{
    public interface IFormatProvider
    {
        string FormatName { get; }

        ISqlFormatter GetSqlFormatter(ISqlFormattable sqlObj);
        ISqlFileBuilder GetSqlFileBuilder();
    }
}