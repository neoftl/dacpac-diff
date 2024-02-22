using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;
using System.Diagnostics.CodeAnalysis;
using IFormatProvider = DacpacDiff.Core.IFormatProvider;

namespace DacpacDiff.Mssql;

public class MssqlFormatProvider : IFormatProvider
{
    public string FormatName { get; } = "mssql";

    private readonly Dictionary<Type, Func<ISqlFormattable, ISqlFormatter>> _sqlFormatters = new();

    [ExcludeFromCodeCoverage(Justification = "Not currently possible to test; will need Shimterface")] // TODO
    internal void Initialise()
    {
        // Find all formatters in this DLL
        var sqlFormatters = GetType().Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(ISqlFormatter).IsAssignableFrom(t))
            .ToArray();
        _sqlFormatters.Merge(sqlFormatters, getFormattableType, t => (f) => (ISqlFormatter?)Activator.CreateInstance(t, new object[] { f }) ?? throw new InvalidCastException());

        static Type getFormattableType(Type t)
        {
            return t.GetConstructors()
                .Where(c => c.IsPublic)
                .Select(c => c.GetParameters())
                .Where(p => p.Length == 1)
                .Single(p => typeof(ISqlFormattable).IsAssignableFrom(p[0].ParameterType))
                [0].ParameterType;
        }
    }

    // TODO: some diff ordering and chaining logic should be provided based on target DBMS

    public ISqlFileBuilder GetSqlFileBuilder() => new MssqlFileBuilder(this);

    public ISqlFormatter GetSqlFormatter(ISqlFormattable sqlObj)
    {
        if (_sqlFormatters.Count == 0)
        {
            Initialise();
        }

        if (_sqlFormatters.TryGetValue(sqlObj.GetType(), out var formatter))
        {
            return formatter(sqlObj);
        }

        throw new NotImplementedException($"No SQL formatter registered for type: {sqlObj.GetType().FullName}");
    }
}
