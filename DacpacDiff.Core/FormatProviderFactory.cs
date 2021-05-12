using DacpacDiff.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DacpacDiff.Core
{
    public class FormatProviderFactory
    {
        private static readonly IDictionary<string, Func<IFormatProvider>> _formatProviders = new Dictionary<string, Func<IFormatProvider>>();

        internal static void Initialise()
        {
            // Find all assemblies referencing DacpacDiff.Core
            var dacpacDiffAsm = typeof(IFormatProvider).Assembly;
            var dlls = new FileInfo(dacpacDiffAsm.Location).Directory?.GetFiles("*.dll") ?? Array.Empty<FileInfo>();
            var asms = dlls.Select(d => Assembly.LoadFrom(d.FullName)) // NOTE: ReflectionOnlyLoad not supported
                .Where(a => a.GetReferencedAssemblies().Any(r => r.FullName == dacpacDiffAsm.FullName))
                .ToArray();

            // Find format providers
            var formatProviders = asms.SelectMany(a => a.GetTypes())
                .Where(t => typeof(IFormatProvider).IsAssignableFrom(t))
                .ToArray();

            // Build map
            _formatProviders.Merge(formatProviders.Select(p => (IFormatProvider?)Activator.CreateInstance(p)).Cast<IFormatProvider>()
                .ToDictionary(p => p.FormatName, (Func<IFormatProvider, Func<IFormatProvider>>)(p => () => p)));
        }

        public static IFormatProvider GetFormat(string format)
        {
            if (_formatProviders.Count == 0)
            {
                Initialise();
            }

            if (_formatProviders.TryGetValue(format, out var provider))
            {
                return provider();
            }

            throw new NotImplementedException($"Unregistered format: {format}");
        }
    }
}
