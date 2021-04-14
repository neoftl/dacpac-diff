using DacpacDiff.Mssql;
using System;
using System.Collections.Generic;
using IFormatProvider = DacpacDiff.Core.IFormatProvider;

namespace DacpacDiff.Comparer
{
    public class FormatProviderFactory
    {
        // TODO: Resolve from assemblies
        private static readonly IDictionary<string, Func<IFormatProvider>> _formatProviders = new Dictionary<string, Func<IFormatProvider>>
        {
            [new MssqlFormatProvider().FormatName] = () => new MssqlFormatProvider()
        };

        public static IFormatProvider GetFormat(string format)
        {
            if (_formatProviders.TryGetValue(format, out var provider))
            {
                return provider();
            }

            throw new NotImplementedException($"Unregistered format: {format}");
        }
    }
}
