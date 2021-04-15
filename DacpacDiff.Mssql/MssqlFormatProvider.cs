using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using IFormatProvider = DacpacDiff.Core.IFormatProvider;

namespace DacpacDiff.Mssql
{
    public class MssqlFormatProvider : IFormatProvider
    {
        public string FormatName => "mssql";

        private readonly IDictionary<Type, Func<ISqlFormattable, ISqlFormatter>> _sqlFormatters = new Dictionary<Type, Func<ISqlFormattable, ISqlFormatter>>();

        public MssqlFormatProvider()
        {
            // Find all formatters
            var sqlFormatters = GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(ISqlFormatter).IsAssignableFrom(t))
                .ToArray();
            _sqlFormatters.Merge(sqlFormatters, getFormattableType, t => (f) => Activator.CreateInstance(t, new object[] { f }) as ISqlFormatter ?? throw new NullReferenceException());

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

        public ISqlFileBuilder GetSqlFileBuilder() => new MssqlFileBuilder(this);

        public ISqlFormatter GetSqlFormatter(ISqlFormattable sqlObj)
        {
            if (_sqlFormatters.TryGetValue(sqlObj.GetType(), out var formatter))
            {
                return formatter(sqlObj);
            }

            throw new NotImplementedException($"No SQL formatter registered for type: {sqlObj.GetType().FullName}");
        }
    }
}
