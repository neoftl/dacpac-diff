using DacpacDiff.Core.Diff;
using DacpacDiff.Core.Output;
using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IFormatProvider = DacpacDiff.Core.IFormatProvider;

namespace DacpacDiff.Mssql
{
    public class MssqlFormatProvider : IFormatProvider
    {
        public string FormatName => "mssql";

        private readonly IDictionary<Type, Func<IDifference, IDiffFormatter>> _diffFormatters = new Dictionary<Type, Func<IDifference, IDiffFormatter>>();

        public MssqlFormatProvider()
        {
            // Find all diff formatters
            var diffFormatters = GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(IDiffFormatter).IsAssignableFrom(t))
                .ToArray();
            _diffFormatters.Merge(diffFormatters, t => getDiffTypeFromConstructor(t.GetConstructors()), t => (d) => Activator.CreateInstance(t, new object[] { d }) as IDiffFormatter ?? throw new NullReferenceException());

            static Type getDiffTypeFromConstructor(ConstructorInfo[] constructors)
            {
                return constructors.Where(c => c.IsPublic)
                    .Select(c => c.GetParameters())
                    .Where(p => p.Length == 1)
                    .Single(p => typeof(IDifference).IsAssignableFrom(p[0].ParameterType))
                    [0].ParameterType;
            }
        }

        public IFileFormat GetOutputGenerator() => new MssqlFileFormat(this);

        public IDiffFormatter GetDiffFormatter(IDifference diff)
        {
            if (_diffFormatters.TryGetValue(diff.GetType(), out var formatter))
            {
                return formatter(diff);
            }

            throw new NotImplementedException($"No diff formatter registered for diff type: {diff.GetType().FullName}");
        }
    }
}
